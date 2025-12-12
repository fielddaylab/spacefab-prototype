using BeauRoutine.Extensions;
using BeauUtil;
using FieldDay.SharedState;
using System;

namespace SpaceFab.Research {
    public sealed class ExplosionState : SharedStateComponent {
        public float PreExplosionCooldown = 1f;
        public float PostExplosionCooldown = 0.5f;
        public KinematicAnimation TooBigAnimation;

        [NonSerialized] public bool AreAnyExploding = false;
        [NonSerialized] public float StateTimer;
    }
}