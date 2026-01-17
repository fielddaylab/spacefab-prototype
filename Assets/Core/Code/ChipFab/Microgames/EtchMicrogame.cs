using BeauRoutine;
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

        [Header("Stencil")]
        public Transform StencilVisual;
        public float StencilXExtents;
        public float StencilSpeed;

        private bool PlacedStencil;
        private bool StencilMovingRight;

        private static KeyCode PlaceKey = KeyCode.Space;

        private Routine m_startupRoutine;

        #region IStationMicrogame

        public override void Activate(WaferState waferState, bool isAutomated)
        {
            base.Activate(waferState, isAutomated);

            DragMgr.Instance.DragWaferEnabled = false;

            var validSteps = new List<SequenceStepID>() {
                SequenceStepID.EtchPattern,
            };

            // PREREQS Resist DEVELOPED
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Developed
                || !FabSequenceMgr.Instance.IsCurrStepAmong(validSteps)
                )
            {
                Debug.Log("Invalid prereqs");
                Game.Events.Dispatch(GameEvents.IncorrectStationAttempted);
                DragMgr.Instance.DragWaferEnabled = true;
                TryDeactivate();
                return;
            }

            Game.Events.Dispatch(GameEvents.StationStarted);

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
            if (!Container.activeInHierarchy && !IsCurrentSessionAutomated) { return; }
            
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
                ProcessAutomation();
            }
            else
            {
                ProcessManual();
            }
        }

        private void ProcessManual()
        {
            // Move dropper back and forth until input
            if (!PlacedStencil)
            {
                var stencilPos = StencilVisual.localPosition;
                if (StencilMovingRight)
                {
                    stencilPos.x = stencilPos.x + StencilSpeed * Time.deltaTime;
                    if (stencilPos.x >= StencilXExtents)
                    {
                        stencilPos.x = StencilXExtents;
                        StencilMovingRight = false;
                    }
                }
                else
                {
                    stencilPos.x = stencilPos.x - StencilSpeed * Time.deltaTime;
                    if (stencilPos.x <= -StencilXExtents)
                    {
                        stencilPos.x = -StencilXExtents;
                        StencilMovingRight = true;
                    }
                }
                StencilVisual.localPosition = stencilPos;

                if (Input.GetKeyDown(PlaceKey))
                {
                    // apply impulse
                    PlaceStencil();
                }
            }
        }

        private void ProcessAutomation()
        {
            if (!m_AutomationRoutine.Exists())
            {
                m_AutomationRoutine.Replace(BasicAutomationRoutine());
                // m_AutomationRoutine.Replace(AutomationRoutine());
            }
        }

        private IEnumerator AutomationRoutine()
        {
            var stencilPos = StencilVisual.localPosition;
            stencilPos.x = 0;
            StencilVisual.localPosition = stencilPos;

            yield return 0.5f;

            PlaceStencil();
        }

        private IEnumerator BasicAutomationRoutine()
        {
            yield return AUTOMATION_TIME;

            PlacedStencil = true;

            var precision = 1;
            SetWaferState(precision);
            Cleanup();
        }

        private void PlaceStencil()
        {
            PlacedStencil = true;

            HandleFinishClicked();
        }

        private void TransitionToActivated()
        {
            PlacedStencil = false;
            StencilVisual.gameObject.SetActive(true);
            StencilMovingRight = true;

            var xPos = StencilVisual.localPosition;
            xPos.x = UnityEngine.Random.Range(-StencilXExtents, StencilXExtents);
            StencilVisual.localPosition = xPos;

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
            float distance = Mathf.Abs(StencilVisual.localPosition.x);

            return 1 - (distance / StencilXExtents);
        }

        private void HandleFinishClicked()
        {
            var precision = EvaluatePrecision();

            SetWaferState(precision);

            Cleanup();
        }

        private void SetWaferState(float precision)
        {
            DragMgr.Instance.DragWaferEnabled = true;
            if (DragMgr.WaferInstance.Data.MetallizationLayer.State == MetallizationState.Full)
            {
                DragMgr.WaferInstance.SetMetallizationStateEtch(precision);
                // auto wash
                DragMgr.WaferInstance.SetResistStateWash();
            }
            else if (DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Full)
            {
                DragMgr.WaferInstance.SetOxideStateEtch(precision);
            }
        }

        private void Cleanup()
        {
            TryDeactivate();

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            Game.Events.Dispatch(GameEvents.StationCompleted);
        }

        private void TryDeactivate()
        {
            Deactivate();

            if (ControlsMgr.Instance.BotEnabled)
            {
                ControlsMgr.Instance.BotInstance.TryCancelCurrStation();
            }
        }
    }
}