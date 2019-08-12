using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRCourse.GDCamera { 
    public class CameraController : MonoBehaviour {
        public GameObject PlayerGO; //玩家

        private Vector3 Pos {
            get { return this.transform.position; }
            set { this.transform.position = value; }
        }
        private Vector3 relativePos;    //相对距离
	    // Use this for initialization
	    void Start () {
            relativePos = PlayerGO.transform.position - Pos;
        }
	
	    // Update is called once per frame
	    void Update () {
            Pos = PlayerGO.transform.position - relativePos;
            //PlayerGO.transform.position = transform.forward;
	    }
    }
}