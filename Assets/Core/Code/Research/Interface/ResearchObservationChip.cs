using BeauUtil;
using FieldDay.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    public sealed class ResearchObservationChip : GuiWidget {
        public Image Icon;
        public TMP_Text Label;
        public Image Background;
    }

    static public partial class ResearchMaterialUtility {
        static private readonly StringHash32 Class_Slot = "Slot";

        static public void ApplyStyle(ResearchObservationChip chip, ResearchChipStyle style) {
            bool applyColors = chip.Class != Class_Slot;
            if (applyColors) {
                chip.Label.color = style.Colors.Content;
                chip.Icon.color = style.Colors.Content;
                chip.Background.color = style.Colors.Background;
            }

            chip.Background.sprite = style.Background;
            chip.Icon.sprite = style.Icon;
        }
    }
}