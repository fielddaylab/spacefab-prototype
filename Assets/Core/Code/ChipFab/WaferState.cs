using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    #region Structs & Enums

    public enum MaskId
    {
        A,
        B,
        C
    }

    public enum DopingType
    {
        N,
        P
    }

    public struct OrientedMask
    {
        public MaskId Id;
        public int Rotation;
    }

    public struct DopingPattern
    {
        public OrientedMask Mask;
        public DopingType DopingType;
    }

    public struct SemiconductorLayer
    {
        public List<DopingPattern> DopingPatterns;
    }


    public enum OxideState
    {
        Empty,
        Full,
        Stripped
    }

    public struct OxideLayer
    {
        public OrientedMask Mask;
        public OxideState State;
    }

    public enum MetallizationState
    {
        Empty,
        Full,
        Stripped,
        OxideFilled
    }

    public struct MetallizationLayer
    {
        public OrientedMask Mask;
        public MetallizationState State;
    }

    public enum ResistState
    {
        Empty,
        Full,
        Developed
    }

    public struct ResistLayer
    {
        public OrientedMask Mask;
        public ResistState State;
    }

    #endregion // Structs & Enums

    public class WaferState : MonoBehaviour
    {
        public ResistLayer ResistLayer;
        public MetallizationLayer MetallizationLayer;
        public OxideLayer OxideLayer;
        public SemiconductorLayer SemiconductorLayer;

        private void Awake()
        {
            Init();

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }

        private void Init()
        {
            SemiconductorLayer = new SemiconductorLayer();
            SemiconductorLayer.DopingPatterns = new List<DopingPattern>();

            OxideLayer = new OxideLayer();

            MetallizationLayer = new MetallizationLayer();

            ResistLayer = new ResistLayer();
        }
    }
}