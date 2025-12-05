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

        private Routine m_StencilFillRoutine;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            DragMgr.Instance.DragWaferEnabled = false;
            SprayerBox.OnMouseDown.AddListener(HandleSprayMouseDown);
            StencilFill.enabled = false;

            Sprayer.localPosition = SprayStartPos;
            fillCount = 0;

            foreach (var dot in FillDots)
            {
                dot.enabled = false;
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
            Vector3 moveVector = Vector3.zero;

            if (Input.GetKey(UP_KEY))
            {
                moveVector += Vector3.up;
            }
            if (Input.GetKey(DOWN_KEY))
            {
                moveVector += Vector3.down;
            }
            if (Input.GetKey(LEFT_KEY))
            {
                moveVector += Vector3.left;
            }
            if (Input.GetKey(RIGHT_KEY))
            {
                moveVector += Vector3.right;
            }

            moveVector = moveVector.normalized;
            moveVector *= SprayerMoveSpeed * Time.deltaTime;

            Sprayer.transform.localPosition += moveVector;

            if (Input.GetKey(FIRE_KEY))
            {
                HandleSprayMouseDown();
            }

            // Check if sufficiently sprayed
            if (fillCount == FillDots.Count)
            {
                HandleFinishClicked();
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
            Vector3 sprayPos = Aim.position;

            Collider2D hit = Physics2D.OverlapPoint(sprayPos, FillDotLayer);
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