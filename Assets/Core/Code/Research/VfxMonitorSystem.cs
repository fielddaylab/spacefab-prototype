using BeauPools;
using BeauRoutine;
using FieldDay.Components;
using FieldDay.Systems;
using FieldDay.UI;
using UnityEngine;

namespace SpaceFab.Research {
    [SysUpdate(FieldDay.GameLoopPhase.UnscaledLateUpdate, 1000)]
    public sealed class VfxMonitorSystem : ComponentSystemBehaviour<VfxInstance> {
        public override void ProcessWork(float deltaTime) {
            foreach(var instance in m_Components) {
                if (!VfxUtility.IsPlaying(instance)) {
                    GuiCommands.TryFreePrefab(instance);
                }
            }
        }
    }
}