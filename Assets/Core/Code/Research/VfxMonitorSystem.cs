using BeauPools;
using BeauRoutine;
using FieldDay;
using FieldDay.Components;
using FieldDay.Systems;
using FieldDay.UI;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class VfxMonitorSystem : SystemModule {
        protected unsafe override void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork,
                new SysUpdate(GameLoopPhase.UnscaledLateUpdate, 1000),
                new SysPermissions().ReadWrite<VfxInstance>());
        }

        static private void ProcessWork(float deltaTime) {
            foreach(var instance in Find.Components<VfxInstance>()) {
                if (!VfxUtility.IsPlaying(instance)) {
                    GuiCommands.TryFreePrefab(instance);
                }
            }
        }
    }
}