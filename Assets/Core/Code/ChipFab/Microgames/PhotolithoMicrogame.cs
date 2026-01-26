using BeauRoutine;
using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class PhotolithoMicrogame : StationMicrogame, IStationMicrogame
    {
        private static KeyCode StartKey = KeyCode.Space;

        private bool m_autoRoutineStarted = false;
        public EtchMaskData CurrMaskData;
        private MaskId m_currSelectedMask = MaskId.NONE;

        [Header("Etch")]
        public LineRenderer TargetPath;
        public Transform TargetCircle;
        public LineRenderer PlayerPath;
        public Transform PlayerCircle;

        public float TracerSpeed = 1.5f;
        public float SampleTime = 1f;

        private int TargetPointIndex;
        private bool TracerReachedNext;
        private float SampleTimer;

        private Vector2 PlayerMoveDir;
        private Vector2 LastKnownMoveDir;
        private bool NewMoveDir;

        private bool Activated;

        private float TotalDif;
        private int NumSamples;

        private bool TraceStarted;

        private Routine m_startupRoutine;

        public override void Activate(WaferState waferState, bool isAutomated)
        {
            base.Activate(waferState, isAutomated);

            m_autoRoutineStarted = false;
            m_currSelectedMask = CurrMaskData.MaskId;
            TargetPath.positionCount = CurrMaskData.TracePoints.Length;
            TargetPath.SetPositions(CurrMaskData.TracePoints);
            TargetPointIndex = 0;
            TracerReachedNext = true;
            TargetCircle.transform.localPosition = CurrMaskData.TracePoints[0];
            SampleTimer = SampleTime;
            PlayerMoveDir = new Vector2(1, 0);
            LastKnownMoveDir = PlayerMoveDir;
            NewMoveDir = true;
            TraceStarted = false;

            ControlsMgr.Instance.InputsEnabled = false;

            PlayerCircle.transform.localPosition = CurrMaskData.TracePoints[0];
            PlayerPath.positionCount = 1;
            PlayerPath.SetPosition(PlayerPath.positionCount - 1, PlayerCircle.transform.localPosition);

            var validSteps = new List<SequenceStepID>() {
                SequenceStepID.DrawPattern,
            };

            // PREREQS: Resist FULL
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Full
                || !FabSequenceMgr.Instance.IsCurrStepAmong(validSteps)
                )
            {
                Debug.Log("Invalid Prereqs");
                Game.Events.Dispatch(GameEvents.IncorrectStationAttempted);
                TryDeactivate();
                return;
            }

            Game.Events.Dispatch(GameEvents.StationStarted);

            m_startupRoutine.Replace(StartupRoutine());
        }

        public override void Deactivate()
        {
            Activated = false;

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Photolithograph)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }

            base.Deactivate();

            ControlsMgr.Instance.InputsEnabled = true;
        }

        private void TryDeactivate()
        {
            Deactivate();

            if (ControlsMgr.Instance.BotEnabled)
            {
                ControlsMgr.Instance.BotInstance.TryCancelCurrStation();
            }
        }

        #region Unity Callbacks

        private void Update()
        {
            if (!Activated) { return; }

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Photolithograph)
            {
                /*
                if (!m_autoRoutineStarted)
                {
                    m_AutomationRoutine.Replace(AutomationRoutine());
                    m_autoRoutineStarted = true;
                }
                */
                ProcessMicrogame();
            }
            else
            {
                ProcessMicrogame();
            }
        }

        #endregion // Unity Callbacks

        public override bool TryCancel()
        {
            Deactivate();
            return true;
        }

        private void ProcessMicrogame()
        {
            if (IsCurrentSessionAutomated)
            {
                ProcessAutomation();
            }
            else if (TraceStarted)
            {
                ProcessManual();
            }
            else
            {
                AwaitTraceStart();
            }
        }

        private void AwaitTraceStart()
        {
            if (Input.GetKeyDown(StartKey))
            {
                TraceStarted = true;
            }
        }

        private void ProcessManual()
        {
            // Handle inputs
            ProcessInputs();

            if (NewMoveDir)
            {
                PlayerPath.positionCount++;
                NewMoveDir = false;
            }

            PlayerPath.SetPosition(PlayerPath.positionCount - 1, PlayerCircle.transform.localPosition);

            ProcessPlayerTracer();

            // Calculate Differences
            SampleTimer -= Time.deltaTime;
            if (SampleTimer <= 0)
            {
                SampleTimer = SampleTime;
                // Sample
                NumSamples++;

                float dif = 0;
                if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Photolithograph)
                {
                    dif = 0;
                }
                else
                {
                    dif = Vector3.Distance(PlayerCircle.transform.localPosition, TargetCircle.transform.localPosition);
                }

                TotalDif += dif;
            }

            ProcessTargetTracer();
        }

        private void ProcessInputs()
        {
            var newDir = Vector2.zero;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                newDir.y = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                newDir.y = -1;
            }

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                newDir.x = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                newDir.x = -1;
            }

            newDir = newDir.normalized;

            if (newDir != Vector2.zero)
            {
                if (newDir != PlayerMoveDir)
                {
                    PlayerMoveDir = newDir;
                    LastKnownMoveDir = PlayerMoveDir;
                    NewMoveDir = true;
                }
            }
        }

        private void ProcessPlayerTracer()
        {
            var playerVector = PlayerMoveDir * TracerSpeed * Time.deltaTime;
            PlayerCircle.transform.localPosition += new Vector3(playerVector.x, playerVector.y, 0);
        }

        private void ProcessTargetTracer()
        {
            // Tracer auto follow path
            if (TracerReachedNext)
            {
                TracerReachedNext = false;
                // get next
                var nextIndex = TargetPointIndex + 1;
                if (nextIndex < CurrMaskData.TracePoints.Length)
                {
                    TargetPointIndex = nextIndex;
                }
                else
                {
                    // reached end
                    HandleDevelopEnded();
                    return;
                }
            }

            // continue moving
            float margin = 0.1f;
            var vector = CurrMaskData.TracePoints[TargetPointIndex] - TargetCircle.transform.localPosition;
            if (vector.magnitude <= margin)
            {
                TargetCircle.transform.localPosition = CurrMaskData.TracePoints[TargetPointIndex];
                TracerReachedNext = true;
            }
            else
            {
                var moveVector = vector.normalized;
                moveVector *= TracerSpeed * Time.deltaTime;
                TargetCircle.transform.localPosition += moveVector;
            }
        }

        private void ProcessAutomation()
        {
            if (!m_AutomationRoutine.Exists())
            {
                m_AutomationRoutine.Replace(BasicAutomationRoutine());
            }
        }

        private IEnumerator StartupRoutine()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Photolithograph)
            {
                PlayerPath.enabled = false;
                PlayerCircle.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                PlayerPath.enabled = true;
                PlayerCircle.GetComponent<SpriteRenderer>().enabled = true;
            }

            yield return 1;

            Activated = true;
        }

        private IEnumerator BasicAutomationRoutine()
        {
            yield return AUTOMATION_TIME;

            var precision = 1;
            SetWaferState(precision);

            Cleanup();
        }

        private void SetWaferState(float precision)
        {
            DragMgr.WaferInstance.SetPhotoState(m_currSelectedMask, 0, precision);
        }

        private void Cleanup()
        {
            TryDeactivate();

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
            Game.Events.Dispatch(GameEvents.StationCompleted);
        }

        #region Handlers

        private IEnumerator AutomationRoutine()
        {
            yield return null;
            /*
            yield return 0.5f;

            var instruction = AutomationMgr.Instance.CurrInstruction;

            yield return 0.5f;

            HandleDevelopEnded();
            */
        }

        private void HandleDevelopEnded()
        {
            // TODO: normalize diff values
            var precision = 1 - (TotalDif / NumSamples);
            SetWaferState(precision);

            Cleanup();
        }

        #endregion // Handlers
    }
}