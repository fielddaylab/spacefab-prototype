using System;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using TMPro;
using UnityEngine.UI;

namespace SpaceFab.Research {
    public sealed class ResearchObservationChip : GuiWidget {
        public Image Icon;
        public TMP_Text Label;
        public Image Background;
        public CursorHint Cursor;
        public Image InputBlocker;

        [NonSerialized] public ResearchChipId Chip;
    }

    static public partial class ResearchMaterialUtility {
        static private readonly StringHash32 Class_Slot = "Slot";
        static private readonly StringHash32 Class_Hypothesis = "Hypothesis";

        static public void ApplyStyle(ResearchObservationChip chip, ResearchChipStyle style) {
            bool applyColors = chip.Class != Class_Slot;
            if (applyColors) {
                chip.Label.color = style.Colors.Content;
                chip.Icon.color = style.Colors.Content;
                chip.Background.color = style.Colors.Background;
            }

            bool applySize = chip.Class != Class_Hypothesis;
            if (applySize) {
                if (style.IsTall) {
                    chip.Rect.SetSizeDelta(48, Axis.Y);
                } else {
                    chip.Rect.SetSizeDelta(32, Axis.Y);
                }
            }

            chip.Label.fontStyle = style.TextStyle;
            chip.Background.sprite = style.Background;
            chip.Icon.sprite = style.Icon;

            if (chip.InputBlocker) {
                chip.InputBlocker.sprite = style.Background;
            }
        }

        static public void PopulateObservationChip(ResearchObservationChip chip, ResearchChipId chipId, StringHash32 materialContext) {
            ResearchChipMetadata meta = ResearchChipUtility.Metadata(chipId);
            chip.Chip = chipId;
            ApplyStyle(chip, Find.NamedAsset<ResearchChipStyle>(ResearchChipUtility.CategoryStyleId(meta.Category)));
            ApplyContextualTextToObservationChip(chip, chipId, materialContext);
        }

        static public void ApplyContextualTextToObservationChip(ResearchObservationChip chip, ResearchChipId chipId, StringHash32 materialContext) {
            ResearchChipMetadata meta = ResearchChipUtility.Metadata(chipId);
            if (ResearchChipUtility.CategoryRequiresContext(meta.Category)) {
                using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    string name;
                    if (materialContext.IsEmpty) {
                        name = "???";
                    } else {
                        ResearchMaterial material = Find.NamedAsset<ResearchMaterial>(materialContext);
                        name = IsNameKnown(materialContext) ? material.DisplayName : material.UnknownDisplayName;
                    }
                    psb.Builder.AppendFormat(meta.Label, name);
                    chip.Label.SetText(psb);
                }
            } else {
                chip.Label.SetText(meta.Label);
            }

            chip.Cursor.Tooltip = meta.Tooltip;
        }
    }
}