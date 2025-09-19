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

        private KeyCode m_nextKey;

        private float m_currSpeed;

        private static KeyCode Key1 = KeyCode.LeftArrow;
        private static KeyCode Key2 = KeyCode.RightArrow;

        private ResistMicrogameState m_state;

        private bool InputsEnabled;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            m_state = ResistMicrogameState.Deactivated;
        }

        public override bool TryCancel()
        {
            return m_state == ResistMicrogameState.Deactivated || m_state == ResistMicrogameState.Finished;
        }

        #endregion // IStationMicrogame

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

            // apply impulse
            if (InputsEnabled)
            {
                if (m_nextKey == KeyCode.Space)
                {
                    if (Input.GetKeyDown(Key1))
                    {
                        m_currSpeed += Impulse;
                        m_nextKey = Key2;
                    }
                    else if (Input.GetKeyDown(Key2))
                    {
                        m_currSpeed += Impulse;
                        m_nextKey = Key1;
                    }
                }
                else if (m_nextKey == Key1)
                {
                    if (Input.GetKeyDown(Key1))
                    {
                        m_currSpeed += Impulse;
                        m_nextKey = Key2;
                    }
                }
                else if (m_nextKey == Key2)
                {
                    if (Input.GetKeyDown(Key2))
                    {
                        m_currSpeed += Impulse;
                        m_nextKey = Key1;
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

        private void TransitionToActivated()
        {
            InputsEnabled = true;

            // PREREQ: Oxide FUll or Metal FULL
            if (DragMgr.WaferInstance.Data.OxideLayer.State != OxideState.Full && DragMgr.WaferInstance.Data.MetallizationLayer.State != MetallizationState.Full || DragMgr.WaferInstance.Data.ResistLayer.State == ResistState.Full)
            {
                Debug.Log("Invalid prereqs");
                Deactivate();
                return;
            }

            m_state = ResistMicrogameState.Activated;
            m_nextKey = KeyCode.Space; // neutral key
            FluidVisual.localScale = Vector3.one * StartFluidScale;
            m_currSpeed = 0;
            TransitionCommon();
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

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }

        private void TransitionCommon()
        {

        }
    }
}