using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    [Serializable]
    public struct AutoInstructionsData
    {
        public StationId Station;
    }

    [CreateAssetMenu(menuName = "ChipFab/Create Level Data")]
    public class LevelSetupData : ScriptableObject
    {
        [SerializeField] private string m_levelId;
        [SerializeField] private List<StationId> m_availableStations;
        [SerializeField] private List<AutoInstructionsData> m_automatedStationData;
        [SerializeField] private WaferData m_initialWafer;

        public string LevelId() { return m_levelId; }
        public List<StationId> AvailableStations() { return m_availableStations; }
        public List<AutoInstructionsData> AutomatedStationData() { return m_automatedStationData; }
        public WaferData InitialWafer() { return m_initialWafer; }
    }
}