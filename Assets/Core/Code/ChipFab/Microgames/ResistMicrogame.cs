using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum ResistMicrogameState
    {
        Activated,
        Ready,
        Spinning,
        Finished,
        Deactivated
    }

    public class ResistMicrogame : StationMicrogame, IStationMicrogame
    {
        public GameObject WaferVisual;

        public Transform FluidVisual;

        public float StartFluidScale;
        public float TargetFluidScale;
        public float Impulse;
        public float Friction;

        [Header("Dropper")]
        public Transform DropperVisual;
        public float DropperXExtents;
        public float DropperSpeed;

        private float m_currSpeed;

        private static KeyCode DropperKey = KeyCode.Space;

        private ResistMicrogameState m_state;

        private bool InputsEnabled;
        private bool UsedDropper;
        private bool DropperMovingRight;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Resist)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }

            base.Deactivate();

            m_state = ResistMicrogameState.Deactivated;
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
            return m_state == ResistMicrogameState.Deactivated || m_state == ResistMicrogameState.Finished;
        }

        #endregion // IStationMicrogame

        private void Awake()
        {
            m_state = ResistMicrogameState.Deactivated;
        }

        private void Update()
        {
            switch (m_state)
            {
                case ResistMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case ResistMicrogameState.Ready:
                    TransitionToSpinning();
                    break;
                case ResistMicrogameState.Spinning:
                    ProcessMicrogame();
                    break;
                case ResistMicrogameState.Finished:
                    Deactivate();
                    break;
                default:
                    break;
            }
        }

        private void ProcessMicrogame()
        {
            // apply friction
            m_currSpeed -= Friction * Time.deltaTime;
            if (m_currSpeed < 0) { m_currSpeed = 0; }

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Resist)
            {
                // add initial impulse
                if (InputsEnabled)
                {
                    UseDropper();
                }
            }
            else
            {
                // Move dropper back and forth until input
                if (!UsedDropper)
                {
                    var dropperPos = DropperVisual.localPosition;
                    if (DropperMovingRight)
                    {
                        dropperPos.x = dropperPos.x + DropperSpeed * Time.deltaTime;
                        if (dropperPos.x >= DropperXExtents)
                        {
                            dropperPos.x = DropperXExtents;
                            DropperMovingRight = false;
                        }
                    }
                    else
                    {
                        dropperPos.x = dropperPos.x - DropperSpeed * Time.deltaTime;
                        if (dropperPos.x <= -DropperXExtents)
                        {
                            dropperPos.x = -DropperXExtents;
                            DropperMovingRight = true;
                        }
                    }
                    DropperVisual.localPosition = dropperPos;

                    if (InputsEnabled)
                    {
                        if (Input.GetKeyDown(DropperKey))
                        {
                            // apply impulse
                            UseDropper();
                        }
                    }
                }
            }

            var scale = FluidVisual.localScale.x;
            scale += m_currSpeed * Time.deltaTime;
            FluidVisual.localScale = Vector3.one * scale;

            if (FluidVisual.localScale.x >= TargetFluidScale)
            {
                InputsEnabled = false;

                if (m_currSpeed == 0)
                {
                    TransitionToFinished();
                }
            }
        }

        private void UseDropper()
        {
            FluidVisual.gameObject.SetActive(true);
            var fluidPos = FluidVisual.position;
            fluidPos.x = DropperVisual.position.x - 0.06f;
            FluidVisual.position = fluidPos;
            m_currSpeed = 1.15f;
            InputsEnabled = false;
            UsedDropper = true;
            DropperVisual.gameObject.SetActive(false);
        }

        private void TransitionToActivated()
        {
            InputsEnabled = true;

            var validSteps = new List<SequenceStepID>() {
                SequenceStepID.ApplyResist,
            };

            // PREREQ: Oxide FUll or Metal FULL
            if (DragMgr.WaferInstance.Data.OxideLayer.State != OxideState.Full
                && DragMgr.WaferInstance.Data.MetallizationLayer.State != MetallizationState.Full
                || DragMgr.WaferInstance.Data.ResistLayer.State == ResistState.Full
                || !FabSequenceMgr.Instance.IsCurrStepAmong(validSteps)
                )
            {
                Debug.Log("Invalid prereqs");
                TryDeactivate();
                return;
            }

            m_state = ResistMicrogameState.Activated;
            FluidVisual.gameObject.SetActive(false);
            FluidVisual.localScale = Vector3.one * StartFluidScale;
            m_currSpeed = 0;
            TransitionCommon();

            // randomize starting pos
            float centerOffset = -0.06f; // 0.156f;
            var xPos = DropperVisual.localPosition;
            xPos.x = Random.Range(-DropperXExtents, DropperXExtents);
            DropperVisual.localPosition = xPos;

            xPos = FluidVisual.localPosition;
            xPos.x = centerOffset;
            FluidVisual.localPosition = xPos;

            UsedDropper = false;
            DropperVisual.gameObject.SetActive(true);
            DropperMovingRight = true;
        }

        private void TransitionToReady()
        {
            m_state = ResistMicrogameState.Ready;
            TransitionCommon();
        }

        private void TransitionToSpinning()
        {
            m_state = ResistMicrogameState.Spinning;
            TransitionCommon();
        }

        private void TransitionToFinished()
        {
            m_state = ResistMicrogameState.Finished;
            float precision = 1 - ((FluidVisual.transform.localScale.x - TargetFluidScale) / (TargetFluidScale - StartFluidScale));
            DragMgr.WaferInstance.SetResistState(precision);
            TransitionCommon();

            TryDeactivate();

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            Game.Events.Dispatch(GameEvents.StationCompleted);
        }

        private void TransitionCommon()
        {

        }
    }
}