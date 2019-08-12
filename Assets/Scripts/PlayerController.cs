using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HHY {
    public enum PlayerStatus
    {
        Move = 0,   //高效移动模式
        Fight = 1   //战斗模式，持武器状态
    }

    //玩家控制器
    public class PlayerController : MonoBehaviour
    {
        private Camera mainCamera;
        private TPCharacterCtrller tpCharacterCtrller;  //第三人称控制器

        private Vector3 m_Move; //移动
        bool jump;
        bool walk;
        bool aim;
        bool roll;

        Transform tpsCamera;    //相机朝向

        PlayerStatus curPlayerStatus;
        // Use this for initialization
        void Start()
        {
            if (Camera.main != null){
                mainCamera = Camera.main;
                tpsCamera = Camera.main.transform.parent.parent;
            }
            else{
                Debug.LogWarning("Warning: 未发现Camera.main", gameObject);
            }
            tpCharacterCtrller = GetComponent<TPCharacterCtrller>();
            curPlayerStatus = PlayerStatus.Move;
        }

        private void FixedUpdate()
        {
            // read inputs
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                curPlayerStatus = SwitchStatus();
                tpCharacterCtrller.SwitchMode(curPlayerStatus);
                print("curPlayerStatus:"+ curPlayerStatus);
            }
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

            switch (curPlayerStatus) {
                case PlayerStatus.Move:
                    walk = Input.GetKey(KeyCode.LeftShift);  //是否走路
                    jump = Input.GetKey(KeyCode.Space);    //是否跳跃
                    tpCharacterCtrller.Move(m_Move, walk, jump);
                    break;
                case PlayerStatus.Fight:
                    aim = Input.GetKey(KeyCode.LeftShift);  //是否瞄准
                    if(aim) tpCharacterCtrller.Towards(tpsCamera.rotation); //瞄准之后，会自动转向
                    roll = Input.GetKey(KeyCode.Space);    //是否翻滚
                    tpCharacterCtrller.Fight(m_Move, aim, roll);
                    break;
            }
        }

        PlayerStatus SwitchStatus() {
            return (PlayerStatus)(((int)curPlayerStatus + 1) % 2);
        }

    }

    //public class Node<T> {
    //    T data;
        
    //    Node<T> preNode;
    //    Node<T> nextNode;
    //    public T Data{
    //        get { return data; }
    //        set { data = value; }
    //    }
    //    public Node<T> PreNode
    //    {
    //        get { return preNode; }
    //        set { preNode = value;  }
    //    }
    //    public Node<T> NextNode
    //    {
    //        get { return nextNode; }
    //        set { nextNode = value; }
    //    }
    //    public Node()
    //    {
    //        data = default(T);
    //        preNode = null;
    //        nextNode = null;
    //    }
    //    public Node(T item)
    //    {
    //        data = item;
    //        preNode = null;
    //        nextNode = null;
    //    }
    //}
    //public class LoopList<T> {
    //    private Node<T> head;
    //    private Node<T> tail;
    //    private int count = 0;
    //    public Node<T> Head
    //    {
    //        set
    //        {
    //            head = value;
    //        }
    //        get { return head; }
    //    }

    //    public LoopList() {
    //        this.head = null;
    //    }

    //    public void Add(Node<T> node) {
    //        if (head == null)
    //        {
    //            head = tail = node;
    //            head.PreNode = tail.PreNode = head;
    //            head.NextNode = tail.NextNode = head;
    //        }
    //        else {
    //            tail.NextNode = node;
    //            node.PreNode = tail;
    //            tail = tail.NextNode;
    //            tail.NextNode = head;
    //        }
    //    }


}
