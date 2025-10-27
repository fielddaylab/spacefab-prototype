using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class StationMgr : MonoBehaviour
    {
        public static StationMgr Instance;

        private List<Station> m_allStations = new List<Station>();

        private void Awake()
        {
            Instance = this;   
        }

        private void Start()
        {
            foreach(var station in m_allStations)
            {
                station.gameObject.SetActive(ChipFabConfig.Instance.CurrLevel.AvailableStations().Contains(station.Id));
            }
        }

        public void RegisterStation(Station station)
        {
            if (m_allStations.Contains(station)) { return; }
            m_allStations.Add(station);
        }
    }
}