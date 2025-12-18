using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [CreateAssetMenu(menuName = "SupplyChain/Math Settings")]
    public sealed class SupplyChainMath : GlobalAsset {
        public float[] Reliabilities = new float[4];
        public float[] Speeds = new float[4];

        [Header("Hazards")]        
        public float TimeDialationSpeedFactor = 0.65f;
        public float RiskyMultiplierPerUnit = 0.85f;
        public float RiskyMultiplierUnitDist = 3;

        public int GetReliabilityIndex(float percent) {
            for(int i = Reliabilities.Length; i-- > 0;) {
                if (Reliabilities[i] <= percent) {
                    return i;
                }
            }
            return 0;
        }
    }
}