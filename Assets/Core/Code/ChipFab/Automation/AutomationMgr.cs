using FieldDay;
using SpaceFab.Research;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    [Serializable]
    public struct AutomationTrigger
    {
        public bool Disable;
        public WaferData TriggerState;
        public AutomationInstruction Instruction;
    }

    [Serializable]
    public struct AutomationInstruction
    {
        [HideInInspector] public bool Valid;
        public StationId TargetStation;

        [Header("Furnace")]
        public DopantType DopantToApply;
        public float Temperature;

        [Header("Photolithograph")]
        public MaskId MaskToApply;
        public int Rotation;
    }


    public class AutomationMgr : MonoBehaviour
    {
        public static AutomationMgr Instance;

        private List<AutomationTrigger> m_allTriggers = new List<AutomationTrigger>();
        private List<AutomationTrigger> m_activeTriggers = new List<AutomationTrigger>();

        [HideInInspector] public AutomationInstruction CurrInstruction = default;

        public bool ActivelyChecking = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CurrInstruction.Valid = false;

            Game.Events.Register(GameEvents.AutomationCompleted, HandleAutomationCompleted);
            Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);

            m_allTriggers = ChipFabConfig.Instance.CurrLevel.AutomatedStationTriggers();

            ResetTriggers();
        }

        private void Update()
        {
            if (ActivelyChecking && !CurrInstruction.Valid)
            {
                CheckForAutomation();
            }
        }

        private void CheckForAutomation()
        {
            for (int i = 0; i < m_activeTriggers.Count; i++)
            {
                if (ConditionsMet(m_activeTriggers[i]))
                {
                    CurrInstruction = m_activeTriggers[i].Instruction;
                    CurrInstruction.Valid = true;
                    m_activeTriggers.RemoveAt(i);

                    Game.Events.Dispatch(GameEvents.AutomationStarted);

                    // move to target station
                    var stationIndex = GetStationIndex(CurrInstruction.TargetStation);
                    SetWaferAtIndex(stationIndex);
                    break;
                }
            }
        }

        private bool ConditionsMet(AutomationTrigger trigger)
        {
            if (DragMgr.WaferInstance == null) { return false; }

            return WaferData.IsEqual(DragMgr.WaferInstance.Data, trigger.TriggerState);
        }

        private void HandleAutomationCompleted()
        {
            CurrInstruction.Valid = false;
        }

        private void HandleNewWaferCreated()
        {
            ResetTriggers();
        }

        private void ResetTriggers()
        {
            m_activeTriggers.Clear();

            foreach (var trigger in m_allTriggers)
            {
                if (trigger.Disable) { continue; }
                m_activeTriggers.Add(trigger);
            }
        }

        private int GetStationIndex(StationId id)
        {
            for (int i = 0; i < NavNodesMgr.Instance.Nodes.Count; i++)
            {
                var station = NavNodesMgr.Instance.Nodes[i].GetComponent<Station>();
                if (station != null && station.Id == id)
                {
                    return i;
                }
            }
            return -1;
        }

        private void SetWaferAtIndex(int index)
        {
            var currNode = NavNodesMgr.Instance.Nodes[index];
            ConveyorMgr.Instance.SetCurrNode(index);

            var pos = DragMgr.WaferInstance.transform.position;
            pos.x = currNode.transform.position.x;
            pos.y = currNode.transform.position.y;
            DragMgr.WaferInstance.transform.position = pos;

            DragMgr.WaferInstance.transform.rotation = default;

            ControlsMgr.Instance.CurrDropZone = currNode.GetComponent<DropZone>();
            if (ControlsMgr.Instance.ConveyorEnabled)
            {
                ConveyorMgr.Instance.TryActivateCurrStation();
            } 
            else
            {
                ControlsMgr.Instance.CurrDropZone.AssignToDropZone(DragMgr.WaferInstance.transform);
                currNode.GetComponent<IStationMicrogame>().Activate(DragMgr.WaferInstance);
            }
        }
    }
}