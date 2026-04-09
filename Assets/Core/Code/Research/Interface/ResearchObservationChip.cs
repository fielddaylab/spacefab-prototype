using BeauPools;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace SpaceFab.Research {
    public sealed class ResearchObservationChip : GuiWidget {
        public Image Icon;
        public TMP_Text Label;
        public Image Background;
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

            chip.Background.sprite = style.Background;
            chip.Icon.sprite = style.Icon;
        }

        static public void PopulateObservationChip(ResearchObservationChip chip, ResearchChipId chipId, StringHash32 materialContext) {
            ResearchChipMetadata meta = ResearchChipUtility.Metadata(chipId);
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
        }
    }
}