using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "SpaceFab/Research/Voltage Configurations")]
    public sealed class ResearchVoltageConfig : GlobalAsset {
        public Sprite[] VoltageIcons;
        public float[] Voltages;
        public int Center;
        public int Default;
    }
}