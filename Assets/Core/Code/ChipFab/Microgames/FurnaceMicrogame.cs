using BeauRoutine;
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

        public Transform DopantSlotPos;

        public ClickBox StartButton;
        public ClickBox ApplyHeatButton;
        public ClickBox FinishButton;

        public SpriteRenderer ApplyHeatButtonRenderer;

        public Sprite DefaultButton;
        public Sprite PressedButton;

        private FurnaceMicrogameState m_state;
        private float m_currTemp;
        private float m_finalTemp;

        private GameObject m_dopantObj;
        private DopingType m_appliedDopant;
        private bool m_usedDopant;

        private bool m_isHeating = false;

        private static KeyCode StokeKey = KeyCode.Space;

        [Header("Automation")]
        public float AutoTime;
        private float AutoTimer;

        [Space(5)]
        [Header("Heating Visuals")]
        public GameObject HeatingGroup;
        public Transform Gauge;
        public Transform TargetIndicator;
        public Transform TargetZone;
        // public Transform ThermoSlider;

        public Transform MicrogameGauge;
        public Transform MicrogameGaugeTargetIndicator;
        public Transform MicrogameGaugeTargetZone;

        private Routine m_applyHeatRoutine;
        private bool HeatingCompleted;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            ApplyHeatButton.OnMouseDown.AddListener(HandleStartHeat);
            ApplyHeatButton.OnMouseUp.AddListener(HandleEndHeat);

            ApplyHeatButtonRenderer.sprite = DefaultButton;

            AutoTimer = AutoTime;

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Furnace)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }

            base.Deactivate();

            ApplyHeatButton.OnMouseDown.RemoveListener(HandleStartHeat);
            ApplyHeatButton.OnMouseUp.RemoveListener(HandleEndHeat);

            TransitionToDeactivated();
        }

        public override bool TryCancel()
        {
            bool canCancel = m_state == FurnaceMicrogameState.Deactivated || m_state == FurnaceMicrogameState.Ready;
            if (canCancel) { Deactivate(); }
            return canCancel;
        }

        #endregion // IStationMicrogame

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
            m_state = FurnaceMicrogameState.Deactivated;
        }

        protected override void Start()
        {
            base.Start();

            var targetTemp = (TargetMaxTemp + TargetMinTemp) / 2.0f;

            var ratio = (targetTemp - MinTemp) / (MaxTemp - MinTemp);

            // Align Target Indicator
            var angles = TargetIndicator.transform.localEulerAngles;
            angles.z = -180 * ratio;
            TargetIndicator.transform.localEulerAngles = angles;
            MicrogameGaugeTargetIndicator.transform.localEulerAngles = angles;

            // Align Target Zone
            angles = TargetZone.transform.localEulerAngles;
            angles.z = -180 * ratio;
            TargetZone.transform.localEulerAngles = angles;
            MicrogameGaugeTargetZone.transform.localEulerAngles = angles;
        }

        private void Update()
        {
            switch (m_state)
            {
                case FurnaceMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case FurnaceMicrogameState.Ready:
                    TryStartFurnace();
                    break;
                case FurnaceMicrogameState.Heating:
                    ProcessMicrogame();
                    break;
                case FurnaceMicrogameState.Finished:
                    FinishFurnace();
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
            if (HeatingCompleted)
            {
                TransitionToFinished();
            }

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Furnace)
            {
                ProcessAutomation();
            }
            else
            {
                ProcessManual();
            }

            if (m_isHeating)
            {
                m_finalTemp += ApplyHeatIncrement * Time.deltaTime;
                if (m_finalTemp > MaxTemp)
                {
                    m_finalTemp = MaxTemp;
                }
            }
        }

        private void ProcessAutomation()
        {
            var instruction = AutomationMgr.Instance.CurrInstruction;

            if (AutoTimer == AutoTime)
            {
                HandleStartHeat();
            }

            AutoTimer -= Time.deltaTime;

            if (AutoTimer <= 0 && m_isHeating)
            {
                HandleEndHeat();
            }
        }

        private void ProcessManual()
        {
            if (Input.GetKeyDown(StokeKey) && !m_AutomationRoutine.Exists() && !HeatingCompleted)
            {
                HandleStartHeat();
            }
            if (Input.GetKeyUp(StokeKey))
            {
                HandleEndHeat();
            }
        }

        #region State Transitions

        private void TransitionToActivated()
        {
            m_state = FurnaceMicrogameState.Activated;
            m_isHeating = false;
            m_finalTemp = 0;
            m_currTemp = 0;
            HeatingCompleted = false;

            var validSteps = new List<SequenceStepID>() {
                SequenceStepID.AddStencil_OXIDE,
                SequenceStepID.FillStencil_DOPE
            };

            // PREREQ: Oxide not FUll and Metal not FULL
            if (DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Full
                || DragMgr.WaferInstance.Data.MetallizationLayer.State == MetallizationState.Full
                || DragMgr.WaferInstance.Data.ResistLayer.State == ResistState.Full
                || !FabSequenceMgr.Instance.IsCurrStepAmong(validSteps)
                )
            {
                Debug.Log("Invalid prereqs");
                TryDeactivate();
                return;
            }

            Game.Events.Dispatch(GameEvents.StationStarted);

            // Auto assign dopant for now
            if (FabSequenceMgr.Instance.CurrStepID() == SequenceStepID.FillStencil_DOPE)
            {
                var chunk = FabSequenceMgr.Instance.CurrChunkID();
                if (chunk == ChunkID.N)
                {
                    AssignDopant(DopingType.N);
                }
                else if (chunk == ChunkID.P)
                {
                    AssignDopant(DopingType.P);
                }
            }

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Furnace)
            {
                if (AutomationMgr.Instance.CurrInstruction.DopantToApply == Research.DopantType.N)
                {
                    // generate dopant
                    // DopantMgr.Instance.NDispenser.Dispense(false);
                }
                else if (AutomationMgr.Instance.CurrInstruction.DopantToApply == Research.DopantType.P)
                {
                    // generate dopant
                    // DopantMgr.Instance.PDispenser.Dispense(false);
                }
            }

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
            m_currTemp = 0;

            TransitionCommon();
        }

        private void TransitionToFinished()
        {
            m_state = FurnaceMicrogameState.Finished;
            float precision = 1;
            if (m_finalTemp > TargetMaxTemp)
            {
                precision -= (m_finalTemp - TargetMaxTemp) / (MaxTemp - MinTemp);
            }
            else if (m_finalTemp < TargetMinTemp)
            {
                precision -= (TargetMinTemp - m_finalTemp) / (MaxTemp - MinTemp);
            }

            DragMgr.WaferInstance.SetOxideStateFurnace(precision, m_usedDopant, m_appliedDopant);
            // Auto wash
            DragMgr.WaferInstance.SetResistStateWash();
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
            TryStartFurnace();
        }

        private bool TryStartFurnace()
        {
            // check if valid combo
            // PREREQ: Oxide STRIPPED & DOPANT or Oxide EMPTY
            bool dopantMode = DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Stripped && m_usedDopant;
            bool emptyMode = DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Empty;
            if (dopantMode || emptyMode)
            {
                DragMgr.Instance.DragWaferEnabled = false;
                TransitionToHeating();
                return true;
            }
            else
            {
                Debug.Log("Invalid prereqs");
                TryDeactivate();
                return false;
            }
        }

        private void HandleStartHeat()
        {
            m_isHeating = true;
            ApplyHeatButtonRenderer.sprite = PressedButton;
        }

        private void HandleEndHeat()
        {
            m_isHeating = false;
            ApplyHeatButtonRenderer.sprite = DefaultButton;

            m_applyHeatRoutine.Replace(ApplyHeatRoutine());
        }

        private void FinishFurnace()
        {
            TryDeactivate();

            Game.Events.Dispatch(GameEvents.StationCompleted);
        }

        private void TryDeactivate()
        {
            if (ControlsMgr.Instance.BotEnabled)
            {
                DragMgr.Instance.DragWaferEnabled = true;
                RemoveDopant();
                Deactivate();
                ControlsMgr.Instance.BotInstance.TryCancelCurrStation();
                Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            }
            else
            {
                DragMgr.Instance.DragWaferEnabled = true;
                RemoveDopant();
                Deactivate();
                Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            }
        }

        private void HandleNewDopantCreated()
        {
            RemoveDopant();
        }

        #endregion // Handlers

        #region Routines

        private IEnumerator ApplyHeatRoutine()
        {
            var ratio = (m_finalTemp - MinTemp) / (MaxTemp - MinTemp);

            // Align Needle
            var angles = Gauge.transform.localEulerAngles;
            angles.z = -180 * ratio;

            yield return Routine.Combine(
                Gauge.transform.RotateTo(angles, 1, Axis.Z, Space.Self).Ease(Curve.CubeOut),
                MicrogameGauge.transform.RotateTo(angles, 1, Axis.Z, Space.Self).Ease(Curve.CubeOut)
                );

            yield return 2;

            angles.z = 0;
            yield return Routine.Combine(
                Gauge.transform.RotateTo(angles, 0.01f, Axis.Z, Space.Self).Ease(Curve.CubeOut),
                MicrogameGauge.transform.RotateTo(angles, 0.01f, Axis.Z, Space.Self).Ease(Curve.CubeOut)
                );

            HeatingCompleted = true;
        }

        #endregion // Routines

        public void AssignDopant(DopingType dopantType)
        {
            // TODO: assign
            m_usedDopant = true;
            m_appliedDopant = dopantType;
        }

        public void RemoveDopant()
        {
            m_usedDopant = false;
        }
    }
}