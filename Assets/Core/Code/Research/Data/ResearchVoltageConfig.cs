using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/SpecialVoltage Configurations")]
    public sealed class ResearchVoltageConfig : GlobalAsset {
        public Sprite[] VoltageIcons;
        public float[] Voltages;
        public int CenterIndex;
        public int DefaultIndex;
    }
}