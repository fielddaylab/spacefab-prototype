using BeauUtil.Debugger;
using BeauUtil.Tags;
using FieldDay.Assets;
using Leaf.Runtime;

namespace FieldDay.Scripting {
    public interface IScriptThreadOwned {
        LeafThreadHandle ThreadOwner { get; set; }
        void OnThreadRelease(LeafThreadHandle threadHandle, ScriptThreadOwnershipClearReason cancelType);
        void OnThreadAcquire(LeafThreadHandle threadHandle);
    }

    public enum ScriptThreadOwnershipClearReason : byte {
        Completed,
        Cancelled,
        Switch
    }

    static public partial class ScriptThreadUtility {
        static public bool TryClearThreadOwner(this IScriptThreadOwned resource, LeafThreadHandle handle, ScriptThreadOwnershipClearReason clearReason) {
            if (handle == default || resource.ThreadOwner != handle) {
                return false;
            }
            resource.OnThreadRelease(handle, clearReason);
            resource.ThreadOwner = default;
            return true;
        }

        static public void SwitchThreadOwner(this IScriptThreadOwned resource, LeafThreadHandle handle) {
            LeafThreadHandle currentHandle = resource.ThreadOwner;
            if (currentHandle == handle) {
                return;
            }

            if (currentHandle != default) {
                resource.OnThreadRelease(currentHandle, ScriptThreadOwnershipClearReason.Switch);
                resource.ThreadOwner = default;
            }

            resource.ThreadOwner = handle;
            if (handle != default) {
                resource.OnThreadAcquire(handle);
            }
        }
    }
}