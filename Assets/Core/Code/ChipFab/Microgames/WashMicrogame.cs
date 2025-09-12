using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum WashMicrogameState
    {
        Activated,
        Ready,
        Washing,
        Finished,
        Deactivated
    }

    public class WashMicrogame : StationMicrogame, IStationMicrogame
    {
        public ClickBox WashButton;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        #endregion // IStationMicrogame

        private void OnEnable()
        {
            WashButton.OnMouseDown.AddListener(HandleWashClicked);
        }

        private void OnDisable()
        {
            WashButton.OnMouseDown.RemoveListener(HandleWashClicked);
        }

        private void HandleWashClicked()
        {
            // PREREQS Resist DEVELOPED
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Stripped)
            {
                Debug.Log("Invalid prereqs");
                Deactivate();
                return;
            }

            DragMgr.WaferInstance.SetResistStateWash();
            Deactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }
    }
}