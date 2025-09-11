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

            // PREREQS Resist DEVELOPED
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Developed)
            {
                Debug.Log("Invalid prereqs");
                Deactivate();
                return;
            }

            WashButton.OnMouseDown.AddListener(HandleWashClicked);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        #endregion // IStationMicrogame

        private void HandleWashClicked()
        {
            DragMgr.WaferInstance.SetResistStateWash();
            Deactivate();
        }
    }
}