using FieldDay;
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
        public static FurnaceMicrogame Instance;

        public float HeatTime;
        public float MaxTemp;
        public float MinTemp;

        public float TargetMaxTemp;
        public float TargetMinTemp;

        public float ApplyHeatIncrement;
        public float HeatLossRate;

        public GameObject HeatingGroup;
        public Transform ThermoSlider;

        public Transform DopantSlotPos;

        public ClickBox StartButton;
        // public ClickBox ApplyHeatButton;
        public ClickBox FinishButton;

        private FurnaceMicrogameState m_state;
        private float m_heatTimer;
        private float m_currTemp;
        private float m_precisionTimer;

        private GameObject m_dopantObj;
        private DopingType m_appliedDopant;
        private bool m_usedDopant;

        private static KeyCode StokeKey = KeyCode.UpArrow;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            StartButton.transform.parent.gameObject.SetActive(false);
            //ApplyHeatButton.transform.parent.gameObject.SetActive(false);
            FinishButton.transform.parent.gameObject.SetActive(false);

            StartButton.OnMouseDown.AddListener(HandleStartMouseDown);
            //ApplyHeatButton.OnMouseDown.AddListener(HandleApplyHeat);
            FinishButton.OnMouseDown.AddListener(HandleFinishClicked);

            Game.Events.Register(GameEvents.NewDopantCreated, HandleNewDopantCreated);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            StartButton.OnMouseDown.RemoveListener(HandleStartMouseDown);
            //ApplyHeatButton.OnMouseDown.RemoveListener(HandleApplyHeat);
            FinishButton.OnMouseDown.RemoveListener(HandleFinishClicked);

            TransitionToDeactivated();
        }

        #endregion // IStationMicrogame

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

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

            if (Input.GetKeyDown(StokeKey))
            {
                HandleApplyHeat();
            }

            UpdateHeatingVisuals();
            EvaluatePrecision();
        }

        private void UpdateHeatingVisuals()
        {
            var scale = ThermoSlider.localScale;
            scale.y = (m_currTemp - MinTemp) / (MaxTemp - MinTemp) * 2;
            ThermoSlider.localScale = scale;
        }

        private void EvaluatePrecision()
        {
            if (m_currTemp > TargetMaxTemp || m_currTemp < TargetMinTemp)
            {
                m_precisionTimer += Time.deltaTime;
            }
        }

        #region State Transitions

        private void TransitionToActivated()
        {
            m_state = FurnaceMicrogameState.Activated;
            m_precisionTimer = 0;
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
            float precision = (HeatTime - m_precisionTimer) / HeatTime;
            DragMgr.WaferInstance.SetOxideStateFurnace(precision, m_usedDopant, m_appliedDopant);
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
            //ApplyHeatButton.transform.parent.gameObject.SetActive(m_state == FurnaceMicrogameState.Heating);
            HeatingGroup.SetActive(m_state == FurnaceMicrogameState.Heating);
            FinishButton.transform.parent.gameObject.SetActive(m_state == FurnaceMicrogameState.Finished);
        }

        #endregion // State Transitions

        #region Handlers

        private void HandleStartMouseDown()
        {
            // check if valid combo
            // PREREQ: Oxide STRIPPED & DOPANT or Oxide EMPTY
            bool dopantMode = DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Stripped && m_usedDopant;
            bool emptyMode = DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Empty;
            if (dopantMode || emptyMode)
            {
                DragMgr.Instance.DragWaferEnabled = false;
                TransitionToHeating();
            }
            else
            {
                Debug.Log("Invalid prereqs");
                Deactivate();
                return;
            }
        }

        private void HandleApplyHeat()
        {
            m_currTemp += ApplyHeatIncrement;
        }

        private void HandleFinishClicked()
        {
            DragMgr.Instance.DragWaferEnabled = true;
            RemoveDopant();
            Deactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }

        private void HandleNewDopantCreated()
        {
            RemoveDopant();
        }

        #endregion // Handlers

        public void AssignDopant(Dispensable dispensable)
        {
            if (m_usedDopant)
            {
                RemoveDopant();
            }

            dispensable.transform.position = DopantSlotPos.transform.position;
            dispensable.transform.rotation = DopantSlotPos.transform.rotation;

            var dopant = dispensable.GetComponent<Dopant>();

            m_usedDopant = true;
            m_appliedDopant = dopant.Type;
            m_dopantObj = dispensable.gameObject;
        }

        private void RemoveDopant()
        {
            if (m_dopantObj)
            {
                Destroy(m_dopantObj);
                m_usedDopant = false;
            }
        }
    }
}