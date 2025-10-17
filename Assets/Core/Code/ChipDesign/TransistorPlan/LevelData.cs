using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{

    public enum Placeable
    {
        VMINUS,
        VPLUS,
        IN,
        A,
        B,
        NNODE,
        PNODE,
        OUT
    }

    [CreateAssetMenu(menuName = "Chip Design/New Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private List<Placeable> m_allowedPlacables;
        [SerializeField] private GridStackConfig m_gridConfig;

        public List<Placeable> GetPlaceables() { return m_allowedPlacables; }
        public GridStackConfig GetGridConfig() { return m_gridConfig; }
    }

    public class LevelDataCopy
    {
        [SerializeField] private List<Placeable> m_allowedPlacables;
        [SerializeField] private GridStackConfig m_gridConfig;

        public List<Placeable> GetPlaceables() { return m_allowedPlacables; }
        public GridStackConfig GetGridConfig() { return m_gridConfig; }


        public void LoadData(LevelData data)
        {
            m_allowedPlacables = data.GetPlaceables();
            m_gridConfig = data.GetGridConfig();
        }
    }
}