using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    #region Structs & Enums

    public enum MaskId
    {
        NONE,
        A,
        B,
        C
    }

    public enum DopingType
    {
        N,
        P
    }

    [Serializable]
    public struct OrientedMask
    {
        public MaskId Id;
        public int Rotation;
    }

    [Serializable]
    public struct DopingPattern
    {
        public OrientedMask Mask;
        public DopingType DopingType;
    }

    public enum SemiconductorState
    {
        Blank,
        DopedN,
        DopedP,
        DopedNP
    }

    [Serializable]
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

    [Serializable]
    public struct OxideLayer
    {
        public OrientedMask Mask;
        public OxideState State;
        public float Precision;
    }

    public enum MetallizationState
    {
        Empty,
        Full,
        Stripped,
        OxideFilled
    }

    [Serializable]
    public struct MetallizationLayer
    {
        public OrientedMask Mask;
        public MetallizationState State;
        public float Precision;
    }

    public enum ResistState
    {
        Empty,
        Full,
        Developed
    }

    [Serializable]
    public struct ResistLayer
    {
        public OrientedMask Mask;
        public ResistState State;
        public float Precision;
    }

    [Serializable]
    public struct WaferData
    {
        public ResistLayer ResistLayer;
        public MetallizationLayer MetallizationLayer;
        public OxideLayer OxideLayer;
        public SemiconductorLayer SemiconductorLayer;
    }

    #endregion // Structs & Enums

    public class WaferState : MonoBehaviour
    {
        public WaferData Data;

        private void Awake()
        {
            Init();

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }

        private void Init()
        {
            Data = new WaferData();

            Data.SemiconductorLayer = new SemiconductorLayer();
            Data.SemiconductorLayer.DopingPatterns = new List<DopingPattern>();

            Data.OxideLayer = new OxideLayer();
            Data.OxideLayer.State = OxideState.Empty;

            Data.MetallizationLayer = new MetallizationLayer();

            Data.ResistLayer = new ResistLayer();
            Data.ResistLayer.State = ResistState.Empty;
        }

        public void SetOxideState(float precision, bool usedDopant, DopingType dopingType)
        {
            Data.OxideLayer.Precision = precision;

            if (usedDopant)
            {
                // TODO: double check. Adds MaskID of Oxidation State to Doping Layer, using the DopingType of the Dopant used.
                var pattern = new DopingPattern();
                pattern.Mask.Id = Data.OxideLayer.Mask.Id;
                pattern.DopingType = dopingType;
                Data.SemiconductorLayer.DopingPatterns.Add(pattern);
            }

            Data.OxideLayer.State = OxideState.Full;
        }
        
        public void SetPhotoState(MaskId mask, int rotation)
        {
            Data.ResistLayer.State = ResistState.Developed;

            var oriented = new OrientedMask();
            oriented.Id = mask;
            oriented.Rotation = rotation;

            Data.ResistLayer.Mask = oriented;
        }

        public void SetResistState(float precision)
        {
            Data.ResistLayer.State = ResistState.Full;
            Data.ResistLayer.Precision = precision;
        }

        public void SetMetallizationState(float precision)
        {
            Data.MetallizationLayer.State = MetallizationState.Full;
            Data.MetallizationLayer.Precision = precision;
        }
    }
}