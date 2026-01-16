using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace  SpaceFab.ChipFab
{
    public enum ClickBoxType
    {
        Dispenser,
        DropZone,
        Button
    }

    public class ClickBox : MonoBehaviour
    {
        public int Priority;
        public ClickBoxType BoxType;

        private bool isHovering;

        [HideInInspector] public UnityEvent OnHoverEnter;
        [HideInInspector] public UnityEvent OnHoverContinue;
        [HideInInspector] public UnityEvent OnHoverExit;

        [HideInInspector] public UnityEvent OnMouseDown;
        // [HideInInspector] public UnityEvent OnMouseContinue;
        [HideInInspector] public UnityEvent OnMouseUp;

        private int m_lastMouseDownFrame = 0;

        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject()) { return; }

            var screenPos = Input.mousePosition;
            screenPos.z = -Camera.main.transform.position.z;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos, DragMgr.Instance.clickBoxLayer);
            Collider2D highestPriorityHit = null;

            int highestPriority = int.MinValue;
            foreach (var hit in hits)
            {
                if (hit != null)
                {
                    var box = hit.GetComponent<ClickBox>();
                    if (box.Priority > highestPriority)
                    {
                        highestPriorityHit = hit;
                        highestPriority = box.Priority;
                    }
                }
            }

            if (highestPriorityHit != null && highestPriorityHit.gameObject == this.gameObject)
            {
                if (!isHovering)
                {
                    isHovering = true;
                    OnHoverEnter?.Invoke();
                }
                else
                {
                    OnHoverContinue?.Invoke();
                }

                if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
                {
                    if (Time.frameCount != m_lastMouseDownFrame)
                    {
                        OnMouseDown?.Invoke();
                        m_lastMouseDownFrame = Time.frameCount;
                    }
                }
                else if (Input.GetMouseButton(0)) // Left mouse button held
                {
                    // OnMouseContinue?.Invoke();
                }
                if (Input.GetMouseButtonUp(0)) // Left mouse button released
                {
                    OnMouseUp?.Invoke();
                }
            }
            else
            {
                if (isHovering)
                {
                    isHovering = false;
                    OnHoverExit?.Invoke();
                }
            }
        }
    }
}
