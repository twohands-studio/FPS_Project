using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 相机的遮挡预防处理
 * 
 */
public class OcclusionPrevention : MonoBehaviour {
    public Transform viewTarget; //观察目标
    public Transform viewCamera; //观察相机

    [Range(5f, 15f)] [SerializeField] private float cam_MoveSpeed; //相机的移动速度，指跟进角色的速度
    [Range(0, 2f)] [SerializeField] private float forwardOffset;
    [Range(0f, 1f)] [SerializeField] private float prevOccTime;    //预防遮罩的调节时间
    [Range(0f, 1f)] [SerializeField] private float recoverTime;    //恢复原位置的调节时间
    [Range(0, 0.3f)] [SerializeField] private float sphereRadius;   //检测的碰撞半径
    [Range(0.5f, 1.5f)] [SerializeField] private float minDistance;   //最小的距离
    
    private float originalDistance; //初始时的距离
    private float currentDistance; //当前距离
    Transform pivot; // the point at which the camera pivots around 相机的轴点

    private Ray viewRay;    //观察射线
    private float camMoveVelocity;             // the velocity at which the camera moved
    private void Reset()
    {
        viewTarget = GameObject.FindGameObjectWithTag("Player").transform;
        viewCamera = Camera.main.gameObject.transform;
        forwardOffset = 0f;
        cam_MoveSpeed = 10f;

        prevOccTime = 0.2f;
        recoverTime = 0.5f;
        sphereRadius = 0.2f;
        minDistance = 0.4f;
    }
    // Use this for initialization
    void Start () {
        originalDistance = viewCamera.transform.localPosition.magnitude;
        currentDistance = originalDistance;
        pivot = viewCamera.parent;
        viewRay = new Ray();
    }

    // Update is called once per frame
    void FixedUpdate () {
        float targetDistance = originalDistance;
        //射线跟踪
        viewRay.direction = -pivot.forward;
        viewRay.origin = pivot.position;
        RaycastHit rayHit;
       
        //若存在碰撞，则更新目标位置为合适距离，否则使用原距离
        if (Physics.SphereCast(viewRay, sphereRadius, out rayHit, originalDistance))
        {
#if UNITY_EDITOR
            Debug.DrawLine(rayHit.point + 0.1f * viewRay.
                direction, rayHit.point - 0.1f * viewRay.direction, Color.red);
#endif
            /*
             * 当角色朝着Camera跑动时，由于相机的跟踪延迟，可能导致角色位于射线之间而产生碰撞
             * 
             */
            if (rayHit.collider.tag != "Player")
                targetDistance = -pivot.InverseTransformPoint(rayHit.point).z;
        }
        //1.Lerp法
        //currentDistance = Mathf.Lerp(currentDistance, targetDistance, cam_MoveSpeed * Time.fixedDeltaTime);
        //2.SmoothDamp法
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref camMoveVelocity,
                                           (currentDistance > targetDistance) ? prevOccTime : recoverTime);
        currentDistance = Mathf.Clamp(currentDistance, minDistance, originalDistance);
        viewCamera.localPosition = -Vector3.forward * currentDistance;
    }
}
