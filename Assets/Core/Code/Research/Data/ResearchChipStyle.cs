using FieldDay;
using FieldDay.Assets;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Chip Style")]
    public sealed class ResearchChipStyle : NamedAsset {
        public Sprite Icon;
        public Sprite Background;
        public ColorPalette2 Colors;
        public bool IsTall;
    }
}