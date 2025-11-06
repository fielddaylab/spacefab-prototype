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
        [SerializeField] private List<Placeable> m_allowedPlaceables;
        [SerializeField] private GridStackConfig m_gridConfig;
        [SerializeField] private TestSuiteData m_testSuite;

        public List<Placeable> GetPlaceables() { return m_allowedPlaceables; }
        public GridStackConfig GetGridConfig() { return m_gridConfig; }
        public TestSuiteData GetTestSuite() { return m_testSuite; }
    }

    public class LevelDataCopy
    {
        [SerializeField] private List<Placeable> m_allowedPlaceables;
        [SerializeField] private GridStackConfig m_gridConfig;
        [SerializeField] private TestSuiteData m_testSuite;

        public List<Placeable> GetPlaceables() { return m_allowedPlaceables; }
        public GridStackConfig GetGridConfig() { return m_gridConfig; }
        public TestSuiteData GetTestSuite() { return m_testSuite; }


        public void LoadData(LevelData data)
        {
            m_allowedPlaceables = data.GetPlaceables();
            m_gridConfig = data.GetGridConfig();
            m_testSuite = data.GetTestSuite();
        }
    }
}