using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;

namespace SpaceFab.Research {
    public sealed class ExplosionSystem : SharedStateSystemBehaviour<ExplosionState> {
        public override void ProcessWork(float deltaTime) {
            if (m_State.AreAnyExploding) {
                bool anyExploding = false;
                foreach(var c in Find.Components<ResearchMaterialItem>()) {
                    if (c.ExplosionRoutine) {
                        anyExploding = true;
                        break;
                    }
                }

                if (!anyExploding) {
                    m_State.StateTimer -= deltaTime;
                    if (m_State.StateTimer <= 0) {
                        m_State.AreAnyExploding = false;
                        Game.Input.ResumeAll();
                    }
                } else {
                    m_State.StateTimer = m_State.PostExplosionCooldown;
                }
            }
        }
    }
}