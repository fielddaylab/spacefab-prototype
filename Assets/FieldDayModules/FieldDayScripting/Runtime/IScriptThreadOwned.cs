using BeauUtil.Tags;
using Leaf.Runtime;

namespace FieldDay.Scripting {
    public interface IScriptThreadOwned {
        LeafThreadHandle GetThreadOwner();
        void SetThreadOwner(LeafThreadHandle handle);
        bool TryClearThreadOwner(LeafThreadHandle handle, ScriptThreadOwnershipClearReason cancelType);
        void ClearThreadOwner();
    }

    public enum ScriptThreadOwnershipClearReason : byte {
        Completed,
        Released,
        Cancelled,
    }
}