using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HHY {
    //玩家控制器
    public class PlayerController : MonoBehaviour
    {
        private Camera mainCamera; 
        private TPCharacterCtrller tpCharacterCtrller;  //第三人称控制器

        private Vector3 m_Move; //移动
        bool jump;
        bool walk;
        // Use this for initialization
        void Start()
        {
            if (Camera.main != null){
                mainCamera = Camera.main;
            }
            else{
                Debug.LogWarning("Warning: 未发现Camera.main", gameObject);
            }
            tpCharacterCtrller = GetComponent<TPCharacterCtrller>();
        }

        private void FixedUpdate()
        {
            // read inputs
            walk = Input.GetKey(KeyCode.LeftShift);  //是否走路
            jump = Input.GetKey(KeyCode.Space);    //是否跳跃
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            
            if (mainCamera != null)
            {
                //根据相机的方向获取当前方向下的x轴以及z轴的方向
                /*
                 *  Camera.forward始终指向相机的自身z轴朝向在世界坐标系下的结果
                 *  因此其在xz平面的分向量即是目标。
                 */
                Transform camTrans = mainCamera.transform;
                Vector3 m_CamForward = Vector3.Scale(camTrans.forward, new Vector3(1, 0, 1)).normalized; //Scale即各分量的相乘
                m_Move = v * m_CamForward + h * camTrans.right;//默认相机只有偏航角和俯仰角，因此local x始终在xz平面
            }
            else
            {
                //无mainCamera时的备选方案
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
            
            tpCharacterCtrller.Move(m_Move , walk, jump);
        }

    }
}
