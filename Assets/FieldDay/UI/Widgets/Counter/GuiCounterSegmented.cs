using BeauUtil.Debugger;
using UnityEngine;

namespace FieldDay.UI.Widgets {
    public sealed class GuiCounterSegmented : GuiCounter.Style {
        public GameObject[] EnabledObjects;
        public GameObject[] DisabledObjects;

        public override void Populate(in int data, GuiWidgetUpdateFlags flags) {
            Assert.True(data >= 0 && data <= EnabledObjects.Length && data <= DisabledObjects.Length);
            for(int i = 0; i < EnabledObjects.Length; i++) {
                EnabledObjects[i].SetActive(data > i);
            }
            for(int i = 0; i < DisabledObjects.Length; i++) {
                DisabledObjects[i].SetActive(data <= i);
            }
        }
    }
}