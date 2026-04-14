using BeauRoutine;
using BeauUtil.Debugger;
using FieldDay.Systems;

namespace FieldDay.Scripting {
    internal static class ScriptRuntimeTickSystem {
        static public unsafe void RegisterModule() {
            Game.Systems.Register(&ProcessWork,
                new SysUpdate(GameLoopPhase.LateUpdate, 10000).AllowDuringCategories(ScriptUtility.RuntimeUpdateMask).AllowDuringLoad(),
                new SysPermissions().ReadWriteShared<ScriptRuntimeState>());
        }

        static public void ProcessWork(float deltaTime) {
            if (ScriptUtility.Runtime.PauseDepth != 0) {
                return;
            }

            if (ScriptUtility.Runtime.ActiveThreads.Count > 0) {
                //using (Profiling.Time("Leaf Update", ProfileTimeUnits.Microseconds)) {
                    Routine.ManualUpdate(deltaTime);
                //}
            }

            ScriptUtility.Runtime.SignalMap.Flush();
            // TODO: process queue?
        }
    }
}