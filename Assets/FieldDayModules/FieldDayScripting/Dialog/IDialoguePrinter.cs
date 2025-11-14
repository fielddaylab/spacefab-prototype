using BeauUtil.Tags;
using Leaf.Runtime;
using System.Collections;

namespace FieldDay.Scripting {
    public interface IDialoguePrinter : IScriptThreadOwned {
        TagStringEventHandler PrepareLine(TagString text, DialogueCharacterState character, TagStringEventHandler parentHandler);
        void UpdateCharacter(DialogueCharacterState character);
        IEnumerator TypeLine(TagString text, TagTextData textData);
        IEnumerator CompleteLine();
        void FastForwardLine(int visibleCount, int richCount);
        void StartSkip();
        void CancelSkip();
    }
}