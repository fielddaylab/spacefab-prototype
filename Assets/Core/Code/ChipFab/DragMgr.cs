using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class DragMgr : MonoBehaviour
    {
        public static DragMgr Instance;

        private Camera MainCamera;

        public LayerMask draggableLayer;

        public Transform CurrDrag { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            MainCamera = Camera.main;
        }

        public void SetCurrDrag(Transform newDrag)
        {
            if (CurrDrag)
            {
                // handle existing drag
            }

            CurrDrag = newDrag;

            Vector3 mouseWorldPos = GetMouseWorldPosition();
            newDrag.position = mouseWorldPos;
        }

        private void Update()
        {
            // move obj to curr pos
            HandleMouseInput();
        }


        void HandleMouseInput()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();

            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, draggableLayer);
                if (hit != null)
                {
                    CurrDrag = hit.transform;
                }
            }

            if (Input.GetMouseButton(0) && CurrDrag != null)
            {
                Vector3 targetPos = mouseWorldPos;
                CurrDrag.position = targetPos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                CurrDrag = null;
            }
        }

        Vector3 GetMouseWorldPosition()
        {
            Vector3 screenMouse = Input.mousePosition;
            screenMouse.z = -MainCamera.transform.position.z;
            return MainCamera.ScreenToWorldPoint(screenMouse);
        }
    }
}