using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Assets;
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
        [NonSerialized] public ResearchChipId CurrentHypothesis;
        [NonSerialized] public StringHash32 ContextId;

        public CastableEvent<ResearchChipId> OnHypothesisUpdated = new CastableEvent<ResearchChipId>();

        void ISceneLateInitialize.LateInitialize() {
            if (PropertyIndex < 0) {
                PropertyIndex = 0;
            }

            NextButton.onClick.AddListener(() => {
                PropertyIndex = (PropertyIndex + 1) % AvailableProperties.Length;
                CurrentHypothesis = AvailableProperties[PropertyIndex];
                ResearchMaterialUtility.PopulateHypothesisChip(Chip, CurrentHypothesis, ContextId);
                OnHypothesisUpdated.Invoke(CurrentHypothesis);
            });
            PrevButton.onClick.AddListener(() => {
                PropertyIndex = (PropertyIndex + AvailableProperties.Length - 1) % AvailableProperties.Length;
                CurrentHypothesis = AvailableProperties[PropertyIndex];
                ResearchMaterialUtility.PopulateHypothesisChip(Chip, CurrentHypothesis, ContextId);
                OnHypothesisUpdated.Invoke(CurrentHypothesis);
            });

            Find.State<ResearchSelectionState>().OnUpdatedContext.Register((m) => {
                ContextId = m ? m.AssetId : null;
                ResearchMaterialUtility.UpdateHypothesisContext(Chip, ContextId);
            });

            CurrentHypothesis = AvailableProperties[PropertyIndex];
            ResearchMaterialUtility.PopulateHypothesisChip(Chip, CurrentHypothesis, null);
        }
    }

    static public partial class ResearchMaterialUtility {
        
    }
}