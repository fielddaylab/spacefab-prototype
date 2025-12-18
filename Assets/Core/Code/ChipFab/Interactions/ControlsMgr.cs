using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class ControlsMgr : MonoBehaviour
    {
        public static ControlsMgr Instance;

        public bool InputsEnabled = true;

        [Header("Drag")]
        public DragMgr DragMgr;
        public bool DragEnabled;

        [Header("Instantiation")]
        public Transform WaferDefaultPos;
        public Transform DopantDefaultPos;

        [Header("Nav Nodes")]
        public ControlNavNode StartingNode;
        public bool NavNodesEnabled;

        [Header("Nav Keys")]
        public KeyCode NavLeftKey = KeyCode.LeftArrow;
        public KeyCode NavRightKey = KeyCode.RightArrow;
        public KeyCode NavUpKey = KeyCode.UpArrow;
        public KeyCode NavDownKey = KeyCode.DownArrow;
        public KeyCode NavEnterNestKey = KeyCode.Equals;
        public KeyCode NavExitNestKey = KeyCode.Minus;
        public KeyCode NavInteractKey = KeyCode.Return;

        private ControlNavNode m_currNode;

        public DropZone CurrDropZone;

        [Header("Conveyor")]
        public ConveyorMgr ConveyorMgr;
        public bool ConveyorEnabled;

        [Header("Bot")]
        public WaferBot BotInstance;
        public bool BotEnabled;

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            DragMgr.gameObject.SetActive(DragEnabled);
            if (NavNodesEnabled)
            {
                GoToNode(StartingNode);
            }

            Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);
            Game.Events.Register(GameEvents.TimerBegin, HandleTimerBegin);
            Game.Events.Register(GameEvents.AutomationStarted, HandleAutomationStarted);
            Game.Events.Register(GameEvents.AutomationCompleted, HandleAutomationCompleted);
        }

        private void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }

            Game.Events.Deregister(GameEvents.NewWaferCreated, HandleNewWaferCreated);
            Game.Events.Deregister(GameEvents.TimerBegin, HandleTimerBegin);
            Game.Events.Deregister(GameEvents.AutomationStarted, HandleAutomationStarted);
            Game.Events.Deregister(GameEvents.AutomationCompleted, HandleAutomationCompleted);
        }

        private void Update()
        {
            ProcessInputs();
        }

        #endregion // Unity Callbacks

        #region Input

        private void ProcessInputs()
        {
            if (InputsEnabled)
            {
                if (NavNodesEnabled)
                {
                    ProcessNavNodeInputs();
                }
                if (ConveyorEnabled)
                {
                    ConveyorMgr.ProcessInputs();
                }
                if (BotEnabled)
                {
                    BotInstance.ProcessInputs();
                }
            }
        }

        private void ProcessNavNodeInputs()
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

            if (node.GetComponent<DropZone>())
            {
                CurrDropZone = node.GetComponent<DropZone>();
            }
            else
            {
                CurrDropZone = null;
            }

            // enter logic
            if (m_currNode != null)
            {
                if (m_currNode.Hoverable)
                {
                    m_currNode.Hoverable.BeginHover();
                }
            }
        }

        private void HandleNewWaferCreated()
        {
            if (ModeMgr.Instance.Mode == GameMode.Timed)
            {
                InputsEnabled = false; // wait for time mgr to re-enable inputs
            }
        }

        private void HandleTimerBegin()
        {
            if (ModeMgr.Instance.Mode == GameMode.Timed)
            {
                InputsEnabled = true;
                if (AutomationMgr.Instance.UsesAutomation)
                {
                    AutomationMgr.Instance.ActivelyChecking = true;
                }
            }
        }

        private void HandleAutomationStarted()
        {
            InputsEnabled = false;
        }

        private void HandleAutomationCompleted()
        {
            InputsEnabled = true;
        }
    }
}
