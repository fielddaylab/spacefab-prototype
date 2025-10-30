using System.Runtime.CompilerServices;
using UnityEngine;

namespace FieldDay.Rendering {
    static public class ParticleUtility {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float GetEmissionMultiplier(this ParticleSystem system) {
            return system.emission.rateOverTimeMultiplier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SetEmissionMultiplier(this ParticleSystem system, float multiplier) {
            var emission = system.emission;
            emission.rateOverTimeMultiplier = multiplier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool GetEmissionEnabled(this ParticleSystem system) {
            return system.emission.enabled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SetEmissionEnabled(this ParticleSystem system, bool enabled) {
            var emission = system.emission;
            emission.enabled = enabled;
        }
    }
}