using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class LevelMgr : MonoBehaviour
    {
        public SideWaferDisplay Current;

        public SideWaferDisplay TargetSide;
        public AngledWaferDisplay TargetAngled;

        public WaferData TargetData;

        private void Start()
        {
            Game.Events.Register(GameEvents.WaferStateUpdated, HandleWaferStateUpdated);

            TargetSide.UpdateDisplay(TargetData);
            // TargetAngled.UpdateDisplay(TargetData);
        }

        #region Handlers

        private void HandleWaferStateUpdated()
        {
            if (DragMgr.WaferInstance) {
                Current.UpdateDisplay(DragMgr.WaferInstance.Data);
            }
            else
            {
                Current.UpdateDisplay(default);
            }
        }

        #endregion // Handlers
    }
}