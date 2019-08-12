using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRCourse.GDCamera {

    public class FreeTPSCamera : MonoBehaviour
    {
        //该脚本用于自由视角的相机控制
        // 	Camera Rig      
        // 		Pivot   //轴点与相机的俯仰视角有关
        // 			Camera
        static public FreeTPSCamera S;
        public Transform viewTarget; //观察目标
        public GameObject viewCamera; //观察相机

        //相机的可调节属性
        /*
         * Basic Setup 基础属性调配
         */
        public bool ___BasicSetup___;
        [Range(0.5f ,2.5f)] [SerializeField] private float heightFromGround; //离地面的高度
        [Range(0f, 1f)] [SerializeField] private float sideOffset; //轴点的侧向偏移
        [Range(1f ,5f)] [SerializeField] private float distFromPlayer_norm;     //正常情况下离玩家的距离
        [Range(1f, 5f)] [SerializeField] private float distFromPlayer_aim;      //瞄准情况下离玩家的距离
        [Range(10f, 30f)] [SerializeField] private float initPitchAngle;    //初始的Pitch角度
        [SerializeField] private float pitchAngle_min; //最小俯仰角
        [SerializeField] private float pitchAngle_max; //最大俯仰角
        /*
         * 相机的移动调控属性
         */
        public bool ___Move___;
        [Range(5f, 10f)] [SerializeField] private float cam_MoveSpeed; //相机的移动速度，指跟进角色的速度
        [Range(5f, 10f)] [SerializeField] private float cam_MoveMaxSpeed; //相机移动的最大速度。
        //[Range(5f, 10f)] [HideInInspector] private float cam_MoveSmoothTime ; //相机移动的平滑时间
        private Vector3 curVelocity;
        /*
         * 相机的旋转动调控属性
         */
        public bool ___Turn___;
        [SerializeField] private bool reverseDir;    //反转方向
        [Range(0f, 10f)] [SerializeField] private float cam_TurnSpeed; //相机的转向速度，指鼠标移动时的转向速度
        [SerializeField] private float view_CurYawAngle; //观察的角度,y轴偏向
        [Range(0f, 15f)] [SerializeField] private float TurnSmooth; //转向的平滑处理
        [Range(0f, 5f)] [SerializeField] private float AudoTurnSmooth; //转向的平滑处理
        [SerializeField] private float view_CurPitchAxis; //观察的角度,x轴偏向方向，俯仰角
        
        Transform pivot; // the point at which the camera pivots around 相机的轴点
        private Vector3 pivotEulers;   //记录轴点的初始欧拉角度
        private Quaternion transTargetRot;
        private Quaternion pivotTargetRot;


        private void Reset()
        {
            reverseDir = false;
            viewTarget = GameObject.FindGameObjectWithTag("Player").transform;
            viewCamera = Camera.main.gameObject;
            //Basic Setup
            heightFromGround = 1.65f;
            sideOffset = 0.4f;
            distFromPlayer_norm = 1.85f;
            distFromPlayer_aim = 0.8f;
            initPitchAngle = 5f;
            pitchAngle_min = -70f;
            pitchAngle_max = 40f;

            //Move & Turn
            cam_MoveSpeed = 5f;
            cam_TurnSpeed = 1.5f;
            TurnSmooth = 10f;
            AudoTurnSmooth = 1f;

        }
        private void Awake()
        {
            if (S != null)
            {
                Destroy(this.gameObject);
            }
            S = this;
        }
        // Use this for initialization
        void Start()
        {
            pivot = viewCamera.transform.parent;
            pivot.localPosition = Vector3.up * heightFromGround + Vector3.right * sideOffset;
            viewCamera.transform.localPosition = -1f * Vector3.forward * distFromPlayer_norm;
            pivotEulers = pivot.eulerAngles;
            view_CurPitchAxis = initPitchAngle;
        }

        private void Update()
        {
            //timer += Time.deltaTime;
            //if (!reseting && timer > 3f)
            //{
            //    view_CurYawAngle = viewTarget.rotation.eulerAngles.y;
            //    view_CurPitchAxis = initPitchAngle;
            //    reseting = true;
            //}
        }
        private void FixedUpdate()
        {
            //如果代码放在LateUpdate下会出现抖动的问题,
            //根据资料显示，应该是因为人物的移动是通过刚体的移动实现，其使用的是FixedUpdate
            FollowTarget(Time.deltaTime);
            HandleRotation();   //相机的旋转处理
        }

        /*
         * 相机的旋转处理函数
         * 通过捕获鼠标的移动，根据鼠标移动的x,y偏移量进行控制
         * 
         * NOTE：相机的俯仰控制与Pivot绑定，而在xz平面的旋转则与人物的位置绑定
         */
        void HandleRotation(){

            // Read the user input
            var x = Input.GetAxis("Mouse X");
            var y = Input.GetAxis("Mouse Y");
            
            //根据x，y偏移量以及TurnSpeed控制相机的转向
            //首先处理xz平面的相机位置,也即y轴的旋转，
            view_CurYawAngle += reverseDir ? (x * cam_TurnSpeed * -1f) : (x * cam_TurnSpeed);
            if (view_CurYawAngle >= 180f) view_CurYawAngle -= 360f;
            else if(view_CurYawAngle <= -180f) view_CurYawAngle += 360f;

            //俯仰角控制
            view_CurPitchAxis += reverseDir ?  (y * cam_TurnSpeed) : (y * cam_TurnSpeed * -1f) ;
            view_CurPitchAxis = Mathf.Clamp(view_CurPitchAxis, pitchAngle_min , pitchAngle_max);

            Quaternion transTargetRot = Quaternion.Euler(0f, view_CurYawAngle, 0f);
            Quaternion pivotTargetRot = Quaternion.Euler(view_CurPitchAxis, pivotEulers.y, pivotEulers.z);
            if(TurnSmooth > 0f)
            {   //若有平滑处理，进行平滑处理
                transform.rotation = Quaternion.Slerp(transform.rotation, transTargetRot, TurnSmooth * Time.deltaTime);
                pivot.localRotation = Quaternion.Slerp(pivot.localRotation, pivotTargetRot, TurnSmooth * Time.deltaTime);
            }
            else
            {
                //没有平滑处理则直接转换
                transform.rotation = transTargetRot;
                pivot.localRotation = pivotTargetRot;
            }
        }

        void FollowTarget(float deltaTime) {
            //相机的跟随，可以通过Lerp， 也可通过阻尼滑动
            if (viewTarget == null) return;
            //1.Lerp
            transform.position = Vector3.Lerp(transform.position, viewTarget.position, deltaTime * cam_MoveSpeed);
            //print("transform.position:"+ transform.position + "   viewTarget.position:"+ viewTarget.position);
            //2.阻尼滑动
            //transform.position = Vector3.SmoothDamp(transform.position, viewTarget.position, ref curVelocity , cam_MoveMaxSpeed);
        }

    }

}
