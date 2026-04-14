using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FieldDay.UI.Widgets {
    public sealed class GuiCounterText : GuiCounter.Style {
        public TMP_Text Text;

        public override void Populate(in int data, GuiWidgetUpdateFlags flags) {
            using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                psb.Builder.AppendNoAlloc(data);
                Text.SetText(psb);
            }
        }
    }
}