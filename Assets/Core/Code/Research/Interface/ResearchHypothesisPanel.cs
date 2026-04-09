using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Collections;
using FieldDay.Scenes;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    public sealed class ResearchHypothesisPanel : BaseGuiModule, ISceneLateInitialize {
        public ResearchHypothesisChip Chip;
        public PointerListener PrevButton;
        public PointerListener NextButton;

        [NonSerialized] public ResearchChipId[] AvailableProperties;
        [NonSerialized] public int PropertyIndex;
        [NonSerialized] public StringHash32 ContextId;

        void ISceneLateInitialize.LateInitialize() {
            if (PropertyIndex < 0) {
                PropertyIndex = 0;
            }

            NextButton.onClick.AddListener(() => {
                PropertyIndex = (PropertyIndex + 1) % AvailableProperties.Length;
                ResearchMaterialUtility.PopulateHypothesisChip(Chip, AvailableProperties[PropertyIndex], ContextId);
            });
            PrevButton.onClick.AddListener(() => {
                PropertyIndex = (PropertyIndex + AvailableProperties.Length - 1) % AvailableProperties.Length;
                ResearchMaterialUtility.PopulateHypothesisChip(Chip, AvailableProperties[PropertyIndex], ContextId);
            });

            ResearchMaterialUtility.PopulateHypothesisChip(Chip, AvailableProperties[PropertyIndex], null);
        }
    }

    static public partial class ResearchMaterialUtility {
        
    }
}