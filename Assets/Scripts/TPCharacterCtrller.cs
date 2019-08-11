using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    第三人称角色控制
     */
namespace HHY
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class TPCharacterCtrller : MonoBehaviour
    {
        public bool RootMotion = false;
        private Animator animator;
        //public Transform camTrans;
        //调节属性
        [Range(1f, 20f)] public float turnSpeed;    //跟随相机的转向速度
        [SerializeField] float m_MovingTurnSpeed = 360;
        [SerializeField] float m_StationaryTurnSpeed = 180;
        [SerializeField] float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
        [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [SerializeField] float m_GroundCheckDistance = 0.1f;    //检测离地面的距离

        public bool ___viewParams___;
        [SerializeField] bool m_IsGrounded;      //在地面
        float m_TurnAmount;     //转向值
        float m_ForwardAmount;  //前向值
        float m_OrigGroundCheckDistance;
        const float k_Half = 0.5f;

        float m_CapsuleHeight;
        Vector3 m_CapsuleCenter;
        bool m_Crouching;

        //属性 
        private Rigidbody m_Rigidbody;
        private CapsuleCollider m_Capsule;
        Vector3 m_GroundNormal; //地面的法向量


        private void Reset()
        {
            turnSpeed = 15f;    //实验结果
            m_GroundCheckDistance = 0.3f;   //靠近Character位置往下一段的距离，用于判断是否位于地面
            m_StationaryTurnSpeed = 180;    //静止时转向速度
            m_MovingTurnSpeed = 360;        //移动时的转向速度
        }

        // Use this for initialization
        void Start()
        {
            animator = this.GetComponent<Animator>();
            m_Rigidbody = this.GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();

            //冻结旋转，否则模型无法移动且会进行翻滚
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            m_OrigGroundCheckDistance = m_GroundCheckDistance;
            m_Rigidbody.useGravity = false;
        }

        public void OnAnimatorMove()    //在每一帧进行回调处理动画
        {
            if (m_IsGrounded && Time.deltaTime > 0)
            {
                Vector3 v = (animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }

        private void ExtraTurn() {
            //旋转辅助
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);//范围：[180,360]
            transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);  //y轴的旋转
        }
        /*
         * params：
         *      move: 移动向量，描述世界坐标系下的移动方向
         *      walk，是否走路
         *      jump，是否跳跃
         */
        public void Move(Vector3 move, bool walk, bool jump)
        {
            if (move.magnitude > 1f) move.Normalize();//首先标准化move向量
            //将move从世界坐标系转换到本地坐标系
            move = transform.InverseTransformDirection(move);   
            CheckGroundStatus();    //检测是否在地面
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);//将移动向量投影到平面上。
            //print("local move:" + move);
            m_TurnAmount = Mathf.Atan2(move.x, move.z); //获取偏离z轴方向的弧度值
            m_ForwardAmount = move.z;
            if (walk) {
                m_ForwardAmount *= 0.5f;
            }
            // control and velocity handling is different when grounded and airborne:
            if (m_IsGrounded)   //当在地面时，调用地面上的移动，否则执行空中的
            {
                if (jump && animator.GetCurrentAnimatorStateInfo(0).IsName("Ground"))
                {
                    // jump! 根据Power属性给予一个向上的速度。
                    m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                    m_IsGrounded = false;
                    animator.applyRootMotion = false;
                    m_GroundCheckDistance = 0.1f;
                }
            }
            else
            {
                //在空中
                Vector3 GravityForce = (Physics.gravity * m_GravityMultiplier);
                m_Rigidbody.AddForce(GravityForce);
                m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
            }

            ExtraTurn();//辅助旋转
            UpdateMoveAnimator(move);
        }

        public void Fight(Vector3 move, bool aim, bool roll) {
            if (move.magnitude > 1f) move.Normalize();//首先标准化move向量
            //将move从世界坐标系转换到本地坐标系
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();    //检测是否在地面
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);//将移动向量投影到平面上。
            //print("local move:" + move);
            m_TurnAmount = Mathf.Atan2(move.x, move.z); //获取偏离z轴方向的弧度值
            m_ForwardAmount = move.z;
            if (aim)
            {
                m_ForwardAmount *= 0.5f;
            }
            // control and velocity handling is different when grounded and airborne:
            if (m_IsGrounded)   //当在地面时，调用地面上的移动，否则执行空中的
            {
                if (roll && animator.GetCurrentAnimatorStateInfo(0).IsName("Ground"))
                {
                    // jump! 根据Power属性给予一个向上的速度。
                    m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                    m_IsGrounded = false;
                    animator.applyRootMotion = false;
                    m_GroundCheckDistance = 0.1f;
                }
            }
            else
            {
                //翻滚中
                Vector3 GravityForce = (Physics.gravity * m_GravityMultiplier);
                m_Rigidbody.AddForce(GravityForce);
                m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
            }
            ExtraTurn();//辅助旋转
            UpdateFightAnimator(move);
        }

        void UpdateMoveAnimator(Vector3 move)
        {
            // update the animator parameters
            animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);  //在0.1s到达值m_ForwardAmount
            animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);        //在0.1s到达值m_TurnAmount                                                               //animator.SetBool("Crouch", m_Crouching);                            
            animator.SetBool("isGrounded", m_IsGrounded);
            if (!m_IsGrounded)
            {
                animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                animator.SetFloat("JumpLeg", jumpLeg);
            }

            //控制动画的播放速度
            if (m_IsGrounded && move.magnitude > float.Epsilon)
            {
                animator.speed = m_AnimSpeedMultiplier;   
            }
            else
            {   //空中时恢复
                animator.speed = 1;
            }
        }

        void UpdateFightAnimator(Vector3 move) {

        }
        /*
         * 检测是否在地面
         */
        void CheckGroundStatus()
        {
            RaycastHit hitInfo;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            //返回真时，位于地面，否则不在地面，
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
            {
                m_GroundNormal = hitInfo.normal;    //获取碰撞表面的法向量
                m_IsGrounded = true;
                animator.applyRootMotion = true;
            }
            else    //不在地面时，关闭applyRootMotion
            {
                m_IsGrounded = false;
                m_GroundNormal = Vector3.up;
                animator.applyRootMotion = false;
            }
        }

        public void Towards(Quaternion dir) {
            transform.rotation = Quaternion.Lerp(transform.rotation,dir, turnSpeed * Time.fixedDeltaTime);
        }

        public void SwitchMode(PlayerStatus status) {
            switch (status) {
                case PlayerStatus.Move:
                    animator.SetBool("Fighting",false);
                    break;
                case PlayerStatus.Fight:
                    animator.SetBool("Fighting", true);
                    break;
            }
        }
    }
}


