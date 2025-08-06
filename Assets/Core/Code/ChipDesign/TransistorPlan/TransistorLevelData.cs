using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{

    public enum Placeable {
        VMINUS,
        VPLUS,
        IN,
        A,
        B,
        NNODE,
        PNODE,
        OUT
    }

    [CreateAssetMenu(menuName = "ChipDesign/Create New Transistor Level Data")]
    public class TransistorLevelData : ScriptableObject
    {
        [SerializeField] private List<Placeable> m_allowedPlacables;

        [SerializeField] private float InVal;
        [SerializeField] private float AVal;
        [SerializeField] private float BVal;
        [SerializeField] private float OutVal;

        public List<Placeable> GetPlaceables() { return m_allowedPlacables; }

        public float GetInVal() { return InVal; }
        public float GetAVal() { return AVal; }
        public float GetBVal() { return BVal; }
        public float GetOutVal() { return OutVal; }
    }

    public class TransistorLevelDataCopy
    {
        [SerializeField] private List<Placeable> m_allowedPlacables;

        public float InVal;
        public float AVal;
        public float BVal;
        public float OutVal;

        public List<Placeable> GetPlaceables() { return m_allowedPlacables; }

        public float GetInVal() { return InVal; }
        public float GetAVal() { return AVal; }
        public float GetBVal() { return BVal; }
        public float GetOutVal() { return OutVal; }

        public void LoadData(TransistorLevelData data)
        {
            InVal = data.GetInVal();
            AVal = data.GetAVal();
            BVal = data.GetBVal();
            OutVal = data.GetOutVal();

            m_allowedPlacables = data.GetPlaceables();
        }
    }
}