using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class Dispenser : NavInteractable
    {
        public GameObject ToDispense;
        public ClickBox ClickBox;

        private void Awake()
        {
            ClickBox.OnMouseDown.AddListener(HandleMouseDown);
        }

        private void HandleMouseDown()
        {
            if (DragMgr.Instance.gameObject.activeInHierarchy)
            {
                Dispense(true);
            }
        }

        private void Dispense(bool fromDrag)
        {
            var newObj = Instantiate(ToDispense);
            if (fromDrag)
            {
                DragMgr.Instance.SetCurrDrag(newObj.transform);
            }

            var dispensable = newObj.GetComponent<Dispensable>();
            if (dispensable)
            {
                if (dispensable.Type == DispensableType.Wafer)
                {
                    // set wafer instance
                    if (DragMgr.WaferInstance)
                    {
                        Game.Events.Dispatch(GameEvents.NewWaferCreated);
                        Destroy(DragMgr.WaferInstance.gameObject);
                    }
                    DragMgr.WaferInstance = newObj.GetComponent<WaferState>();

                    if (!fromDrag)
                    {
                        newObj.transform.position = ControlsMgr.Instance.WaferDefaultPos.position;
                    }
                }
                else if (dispensable.Type == DispensableType.Dopant)
                {
                    newObj.transform.position = ControlsMgr.Instance.DopantDefaultPos.position;
                }
            }
        }

        #region INavInteractable

        public override void Interact()
        {
            base.Interact();
            Dispense(false);
        }

        #endregion // INavInteractable
    }
}