using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    [CreateAssetMenu(menuName = "ChipFab/Create Level Data")]
    public class LevelSetupData : ScriptableObject
    {
        [SerializeField] private string m_levelId;
        [SerializeField] private List<StationId> m_availableStations;
        [SerializeField] private List<AutomationTrigger> m_automatedStationTriggers;
        [SerializeField] private WaferData m_initialWafer;
        [SerializeField] private WaferData m_targetWafer;
        [SerializeField] private FabSequence m_fabSequence;

        public string LevelId() { return m_levelId; }
        public List<StationId> AvailableStations() { return m_availableStations; }
        public List<AutomationTrigger> AutomatedStationTriggers() { return m_automatedStationTriggers; }
        public WaferData InitialWafer() { return m_initialWafer; }
        public WaferData TargetWafer() { return m_targetWafer; }
        public FabSequence FabSequence() { return m_fabSequence; }
    }
}