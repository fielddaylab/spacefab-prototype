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
            Dispense(true);
        }

        private void Dispense(bool fromDrag)
        {
            var newObj = Instantiate(ToDispense);
            if (fromDrag)
            {
                // DragMgr.Instance.SetCurrDrag(newObj.transform);
            }

            var dispensable = newObj.GetComponent<Dispensable>();
            if (dispensable)
            {
                if (dispensable.Type == DispensableType.Wafer)
                {
                    Game.Events.Dispatch(GameEvents.NewWaferCreated);

                    // set wafer instance
                    if (DragMgr.WaferInstance)
                    {
                        Destroy(DragMgr.WaferInstance.gameObject);
                    }
                    DragMgr.WaferInstance = newObj.GetComponent<WaferState>();
                    Game.Events.Dispatch(GameEvents.WaferStateUpdated);

                    if (!fromDrag)
                    {
                        newObj.transform.position = ControlsMgr.Instance.WaferDefaultPos.position;
                    }

                    if (ControlsMgr.Instance.ConveyorEnabled)
                    {
                        ConveyorMgr.Instance.AssignWafer();
                    }
                }
                else if (dispensable.Type == DispensableType.Dopant)
                {
                    if (!fromDrag)
                    {
                        newObj.transform.position = ControlsMgr.Instance.DopantDefaultPos.position;
                    }

                    // set dopant instance
                    if (DragMgr.DopantInstance)
                    {
                        Game.Events.Dispatch(GameEvents.NewDopantCreated);
                        Destroy(DragMgr.DopantInstance.gameObject);
                    }

                    DragMgr.DopantInstance = newObj;

                    // only place to put dopant is in the furnace
                    FurnaceMicrogame.Instance.AssignDopant(dispensable);
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