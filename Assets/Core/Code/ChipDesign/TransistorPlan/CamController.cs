using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign {
    public class CamController : MonoBehaviour
    {
        private Camera controllingCam;
        public float CamSpeed;

        private void Start()
        {
            controllingCam = Camera.main;
        }

        private void Update()
        {
            if (controllingCam == null) { return; }

            var moveVector = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                moveVector += new Vector3(0, 1, 0);
            }
            if (Input.GetKey(KeyCode.A)) 
            {
                moveVector += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveVector += new Vector3(0, -1, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveVector += new Vector3(1, 0, 0);
            }

            controllingCam.transform.position += moveVector * CamSpeed * Time.deltaTime;
        }
    }
}
