using BeauUtil.Debugger;
using FieldDay.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.SupplyChain {
    public sealed class ColorBarMeterStyle : GuiMeter.Style {
        public Graphic[] Targets;
        public Color32 OnColor;
        public Color32 OffColor;

        public override void Populate(in int data) {
            Assert.True(data >= 0 && data <= Targets.Length);
            for(int i = 0; i < Targets.Length; i++) {
                Targets[i].color = data > i ? OnColor : OffColor;
            }
        }
    }
}