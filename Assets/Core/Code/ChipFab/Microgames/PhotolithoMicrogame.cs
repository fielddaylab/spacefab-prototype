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

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

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

            Activated = true;

            ControlsMgr.Instance.InputsEnabled = false;

            PlayerCircle.transform.localPosition = CurrMaskData.TracePoints[0];
            PlayerPath.positionCount = 1;
            PlayerPath.SetPosition(PlayerPath.positionCount - 1, PlayerCircle.transform.localPosition);

            // PREREQS: Resist FULL
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Full)
            {
                Debug.Log("Invalid Prereqs");
                // Deactivate();
            }
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

        #region Unity Callbacks

        private void Update()
        {
            if (!Activated) { return; }

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Photolithograph)
            {
                if (!m_autoRoutineStarted)
                {
                    m_AutomationRoutine.Replace(AutomationRoutine());
                    m_autoRoutineStarted = true;
                }
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
                var dif = Vector3.Distance(PlayerCircle.transform.localPosition, TargetCircle.transform.localPosition);
                TotalDif += dif;
            }

            ProcessTargetTracer();
        }

        private void ProcessInputs()
        {
            var newDir = Vector2.zero;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                newDir.y = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                newDir.y = -1;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                newDir.x = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
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

        #region Handlers

        private IEnumerator AutomationRoutine()
        {
            yield return 0.5f;

            var instruction = AutomationMgr.Instance.CurrInstruction;

            yield return 0.5f;

            HandleDevelopEnded();
        }

        private void HandleDevelopEnded()
        {
            // TODO: normalize diff values
            var precision = 1 - (TotalDif / NumSamples);
            DragMgr.WaferInstance.SetPhotoState(m_currSelectedMask, 0, precision);
            Deactivate();

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }

        #endregion // Handlers
    }
}