using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class DragMgr : MonoBehaviour
    {
        public static DragMgr Instance;

        public static WaferState WaferInstance;

        private Camera MainCamera;

        public LayerMask draggableLayer;
        public LayerMask clickBoxLayer;

        [HideInInspector] public bool DragWaferEnabled;

        public Transform CurrDrag { get; private set; }

        public DropZone CurrDropZone { get; private set; }

        private void Awake()
        {
            Instance = this;
            DragWaferEnabled = true;
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
        
        public void SetCurrDropZone(DropZone newZone)
        {
            if (CurrDropZone)
            {
                // handle existing drop zone
            }

            CurrDropZone = newZone;
        }

        public void UnsetCurrDropZone(DropZone prevZone)
        {
            if (CurrDropZone != prevZone)
            {
                // handle mismatch drop zone
                return;
            }

            CurrDropZone = null;
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
                    var dispensable = hit.transform.GetComponent<Dispensable>();
                    if (dispensable && dispensable.Type == DispensableType.Wafer)
                    {
                        if (DragWaferEnabled)
                        {
                            CurrDrag = hit.transform;
                            dispensable.transform.rotation = default;
                            Game.Events.Dispatch(GameEvents.WaferPickedUp);
                        }
                    }
                    else {
                        CurrDrag = hit.transform;
                    }
                }
            }

            if (Input.GetMouseButton(0) && CurrDrag != null)
            {
                 Vector3 targetPos = mouseWorldPos;
                 CurrDrag.position = targetPos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, clickBoxLayer);
                if (hit != null)
                {
                    // if dispenser, destroy dragged object
                    if (hit.GetComponent<ClickBox>().BoxType == ClickBoxType.Dispenser)
                    {
                        Destroy(CurrDrag.gameObject);
                    }
                    // else if drop zone
                    else if (hit.GetComponent<ClickBox>().BoxType == ClickBoxType.DropZone && CurrDrag != null)
                    {
                        var dispensable = CurrDrag.GetComponent<Dispensable>();
                        if (dispensable && dispensable.Type == DispensableType.Wafer)
                        {
                            if (DragWaferEnabled)
                            {
                                // handle wafer
                                // set InUse
                                if (CurrDropZone != null)
                                {
                                    // only allow one at a time
                                    CurrDropZone.AssignToDropZone(CurrDrag);
                                }
                            }
                        }
                        else if (dispensable)
                        {
                            // handle custom
                            if (CurrDropZone != null)
                            {
                                // only allow one at a time
                                CurrDropZone.CustomAssignToDropZone(dispensable);
                            }
                        }
                    }
                }

                // release dragged item
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