using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace  SpaceFab.ChipFab
{
    public class ClickBox : MonoBehaviour
    {
        private bool isHovering;

        [HideInInspector] public UnityEvent OnHoverEnter;
        [HideInInspector] public UnityEvent OnHoverContinue;
        [HideInInspector] public UnityEvent OnHoverExit;

        [HideInInspector] public UnityEvent OnMouseDown;
        [HideInInspector] public UnityEvent OnMouseUp;

        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject()) { return; }

            var screenPos = Input.mousePosition;
            screenPos.z = -Camera.main.transform.position.z;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit != null && hit.gameObject == this.gameObject)
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
                    OnMouseDown?.Invoke();
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
