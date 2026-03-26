using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;

namespace SpaceFab.Research {
    public sealed class ExplosionSystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork, SysUpdate.Default(),
                new SysPermissions().ReadWriteShared<ExplosionState>().Read<ResearchMaterialItem>());
        }

        static private void ProcessWork(float deltaTime) {
            Find.State(out ExplosionState state);

            if (state.AreAnyExploding) {
                bool anyExploding = false;
                foreach(var c in Find.Components<ResearchMaterialItem>()) {
                    if (c.ExplosionRoutine) {
                        anyExploding = true;
                        break;
                    }
                }

                if (!anyExploding) {
                    state.StateTimer -= deltaTime;
                    if (state.StateTimer <= 0) {
                        state.AreAnyExploding = false;
                        Game.Input.ResumeAll();
                    }
                } else {
                    state.StateTimer = state.PostExplosionCooldown;
                }
            }
        }
    }
}