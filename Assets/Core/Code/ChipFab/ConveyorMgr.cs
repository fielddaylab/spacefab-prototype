using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum ConveyorState
    {
        Empty,
        Full
    }

    public class ConveyorMgr : MonoBehaviour
    {
        public static ConveyorMgr Instance;
        public List<ControlNavNode> Nodes;

        [Header("Nav Keys")]
        public KeyCode NavLeftKey = KeyCode.LeftArrow;
        public KeyCode NavRightKey = KeyCode.RightArrow;
        public KeyCode NavUpKey = KeyCode.UpArrow;
        public KeyCode NavDownKey = KeyCode.DownArrow;

        public ConveyorState State;

        public Transform CarryPos;

        private ControlNavNode m_currNode;
        private int m_currNodeIndex;

        private void Awake()
        {
            m_currNodeIndex = 0;
            m_currNode = Nodes[0];

            State = ConveyorState.Empty;

            Instance = this;
        }

        public void ProcessInputs()
        {
            if (Input.GetKeyDown(NavLeftKey))
            {
                if (State == ConveyorState.Full)
                {
                    TryShift(-1);
                }
            }
            else if (Input.GetKeyDown(NavRightKey))
            {
                if (State == ConveyorState.Full)
                {
                    TryShift(1);
                }
            }
            else if (Input.GetKeyDown(NavUpKey))
            {
                if (State == ConveyorState.Full)
                {
                    // try activate
                    if (m_currNode.GetComponent<IStationMicrogame>() != null)
                    {
                        State = ConveyorState.Empty;
                        ControlsMgr.Instance.CurrDropZone.AssignToDropZone(DragMgr.WaferInstance.transform);
                        m_currNode.GetComponent<IStationMicrogame>().Activate(DragMgr.WaferInstance);
                    }
                }
            }
            else if (Input.GetKeyDown(NavDownKey))
            {
                if (State == ConveyorState.Empty)
                {
                    // try cancel
                    if (m_currNode.GetComponent<IStationMicrogame>().TryCancel())
                    {
                        State = ConveyorState.Full;
                        SetAtIndex(m_currNodeIndex);
                    }
                }
            }
        }

        private void TryShift(int amt)
        {
            if (m_currNodeIndex + amt >= Nodes.Count || m_currNodeIndex + amt < 0)
            {
                return;
            }

            SetAtIndex(m_currNodeIndex + amt);
        }

        private void SetAtIndex(int index)
        {
            m_currNodeIndex = index;
            m_currNode = Nodes[m_currNodeIndex];

            var pos = DragMgr.WaferInstance.transform.position;
            pos.x = m_currNode.transform.position.x;
            pos.y = CarryPos.transform.position.y;
            DragMgr.WaferInstance.transform.position = pos;

            ControlsMgr.Instance.CurrDropZone = m_currNode.GetComponent<DropZone>();
        }

        public void AssignWafer()
        {
            State = ConveyorState.Full;
            SetAtIndex(0);
        }
    }
}