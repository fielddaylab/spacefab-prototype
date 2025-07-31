using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class FloorDragMgr : MonoBehaviour
    {
        [Header("Grid Settings")]
        public float gridSize = 1f;

        [Header("Interaction Settings")]
        public LayerMask draggableLayer;

        private Camera mainCamera;
        private Transform selectedObject;
        private Vector3 offset; // grid offset
        private Vector3 objOffset; // obj offset

        void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            if (FloorInteractionMgr.Instance.ActiveLayer == GridInteractionLayer.Nodes)
            { 
                HandleMouseInput();
            }
        }

        void HandleMouseInput()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();

            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, draggableLayer);
                if (hit != null)
                {
                    selectedObject = hit.transform;
                    offset = selectedObject.position - mouseWorldPos;
                    var offsetCalcX = selectedObject.position.x;
                    offsetCalcX *= 100;
                    offsetCalcX %= 100;
                    offsetCalcX /= 100;

                    var offsetCalcY = selectedObject.position.y;
                    offsetCalcY *= 100;
                    offsetCalcY %= 100;
                    offsetCalcY /= 100;

                    objOffset = new Vector3(offsetCalcX, offsetCalcY, 0);
                }
            }

            if (Input.GetMouseButton(0) && selectedObject != null)
            {
                Vector3 targetPos = mouseWorldPos + offset;
                selectedObject.position = SnapToGrid(targetPos) + objOffset;
            }

            if (Input.GetMouseButtonUp(0))
            {
                selectedObject = null;
            }
        }

        Vector3 GetMouseWorldPosition()
        {
            Vector3 screenMouse = Input.mousePosition;
            screenMouse.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(screenMouse);
        }

        Vector3 SnapToGrid(Vector3 position)
        {
            float x = Mathf.Round(position.x / gridSize) * gridSize;
            float y = Mathf.Round(position.y / gridSize) * gridSize;
            return new Vector3(x, y, 0f);
        }
    }
}