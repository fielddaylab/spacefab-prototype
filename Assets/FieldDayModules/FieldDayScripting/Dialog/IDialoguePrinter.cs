using BeauUtil.Tags;
using Leaf.Runtime;
using System.Collections;

namespace FieldDay.Scripting {
    public interface IDialoguePrinter : IScriptThreadOwned {
        TagStringEventHandler PrepareLine(TagString text, TagStringEventHandler parentHandler);
        IEnumerator TypeLine(TagTextData textData);
        IEnumerator CompleteLine();
        void StartSkip();
        void CancelSkip();
    }
}