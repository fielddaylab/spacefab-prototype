using BeauUtil;
using FieldDay.Assets;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Level")]
    public sealed class ResearchLevel : NamedAsset {
        public string Label;
        public ResearchToolsMask AvailableTools;
        public ResearchChipId[] AvailableProperties;
        public ResearchChipId StartingHypothesis;
        [AssetName(typeof(ResearchMaterial))] public StringHash32[] AvailableMaterials;
        public ResearchMaterialKnowledgePair[] PrePopulate;
        public ResearchMaterialKnowledgePair[] Objectives;

        [AssetName(typeof(ResearchLevel))] public StringHash32 NextLevelId;
    }

    [Flags]
    public enum ResearchToolsMask : uint {
        AdjustableBattery = 0x01,
        Thermal = 0x02,
        Doping = 0x04,
        Junction = 0x08
    }

    [Serializable]
    public struct ResearchMaterialKnowledgePair {
        public ResearchChipId Chip;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 MaterialId;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 ContextId;
    }
}