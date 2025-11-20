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

        private ControlNavNode m_currNode;
        private int m_currNodeIndex;

        private void Start()
        {
            m_currNodeIndex = 0;
            m_currNode = NavNodesMgr.Instance.Nodes[0];

            SetAtIndex(m_currNodeIndex);

            State = ConveyorState.Uninitialized;

            Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);
        }

        public void SetCurrNode(int index)
        {
            m_currNode = NavNodesMgr.Instance.Nodes[index];
        }

        public void ProcessInputs()
        {
            if (Input.GetKeyDown(NavLeftKey))
            {
                if (State == ConveyorState.Full || State == ConveyorState.Uninitialized)
                {
                    TryShift(-1);
                }
            }
            else if (Input.GetKeyDown(NavRightKey))
            {
                if (State == ConveyorState.Full || State == ConveyorState.Uninitialized)
                {
                    TryShift(1);
                }
            }
            else if (Input.GetKeyDown(NavUpKey))
            {
                if (State == ConveyorState.Full)
                {
                    // try activate
                    TryActivateCurrStation();
                }
            }
            else if (Input.GetKeyDown(NavDownKey))
            {
                if (State == ConveyorState.Empty && DragMgr.WaferInstance != null)
                {
                    // try cancel
                    TryCancelCurrStation();
                }
            }
        }

        private void TryShift(int amt)
        {
            if (m_currNodeIndex + amt >= NavNodesMgr.Instance.Nodes.Count || m_currNodeIndex + amt < 0)
            {
                return;
            }

            SetAtIndex(m_currNodeIndex + amt);
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
            CamMgr.Instance.UnloadCamPos(m_currNode.GetComponent<StationMicrogame>().CamPos.Pos);
            SetAtIndex(m_currNodeIndex);
        }

        private void SetAtIndex(int index)
        {
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
                CamMgr.Instance.LoadCamPos(station.CamPos.Pos);
            }
        }

        private void HandleNewWaferCreated()
        {
            State = ConveyorState.Full;
            SetAtIndex(m_currNodeIndex);
        }
    }
}