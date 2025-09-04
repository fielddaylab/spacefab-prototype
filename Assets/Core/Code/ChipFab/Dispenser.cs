using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class Dispenser : MonoBehaviour
    {
        public GameObject ToDispense;
        public ClickBox ClickBox;

        private void Start()
        {
            ClickBox.OnMouseDown.AddListener(HandleMouseDown);
        }

        private void HandleMouseDown()
        {
            var newObj = Instantiate(ToDispense);
            DragMgr.Instance.SetCurrDrag(newObj.transform);

            var dispensable = newObj.GetComponent<Dispensable>();
            if (dispensable && dispensable.Type == DispensableType.Wafer)
            {
                // set wafer instance
                if (DragMgr.WaferInstance)
                {
                    Game.Events.Dispatch(GameEvents.NewWaferCreated);
                    Destroy(DragMgr.WaferInstance.gameObject);
                }
                DragMgr.WaferInstance = dispensable;
            }
        }
    }
}