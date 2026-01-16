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

        public override void Activate(WaferState waferState, bool isAutomated)
        {
            base.Activate(waferState, isAutomated);

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Wash)
            {
                if (!m_AutomationRoutine.Exists())
                {
                    m_AutomationRoutine.Replace(AutomationRoutine());
                }
            }
        }

        public override void Deactivate()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Wash)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }


            base.Deactivate();
        }

        public override bool TryCancel()
        {
            return true;
        }

        #endregion // IStationMicrogame

        private IEnumerator AutomationRoutine()
        {
            yield return 0.5f;

            HandleWashClicked();

            yield return 0.5f;
        }

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
                Game.Events.Dispatch(GameEvents.IncorrectStationAttempted);
                Deactivate();
                return;
            }

            DragMgr.WaferInstance.SetResistStateWash();
            Deactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            Game.Events.Dispatch(GameEvents.StationCompleted);
        }
    }
}