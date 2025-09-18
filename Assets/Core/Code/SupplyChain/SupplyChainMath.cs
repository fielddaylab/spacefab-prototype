using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [CreateAssetMenu(menuName = "SupplyChain/Math Settings")]
    public sealed class SupplyChainMath : GlobalAsset {
        public float[] Reliabilities = new float[4];
        public float[] Speeds = new float[4];
        public float[] Costs = new float[4];

        [Header("Hazards")]        
        public float TimeDialationSpeedFactor = 0.65f;
        public float RiskyMultiplierPerUnit = 0.85f;
        public float RiskyMultiplierUnitDist = 3;
    }
}