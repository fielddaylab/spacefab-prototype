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
    }

    public enum ResistState
    {
        Empty,
        Full,
        Developed,
        Stripped
    }

    [Serializable]
    public struct ResistLayer
    {
        public OrientedMask Mask;
        public ResistState State;
    }

    [Serializable]
    public struct PrecisionData
    {
        public List<float> Values;

        public float Avg()
        {
            float total = 0;
            foreach (var v in Values) {
                total += v;
            }

            return total / Values.Count;
        }
    }

    [Serializable]
    public struct WaferData
    {
        public ResistLayer ResistLayer;
        public MetallizationLayer MetallizationLayer;
        public OxideLayer OxideLayer;
        public SemiconductorLayer SemiconductorLayer;
        public PrecisionData Precision;

        public static bool IsEqual(WaferData dataA, WaferData dataB)
        {
            // TODO: make more dynamic
            bool isEqual = false;
            if (dataA.ResistLayer.State != dataB.ResistLayer.State)
            {
                isEqual = false;
            }

            if ((dataA.MetallizationLayer.State != dataB.MetallizationLayer.State)
                || (dataA.MetallizationLayer.Mask.Id != dataB.MetallizationLayer.Mask.Id)
                || (dataA.MetallizationLayer.Mask.Rotation != dataB.MetallizationLayer.Mask.Rotation)
                )
            {
                isEqual = false;
            }

            if (dataA.OxideLayer.State != dataB.OxideLayer.State)
            {
                isEqual = false;
            }

            bool hasPatterns = true;

            foreach (var pattern in dataA.SemiconductorLayer.DopingPatterns)
            {
                bool anyFound = false;
                foreach (var currPattern in dataB.SemiconductorLayer.DopingPatterns)
                {
                    if ((currPattern.Mask.Id == pattern.Mask.Id)
                        && (currPattern.Mask.Rotation == pattern.Mask.Rotation)
                        && (currPattern.DopingType == pattern.DopingType)
                        )
                    {
                        anyFound = true;
                    }
                }

                if (!anyFound)
                {
                    hasPatterns = false;
                    break;
                }
            }

            if (!hasPatterns)
            {
                isEqual = false;
            }

            return isEqual;
        }
    }

    #endregion // Structs & Enums

    public class WaferState : MonoBehaviour
    {
        public WaferData Data;
        public SpriteRenderer LatestMaskRenderer;
        public int NumLayers = 0;

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

            Data.Precision = new PrecisionData();
            Data.Precision.Values = new List<float>();
        }

        public void SetOxideStateFurnace(float precision, bool usedDopant, DopingType dopingType)
        {
            Data.Precision.Values.Add(precision);

            if (usedDopant)
            {
                var pattern = new DopingPattern();
                pattern.Mask.Id = Data.OxideLayer.Mask.Id;
                pattern.Mask.Rotation = Data.OxideLayer.Mask.Rotation;
                pattern.DopingType = dopingType;
                if (Data.OxideLayer.Mask.Id != MaskId.NONE)
                {
                    Data.SemiconductorLayer.DopingPatterns.Add(pattern);
                }

                if (LatestMaskRenderer)
                {
                    if (dopingType == DopingType.N)
                    {
                        LatestMaskRenderer.color = GameDB.Instance.NDopantColor;
                    }
                    else if (dopingType == DopingType.P)
                    {
                        LatestMaskRenderer.color = GameDB.Instance.PDopantColor;
                    }
                }

                LatestMaskRenderer = null;

                Data.OxideLayer.State = OxideState.Empty;

                Data.OxideLayer.Mask.Id = MaskId.NONE;
                Data.OxideLayer.Mask.Rotation = 0;
            }
            else
            {
                Data.OxideLayer.State = OxideState.Full;
            }
        }
        
        public void SetPhotoState(MaskId mask, int rotation)
        {
            Data.ResistLayer.State = ResistState.Developed;

            var oriented = new OrientedMask();
            oriented.Id = mask;

            if (rotation < 0) { rotation += 360; }
            oriented.Rotation = rotation;

            Data.ResistLayer.Mask = oriented;
        }

        public void SetResistState(float precision)
        {
            Data.ResistLayer.State = ResistState.Full;
            Data.Precision.Values.Add(precision);
        }

        public void SetResistStateWash()
        {
            Data.ResistLayer.State = ResistState.Empty;
            Data.ResistLayer.Mask.Id = MaskId.NONE;
            Data.ResistLayer.Mask.Rotation = 0;
        }

        public void SetOxideStateEtch(float precision)
        {
            Data.OxideLayer.Mask.Id = Data.ResistLayer.Mask.Id;
            Data.OxideLayer.Mask.Rotation = Data.ResistLayer.Mask.Rotation;
            Data.OxideLayer.State = OxideState.Stripped;

            Data.ResistLayer.State = ResistState.Stripped;
            Data.Precision.Values.Add(precision);
        }

        public void SetMetallizationState(float precision)
        {
            Data.MetallizationLayer.State = MetallizationState.Full;
            Data.Precision.Values.Add(precision);
        }

        public void SetMetallizationStateEtch(float precision)
        {
            Data.MetallizationLayer.State = MetallizationState.Stripped;
            // Data.MetallizationLayer.Precision = precision;

            LatestMaskRenderer.color = Color.yellow;
            LatestMaskRenderer = null;

            Data.MetallizationLayer.Mask.Id = Data.ResistLayer.Mask.Id;
            Data.MetallizationLayer.Mask.Rotation = Data.ResistLayer.Mask.Rotation;

            Data.ResistLayer.State = ResistState.Stripped;
            Data.Precision.Values.Add(precision);
        }
    }
}