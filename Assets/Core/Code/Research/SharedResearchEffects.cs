using BeauPools;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Rendering;
using FieldDay.SharedState;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class SharedResearchEffects : SharedStateComponent {
        public ParticleSystem LightEmission;
        public ParticleSystem HighMobility;
    }

    static public partial class ResearchToolUtility {
        static public void SetLightEmissionStrength(Transform position, float strength) {
            SharedResearchEffects effects = Find.State<SharedResearchEffects>();
            strength = Math.Abs(strength);
            if (strength > 0) {
                effects.LightEmission.transform.SetPosition(position.position, Axis.XY);
                effects.LightEmission.SetEmissionMultiplier(30 * strength);
                effects.LightEmission.Play();
            } else {
                effects.LightEmission.Stop();
            }
        }

        static public void SetHighMobilityStrength(Transform position, float strength) {
            SharedResearchEffects effects = Find.State<SharedResearchEffects>();
            float absStrength = Math.Abs(strength);
            if (absStrength > 0) {
                effects.HighMobility.transform.SetRotation(strength < 0 ? 180 : 0, Axis.Z, Space.Self);
                effects.HighMobility.transform.SetPosition(position.position, Axis.XY);
                effects.HighMobility.SetEmissionMultiplier(30 * absStrength);
                effects.HighMobility.Play();
            } else {
                effects.HighMobility.Stop();
            }
        }
    }
}