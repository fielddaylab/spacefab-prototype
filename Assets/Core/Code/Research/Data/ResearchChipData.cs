using BeauUtil;
using FieldDay;
using FieldDay.Assets;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Chip Data")]
    public sealed class ResearchChipData : NamedAsset {
        [AssetName(typeof(ResearchChipData))] public StringHash32 AliasFor;
        public bool RequiresContext;

        [Header("Display")]
        [AssetName(typeof(ResearchChipStyle))] public StringHash32 Style;
        public string Label;

        [Header("Property-Specific")]
        [AssetName(typeof(ResearchChipData))] public StringHash32[] Dependencies;
    }
}