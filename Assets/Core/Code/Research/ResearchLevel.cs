using BeauUtil;
using FieldDay.Assets;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Level")]
    public sealed class ResearchLevel : NamedAsset {
        public ResearchToolsMask AvailableTools;
        public ResearchMaterialKnowledge AvailableProperties = ResearchMaterialKnowledge.AllBasic;
        [AssetName(typeof(ResearchMaterial))] public StringHash32[] AvailableMaterials;
        public ResearchMaterialKnowledgePair[] PrePopulate;
        public ResearchMaterialKnowledgePair[] Objectives;
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
        [AssetName(typeof(ResearchMaterial))] public StringHash32 MaterialId;
        public ResearchMaterialKnowledge Knowledge;
    }
}