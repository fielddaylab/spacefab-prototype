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

        private static KeyCode LEFT_KEY = KeyCode.LeftArrow;
        private static KeyCode RIGHT_KEY = KeyCode.RightArrow;
        private static KeyCode UP_KEY = KeyCode.UpArrow;
        private static KeyCode DOWN_KEY = KeyCode.DownArrow;

        private static KeyCode LEFT_KEY2 = KeyCode.A;
        private static KeyCode RIGHT_KEY2 = KeyCode.D;
        private static KeyCode UP_KEY2 = KeyCode.W;
        private static KeyCode DOWN_KEY2 = KeyCode.S;

        public Vector3 SprayStartPos;
        public Transform Sprayer;
        public float SprayerMoveSpeed;
        public Transform Aim;

        public LayerMask FillDotLayer;
        public List<SpriteRenderer> FillDots;

        private int fillCount = 0;

        private SputteringMicrogameState m_state;

        public ClickBox SprayerBox;
        public SpriteRenderer StencilFill;

        private bool AwaitingKeyUp;

        private Routine m_StencilFillRoutine;

        #region IStationMicrogame

        public override void Activate(WaferState waferState, bool isAutomated)
        {
            base.Activate(waferState, isAutomated);

            DragMgr.Instance.DragWaferEnabled = false;
            SprayerBox.OnMouseDown.AddListener(HandleSprayMouseDown);
            StencilFill.enabled = false;

            Sprayer.localPosition = SprayStartPos;
            fillCount = 0;

            foreach (var dot in FillDots)
            {
                dot.enabled = false;
            }

            if (Input.GetKey(FIRE_KEY))
            {
                AwaitingKeyUp = true;
            }

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
            if (!Container.activeInHierarchy && !IsCurrentSessionAutomated) { return; }

            if (AwaitingKeyUp)
            {
                if (Input.GetKeyUp(FIRE_KEY))
                {
                    AwaitingKeyUp = false;
                }
            }

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
                ProcessAutomation();
            }
            else
            {
                ProcessManual();
            }
        }

        private void ProcessManual()
        {
            Vector3 moveVector = Vector3.zero;

            if (Input.GetKey(UP_KEY) || Input.GetKey(UP_KEY2))
            {
                moveVector += Vector3.up;
            }
            if (Input.GetKey(DOWN_KEY) || Input.GetKey(DOWN_KEY2))
            {
                moveVector += Vector3.down;
            }
            if (Input.GetKey(LEFT_KEY) || Input.GetKey(LEFT_KEY2))
            {
                moveVector += Vector3.left;
            }
            if (Input.GetKey(RIGHT_KEY) || Input.GetKey(RIGHT_KEY2))
            {
                moveVector += Vector3.right;
            }

            moveVector = moveVector.normalized;
            moveVector *= SprayerMoveSpeed * Time.deltaTime;

            Sprayer.transform.localPosition += moveVector;

            if (AwaitingKeyUp)
            {
                if (Input.GetKeyUp(FIRE_KEY))
                {
                    AwaitingKeyUp = false;
                }
            }
            else
            {
                if (Input.GetKey(FIRE_KEY))
                {
                    HandleSprayMouseDown();
                }
            }

            // Check if sufficiently sprayed
            if (fillCount == FillDots.Count)
            {
                HandleFinishClicked();
            }
        }

        private void ProcessAutomation()
        {
            if (!m_AutomationRoutine.Exists())
            {
                m_AutomationRoutine.Replace(BasicAutomationRoutine());
            }
        }

        private IEnumerator AutomationRoutine()
        {

            yield return 0.5f;

            foreach (var dot in FillDots)
            {
                dot.enabled = true;
                // fillCount++;
            }

            yield return 0.5f;

            HandleFinishClicked();
        }

        private IEnumerator BasicAutomationRoutine()
        {
            yield return AUTOMATION_TIME;

            var precision = 1;
            SetWaferState(precision);

            Cleanup();
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
                Debug.Log("Invalid Prereqs");
                Game.Events.Dispatch(GameEvents.IncorrectStationAttempted);
                TryDeactivate();
            }

            Game.Events.Dispatch(GameEvents.StationStarted);
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
            SetWaferState(precision);

            Cleanup();
        }

        private void HandleSprayMouseDown()
        {
            Vector3 sprayPos = Aim.position;

            Collider2D[] hits = Physics2D.OverlapPointAll(sprayPos, FillDotLayer);
            foreach (var hit in hits)
            {
                if (hit != null)
                {
                    SpriteRenderer dot = hit.GetComponent<SpriteRenderer>();
                    if (dot && !dot.enabled)
                    {
                        dot.enabled = true;
                        fillCount++;
                    }
                }

                /*
                if (m_StencilFillRoutine.Exists())
                {
                    return;
                }

                m_StencilFillRoutine.Replace(StencilFillRoutine());
                */
            }
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

        private void SetWaferState(float precision)
        {
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
        }

        private void Cleanup()
        {
            TryDeactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            Game.Events.Dispatch(GameEvents.StationCompleted);
        }
    }
}