using BeauUtil.Tags;
using Leaf;
using Leaf.Runtime;
using System.Collections;

namespace FieldDay.Scripting {
    public interface IDialogueChoicePresenter : IScriptThreadOwned {
        IEnumerator ShowOptions(LeafChoice choice, LeafNode node, ScriptThread thread, DialogueCharacterState character);
    }
}