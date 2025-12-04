using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum SputteringMicrogameState
    {
        Activated,
        Ready,
        Sputtering,
        Finished,
        Deactivated
    }

    public class SputteringMicrogame : StationMicrogame, IStationMicrogame
    {
        private static KeyCode FIRE_KEY = KeyCode.Space;

        private SputteringMicrogameState m_state;

        public ClickBox SprayerBox;
        public SpriteRenderer StencilFill;

        private Routine m_StencilFillRoutine;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            DragMgr.Instance.DragWaferEnabled = false;
            SprayerBox.OnMouseDown.AddListener(HandleSprayMouseDown);
            StencilFill.enabled = false;

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Sputter)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }

            base.Deactivate();

            SprayerBox.OnMouseDown.RemoveListener(HandleSprayMouseDown);

            m_state = SputteringMicrogameState.Deactivated;
        }

        private void TryDeactivate()
        {
            Deactivate();
            if (ControlsMgr.Instance.BotEnabled)
            {
                ControlsMgr.Instance.BotInstance.TryCancelCurrStation();
            }
        }

        public override bool TryCancel()
        {
            return m_state == SputteringMicrogameState.Deactivated;
        }

        #endregion // IStationMicrogame

        private void Update()
        {
            if (!Container.activeInHierarchy) { return; }

            switch (m_state)
            {
                case SputteringMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case SputteringMicrogameState.Ready:
                    TransitionToSputtering();
                    break;
                case SputteringMicrogameState.Sputtering:
                    ProcessMicrogame();
                    break;
                case SputteringMicrogameState.Finished:
                    break;
                default:
                    break;
            }
        }

        private void ProcessMicrogame()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Sputter)
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
            if (Input.GetKey(FIRE_KEY))
            {
                HandleSprayMouseDown();
            }
        }

        private IEnumerator AutomationRoutine()
        {

            yield return 0.5f;

            yield return 0.5f;

            HandleFinishClicked();
        }

        private void TransitionToActivated()
        {
            m_state = SputteringMicrogameState.Activated;
            TransitionCommon();

            var validSteps = new List<SequenceStepID>() {
                SequenceStepID.AddStencil_SPUTTER,
                SequenceStepID.FillStencil_SPUTTER,
            };

            // PREREQS (Oxide STRIPPED & Resist STRIPPED) or (Oxide EMPTY & Resist EMPTY)
            bool oxideAndResistPrereq = (DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Stripped && DragMgr.WaferInstance.Data.ResistLayer.State == ResistState.Stripped)
                || (DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Empty && DragMgr.WaferInstance.Data.ResistLayer.State == ResistState.Empty);
            
            if (!oxideAndResistPrereq
                || !FabSequenceMgr.Instance.IsCurrStepAmong(validSteps)
                )
            {
                DragMgr.Instance.DragWaferEnabled = true;
                TryDeactivate();
            }
        }

        private void TransitionToReady()
        {
            m_state = SputteringMicrogameState.Ready;
            TransitionCommon();
        }

        private void TransitionToSputtering()
        {
            m_state = SputteringMicrogameState.Sputtering;
            TransitionCommon();
        }

        private void TransitionCommon()
        {

        }

        private float EvaluatePrecision()
        {
            float precision = 1;


            return precision;
        }

        private void HandleFinishClicked()
        {
            var precision = EvaluatePrecision();
            DragMgr.Instance.DragWaferEnabled = true;
            bool fillStencil = DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Stripped && DragMgr.WaferInstance.Data.ResistLayer.State == ResistState.Stripped;
            bool createStencil = DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Empty && DragMgr.WaferInstance.Data.ResistLayer.State == ResistState.Empty;
            if (fillStencil)
            {
                DragMgr.WaferInstance.SetMetallizationStateFillStencil(precision);
            }
            else if (createStencil)
            {
                DragMgr.WaferInstance.SetMetallizationStateCreateStencil(precision);
            }
            TryDeactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            Game.Events.Dispatch(GameEvents.StationCompleted);
        }

        private void HandleSprayMouseDown()
        {
            if (m_StencilFillRoutine.Exists())
            {
                return;
            }

            m_StencilFillRoutine.Replace(StencilFillRoutine());
        }

        private IEnumerator StencilFillRoutine()
        {
            StencilFill.enabled = true;

            var currColor = StencilFill.color;
            currColor.a = 0;
            StencilFill.color = currColor;

            var targetColor = currColor;
            targetColor.a = 1;

            yield return StencilFill.ColorTo(targetColor, 1f, ColorUpdate.FullColor);

            yield return 1.5f;

            HandleFinishClicked();
        }
    }
}