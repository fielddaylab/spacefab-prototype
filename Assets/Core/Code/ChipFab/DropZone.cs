using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class DropZone : MonoBehaviour
    {
        public ClickBox ClickBox;
        [HideInInspector] public bool InUse;
        public GameObject HoverGroup;
        [HideInInspector] public Transform UsingTransform;
        public Transform SlotPos;

        private void Start()
        {
            ClickBox.OnHoverEnter.AddListener(HandleMouseEnter);
            ClickBox.OnHoverContinue.AddListener(HandleHoverContinue);
            ClickBox.OnHoverExit.AddListener(HandleMouseExit);

            ClickBox.OnMouseDown.AddListener(HandleMouseDown);

            Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);
            Game.Events.Register(GameEvents.WaferPickedUp, HandleWaferPickedUp);

            HoverGroup.SetActive(false);
        }

        private void HandleMouseEnter()
        {
            CheckTriggerHoverDisplays();

        }

        private void HandleHoverContinue()
        {
            CheckTriggerHoverDisplays();
        }

        private void HandleMouseExit()
        {
            DragMgr.Instance.UnsetCurrDropZone(this);
            HoverGroup.SetActive(false);
        }

        private void HandleNewWaferCreated()
        {
            if (UsingTransform == DragMgr.WaferInstance.transform)
            {
                UsingTransform = null;
                InUse = false;
            }
        }

        private void HandleWaferPickedUp()
        {
            if (UsingTransform == DragMgr.WaferInstance.transform)
            {
                UsingTransform = null;
                InUse = false;
            }
        }

        private void HandleMouseDown()
        {
            if (InUse)
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();

                if (Input.GetMouseButtonDown(0))
                {
                    Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, DragMgr.Instance.draggableLayer);
                    if (hit != null)
                    {
                        var dispensable = hit.GetComponent<Dispensable>();
                        if (dispensable && dispensable.Type == DispensableType.Wafer)
                        {
                            RemoveFromDropZone();
                        }
                    }
                }
            }
        }

        private void CheckTriggerHoverDisplays()
        {
            if (DragMgr.Instance.CurrDrag != null && !InUse)
            {
                DragMgr.Instance.SetCurrDropZone(this);
                HoverGroup.SetActive(true);
            }
        }

        public void AssignToDropZone(Transform toAssign)
        {
            if (!InUse || (InUse && toAssign == UsingTransform))
            {
                InUse = true;
                UsingTransform = toAssign;

                toAssign.transform.position = SlotPos.transform.position;
                toAssign.transform.rotation = SlotPos.transform.rotation;
            }
        }

        public void RemoveFromDropZone()
        {
            // remove wafer from station
            InUse = false;
            DragMgr.Instance.SetCurrDrag(UsingTransform);

            UsingTransform = null;
        }

        Vector3 GetMouseWorldPosition()
        {
            Vector3 screenMouse = Input.mousePosition;
            screenMouse.z = -Camera.main.transform.position.z;
            return Camera.main.ScreenToWorldPoint(screenMouse);
        }
    }
}