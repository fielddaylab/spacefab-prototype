using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class ControlsMgr : MonoBehaviour
    {
        public static ControlsMgr Instance;

        [Header("Drag")]
        public DragMgr DragMgr;
        public bool DragEnabled;

        [Header("Instantiation")]
        public Transform WaferDefaultPos;
        public Transform DopantDefaultPos;

        [Header("Nav Nodes")]
        public ControlNavNode StartingNode;

        [Header("Nav Keys")]
        public KeyCode NavLeftKey = KeyCode.LeftArrow;
        public KeyCode NavRightKey = KeyCode.RightArrow;
        public KeyCode NavUpKey = KeyCode.UpArrow;
        public KeyCode NavDownKey = KeyCode.DownArrow;
        public KeyCode NavEnterNestKey = KeyCode.Equals;
        public KeyCode NavExitNestKey = KeyCode.Minus;
        public KeyCode NavInteractKey = KeyCode.Return;

        private ControlNavNode m_currNode;


        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            DragMgr.gameObject.SetActive(DragEnabled);
            GoToNode(StartingNode);
        }

        private void Update()
        {
            ProcessInputs();
        }

        #endregion // Unity Callbacks

        #region Input

        private void ProcessInputs()
        {
            if (Input.GetKeyDown(NavLeftKey))
            {
                if (m_currNode && m_currNode.LeftNode != null)
                {
                    GoToNode(m_currNode.LeftNode);
                }
            }
            else if (Input.GetKeyDown(NavRightKey))
            {
                if (m_currNode && m_currNode.RightNode != null)
                {
                    GoToNode(m_currNode.RightNode);
                }
            }
            else if (Input.GetKeyDown(NavUpKey))
            {
                if (m_currNode && m_currNode.UpNode != null)
                {
                    GoToNode(m_currNode.UpNode);
                }
            }
            else if (Input.GetKeyDown(NavDownKey))
            {
                if (m_currNode && m_currNode.DownNode != null)
                {
                    GoToNode(m_currNode.DownNode);
                }
            }
            else if (Input.GetKeyDown(NavEnterNestKey))
            {
                if (m_currNode && m_currNode.EnterNestNode != null)
                {
                    GoToNode(m_currNode.EnterNestNode);
                }
            }
            else if (Input.GetKeyDown(NavExitNestKey))
            {
                if (m_currNode && m_currNode.ExitNestNode != null)
                {
                    GoToNode(m_currNode.ExitNestNode);
                }
            }
            else if (Input.GetKeyDown(NavInteractKey))
            {
                if (m_currNode && m_currNode.Interactable != null)
                {
                    m_currNode.Interactable.Interact();
                }
            }
        }

        #endregion // Input

        private void GoToNode(ControlNavNode node)
        {
            // exit logic
            if (m_currNode != null)
            {
                if (m_currNode.Hoverable)
                {
                    m_currNode.Hoverable.EndHover();
                }
            }

            // assign
            m_currNode = node;

            // enter logic
            if (m_currNode != null)
            {
                if (m_currNode.Hoverable)
                {
                    m_currNode.Hoverable.BeginHover();
                }
            }
        }
    }
}
