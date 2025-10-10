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
        public float Rotation;
    }


    public class AutomationMgr : MonoBehaviour
    {
        public static AutomationMgr Instance;

        public List<AutomationTrigger> AllTriggers;
        private List<AutomationTrigger> m_activeTriggers;

        [HideInInspector] public AutomationInstruction CurrInstruction = default;

        public bool ActivelyChecking = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            foreach(var trigger in AllTriggers) { 
                m_activeTriggers.Add(trigger);
            }

            CurrInstruction.Valid = false;

            Game.Events.Register(GameEvents.AutomationCompleted, HandleAutomationCompleted);
        }

        private void Update()
        {
            if (ActivelyChecking)
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

                    // move to target station
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
    }
}