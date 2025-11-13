using BeauUtil.Tags;
using Leaf;
using Leaf.Runtime;
using System.Collections;

namespace FieldDay.Scripting {
    public interface IDialogueChooser : IScriptThreadOwned {
        IEnumerator ShowOptions(LeafChoice choice, LeafNode node, ScriptThread thread);
    }
}