using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class WaferBot : MonoBehaviour
    {
        [Header("Nav Keys")]
        public KeyCode NavLeftKey = KeyCode.LeftArrow;
        public KeyCode NavRightKey = KeyCode.RightArrow;
        public KeyCode NavUpKey = KeyCode.UpArrow;
        public KeyCode NavDownKey = KeyCode.DownArrow;

        public ConveyorState State;

        public float WaferOffset = 1.5f;

        public GameObject StartBotPrompt;
        public GameObject StunDialogue;

        public bool IsStunned = false;
        public const float StunTime = 4;

        public Transform BodyTransform;

        [Space(5)]
        [Header("Faces")]
        public SpriteRenderer FaceRenderer;
        public Sprite DefaultFace;
        public Sprite StunnedFace;

        private float StunTimer = 0;

        private ControlNavNode m_currNode;
        private int m_currNodeIndex;

        private bool m_inMotion;

        private Routine m_moveRoutine;
        private Routine m_stunRoutine;

        private void Start()
        {
            IsStunned = false;
            StunDialogue.SetActive(false);
            m_currNodeIndex = 0;
            m_currNode = NavNodesMgr.Instance.Nodes[0];

            SetAtIndex(m_currNodeIndex);

            State = ConveyorState.Uninitialized;

            StartBotPrompt.SetActive(true);

            Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);
            Game.Events.Register(GameEvents.IncorrectStationAttempted, HandleIncorrectStationAttempted);
        }

        private void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }
            Game.Events.Deregister(GameEvents.NewWaferCreated, HandleNewWaferCreated);
            Game.Events.Deregister(GameEvents.IncorrectStationAttempted, HandleIncorrectStationAttempted);
        }

        public void SetCurrNode(int index)
        {
            m_currNode = NavNodesMgr.Instance.Nodes[index];
        }

        public void ProcessInputs()
        {
            if (IsStunned) { return; }

            if (Input.GetKeyDown(NavLeftKey))
            {
                if (State == ConveyorState.Full || State == ConveyorState.Uninitialized)
                {
                    if (!m_inMotion)
                    {
                        TryShift(-1);
                    }
                }
            }
            else if (Input.GetKeyDown(NavRightKey))
            {
                if (State == ConveyorState.Full || State == ConveyorState.Uninitialized)
                {
                    if (!m_inMotion)
                    {
                        TryShift(1);
                    }
                }
            }
            else if (Input.GetKeyDown(NavUpKey))
            {
                if (State == ConveyorState.Full)
                {
                    if (!m_inMotion)
                    {
                        // try activate
                        TryActivateCurrStation();
                    }
                }
            }
            else if (Input.GetKeyDown(NavDownKey))
            {
                if (State == ConveyorState.Empty && DragMgr.WaferInstance != null)
                {
                    if (!m_inMotion)
                    {
                        // try cancel
                        TryCancelCurrStation();
                    }
                }
            }
        }

        private void TryShift(int amt)
        {
            if (m_currNodeIndex + amt >= NavNodesMgr.Instance.Nodes.Count || m_currNodeIndex + amt < 0)
            {
                return;
            }

            m_moveRoutine.Replace(MoveToIndex(m_currNodeIndex + amt));
        }

        private IEnumerator MoveToIndex(int index)
        {
            m_inMotion = true;

            yield return SetAtIndexRoutine(index);

            m_inMotion = false;
        }

        public void TryActivateCurrStation()
        {
            if (m_currNode.GetComponent<IStationMicrogame>() != null)
            {
                State = ConveyorState.Empty;
                ControlsMgr.Instance.CurrDropZone.AssignToDropZone(DragMgr.WaferInstance.transform);
                m_currNode.GetComponent<IStationMicrogame>().Activate(DragMgr.WaferInstance);
            }
        }

        public void TryCancelCurrStation()
        {
            if (m_currNode.GetComponent<IStationMicrogame>() != null)
            {
                if (m_currNode.GetComponent<IStationMicrogame>().TryCancel())
                {
                    TryReturnToConveyor();
                }
            }
        }

        public bool IsAtStation(IStationMicrogame station)
        {
            return m_currNode.GetComponent<IStationMicrogame>() == station;
        }

        public void TryReturnToConveyor()
        {
            State = ConveyorState.Full;
            CamMgr.Instance.UnloadCamPosImmediate(m_currNode.GetComponent<StationMicrogame>().CamPos.Pos);
            SetAtIndex(m_currNodeIndex);
        }

        public void SetAtIndex(int index)
        {
            if (ControlsMgr.Instance.CurrDropZone)
            {
                var prevStation = ControlsMgr.Instance.CurrDropZone.GetComponent<StationMicrogame>();
                if (prevStation && prevStation.ActivateGroup)
                {
                    prevStation.ActivateGroup.SetActive(false);
                }
            }

            m_currNodeIndex = index;
            m_currNode = NavNodesMgr.Instance.Nodes[m_currNodeIndex];

            var pos = this.transform.position;
            pos.x = m_currNode.transform.position.x;
            this.transform.position = pos;

            if (DragMgr.WaferInstance)
            {
                DragMgr.WaferInstance.transform.parent = this.transform;
                pos.x = 0;
                pos.y = WaferOffset;
                DragMgr.WaferInstance.transform.localPosition = pos;
                DragMgr.WaferInstance.transform.rotation = default;
            }

            ControlsMgr.Instance.CurrDropZone = m_currNode.GetComponent<DropZone>();

            var station = ControlsMgr.Instance.CurrDropZone.GetComponent<StationMicrogame>();
            if (station)
            {
                CamMgr.Instance.LoadCamPosImmediate(station.CamPos.Pos);

                if (station.ActivateGroup)
                {
                    station.ActivateGroup.SetActive(true);
                }
            }
        }

        public IEnumerator SetAtIndexRoutine(int index)
        {
            if (ControlsMgr.Instance.CurrDropZone)
            {
                var prevStation = ControlsMgr.Instance.CurrDropZone.GetComponent<StationMicrogame>();
                if (prevStation && prevStation.ActivateGroup)
                {
                    prevStation.ActivateGroup.SetActive(false);
                }
            }

            m_currNodeIndex = index;
            m_currNode = NavNodesMgr.Instance.Nodes[m_currNodeIndex];

            var pos = this.transform.position;
            pos.x = m_currNode.transform.position.x;

            if (DragMgr.WaferInstance)
            {
                DragMgr.WaferInstance.transform.parent = this.transform;
                pos.x = 0;
                pos.y = WaferOffset;
                DragMgr.WaferInstance.transform.localPosition = pos;
                DragMgr.WaferInstance.transform.rotation = default;
            }

            ControlsMgr.Instance.CurrDropZone = m_currNode.GetComponent<DropZone>();

            var station = ControlsMgr.Instance.CurrDropZone.GetComponent<StationMicrogame>();
            if (station)
            {
                yield return Routine.Combine(
                    CamMgr.Instance.LoadCamPosRoutine(station.CamPos.Pos, 0.1f),
                    MoveBotRoutine(0.1f)
                    );

                if (station.ActivateGroup)
                {
                    station.ActivateGroup.SetActive(true);
                }
            }
            else
            {
                yield return null;
            }
        }

        private IEnumerator MoveBotRoutine(float time)
        {
            var pos = this.transform.position;
            pos.x = m_currNode.transform.position.x;
            yield return this.transform.MoveTo(pos, time, Axis.X);
        }

        private void HandleNewWaferCreated()
        {
            State = ConveyorState.Full;
            SetAtIndex(m_currNodeIndex);

            CancelStun();

            StartBotPrompt.SetActive(false);
        }

        private void HandleIncorrectStationAttempted()
        {
            if (!IsStunned)
            {
                StartStun();
            }
        }

        private void StartStun()
        {
            StunTimer = StunTime;
            IsStunned = true;
            FaceRenderer.sprite = StunnedFace;
            StunDialogue.SetActive(true);
            m_stunRoutine.Replace(BeginStunRoutine());
        }

        private void EndStun()
        {
            IsStunned = false;
            FaceRenderer.sprite = DefaultFace;
            StunDialogue.SetActive(false);
        }

        private void CancelStun()
        {
            IsStunned = false;
            FaceRenderer.sprite = DefaultFace;
            StunDialogue.SetActive(false);
        }

        public void ProgressStunTimer()
        {
            StunTimer -= Time.deltaTime;
            if (StunTimer < 0)
            {
                EndStun();
            }
        }

        private IEnumerator BeginStunRoutine()
        {
            var targetVector = BodyTransform.position + Vector3.one * 0.3f;
            yield return BodyTransform.MoveTo(targetVector, 0.25f, Axis.X, Space.Self).Wave(Wave.Function.CosFade, 3);
        }
    }
}