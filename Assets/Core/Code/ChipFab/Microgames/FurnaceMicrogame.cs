using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum FurnaceMicrogameState
    {
        Activated,
        Ready,
        Heating,
        Finished,
        Deactivated
    }

    public class FurnaceMicrogame : StationMicrogame, IStationMicrogame
    {
        public float HeatTime;
        public float MaxTemp;
        public float MinTemp;

        public float TargetMaxTemp;
        public float TargetMinTemp;

        public float ApplyHeatIncrement;
        public float HeatLossRate;

        public GameObject HeatingGroup;
        public Transform ThermoSlider;

        public ClickBox StartButton;
        public ClickBox ApplyHeatButton;
        public ClickBox FinishButton;

        private FurnaceMicrogameState m_state;
        private float m_heatTimer;
        private float m_currTemp;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            StartButton.transform.parent.gameObject.SetActive(false);
            ApplyHeatButton.transform.parent.gameObject.SetActive(false);
            FinishButton.transform.parent.gameObject.SetActive(false);

            StartButton.OnMouseDown.AddListener(HandleStartMouseDown);
            ApplyHeatButton.OnMouseDown.AddListener(HandleApplyHeat);
            FinishButton.OnMouseDown.AddListener(HandleFinishClicked);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            StartButton.OnMouseDown.RemoveListener(HandleStartMouseDown);
            ApplyHeatButton.OnMouseDown.RemoveListener(HandleApplyHeat);
            FinishButton.OnMouseDown.RemoveListener(HandleFinishClicked);

            TransitionToDeactivated();
        }

        #endregion // IStationMicrogame

        #region Unity Callbacks

        private void Update()
        {
            switch (m_state)
            {
                case FurnaceMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case FurnaceMicrogameState.Ready:
                    break;
                case FurnaceMicrogameState.Heating:
                    ProcessMicrogame();
                    break;
                case FurnaceMicrogameState.Finished:
                    break;
                default:
                    break;
            }
        }

        #endregion // Unity Callbacks

        public void SetTargetRange(float min, float max)
        {
            TargetMinTemp = min;
            TargetMaxTemp = max;
        }

        private void ProcessMicrogame()
        {
            m_heatTimer -= Time.deltaTime;

            if (m_heatTimer <= 0)
            {
                TransitionToFinished();
            }
            else
            {
                // reduce heat by steady amount
                m_currTemp -= Time.deltaTime * HeatLossRate;
            }

            UpdateHeatingVisuals();
        }

        private void UpdateHeatingVisuals()
        {
            var scale = ThermoSlider.localScale;
            scale.y = (m_currTemp - MinTemp) / (MaxTemp - MinTemp) * 2;
            ThermoSlider.localScale = scale;
        }

        #region State Transitions

        private void TransitionToActivated()
        {
            m_state = FurnaceMicrogameState.Activated;
            TransitionCommon();
        }

        private void TransitionToReady()
        {
            m_state = FurnaceMicrogameState.Ready;
            TransitionCommon();
        }

        private void TransitionToHeating()
        {
            m_state = FurnaceMicrogameState.Heating;
            m_heatTimer = HeatTime;
            m_currTemp = (TargetMaxTemp + TargetMinTemp) / 2.0f;
            TransitionCommon();
        }

        private void TransitionToFinished()
        {
            m_state = FurnaceMicrogameState.Finished;
            TransitionCommon();
        }

        private void TransitionToDeactivated()
        {
            m_state = FurnaceMicrogameState.Deactivated;
            TransitionCommon();
        }

        private void TransitionCommon()
        {
            StartButton.transform.parent.gameObject.SetActive(m_state == FurnaceMicrogameState.Ready);
            ApplyHeatButton.transform.parent.gameObject.SetActive(m_state == FurnaceMicrogameState.Heating);
            HeatingGroup.SetActive(m_state == FurnaceMicrogameState.Heating);
            FinishButton.transform.parent.gameObject.SetActive(m_state == FurnaceMicrogameState.Finished);
        }

        #endregion // State Transitions

        #region Handlers

        private void HandleStartMouseDown()
        {
            TransitionToHeating();
        }

        private void HandleApplyHeat()
        {
            m_currTemp += ApplyHeatIncrement;
        }

        private void HandleFinishClicked()
        {
            Deactivate();
        }

        #endregion // Handlers
    }
}