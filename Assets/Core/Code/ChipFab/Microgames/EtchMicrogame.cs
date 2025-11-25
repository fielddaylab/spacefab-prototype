using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum EtchMicrogameState
    {
        Activated,
        Ready,
        Etching,
        Finished,
        Deactivated
    }

    public class EtchMicrogame : StationMicrogame, IStationMicrogame
    {
        private EtchMicrogameState m_state;

        public Transform ParentFrame;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            DragMgr.Instance.DragWaferEnabled = false;

            // PREREQS Resist DEVELOPED
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Developed)
            {
                Debug.Log("Invalid prereqs");
                DragMgr.Instance.DragWaferEnabled = true;
                Deactivate();
                return;
            }

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Etch)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }

            base.Deactivate();

            m_state = EtchMicrogameState.Deactivated;
        }

        public override bool TryCancel()
        {
            return m_state == EtchMicrogameState.Deactivated;
        }

        #endregion // IStationMicrogame


        private void Update()
        {
            if (!Container.activeInHierarchy) { return; }
            
            switch (m_state)
            {
                case EtchMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case EtchMicrogameState.Ready:
                    TransitionToEtching();
                    break;
                case EtchMicrogameState.Etching:
                    ProcessMicrogame();
                    break;
                case EtchMicrogameState.Finished:
                    break;
                default:
                    break;
            }
        }

        private void ProcessMicrogame()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Etch)
            {
                if (!m_AutomationRoutine.Exists())
                {
                    m_AutomationRoutine.Replace(AutomationRoutine());
                }
            }
            else
            {
                ProcessManual();
            }
        }

        private void ProcessManual()
        {

        }

        private IEnumerator AutomationRoutine()
        {
            yield return null;
            HandleFinishClicked();
        }

        private void TransitionToActivated()
        {
            m_state = EtchMicrogameState.Activated;
            TransitionCommon();
        }

        private void TransitionToReady()
        {
            m_state = EtchMicrogameState.Ready;
            TransitionCommon();
        }

        private void TransitionToEtching()
        {
            m_state = EtchMicrogameState.Etching;
            TransitionCommon();
        }

        private void TransitionCommon()
        {

        }

        private float EvaluatePrecision()
        {
            int hitCount = 0;

            return 1;
        }

        private void HandleFinishClicked()
        {
            var precision = EvaluatePrecision();
            DragMgr.Instance.DragWaferEnabled = true;
            if (DragMgr.WaferInstance.Data.MetallizationLayer.State == MetallizationState.Full)
            {
                DragMgr.WaferInstance.SetMetallizationStateEtch(precision);

            }
            else if (DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Full)
            {
                DragMgr.WaferInstance.SetOxideStateEtch(precision);
            }
            Deactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }
    }
}