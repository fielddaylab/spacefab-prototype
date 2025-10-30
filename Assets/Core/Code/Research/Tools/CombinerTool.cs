using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using System;
using System.Collections;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class CombinerTool : MonoBehaviour {
        [NonSerialized] private ResearchTool m_Tool;

        private bool m_OutputWasFilled;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);
            m_Tool.OutputSlot.OnSlotUpdated.Register(OnOutputSlotUpdated);
        }

        private void OnOutputSlotUpdated() {
            if (m_Tool.OutputSlot.Item == null && m_OutputWasFilled) {
                m_OutputWasFilled = false;
                ResearchSlotUtility.FillInSlot(m_Tool.Slots[0], null);
                ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
            }
        }

        private void OnSlotFillUpdated() {
            if (m_Tool.AllSlotsFilled) {
                var recipeBook = Find.GlobalAsset<ResearchMaterialRecipeBook>();
                if (recipeBook.TryGetResult(ResearchToolUtility.GetInputMaterial(m_Tool, 0).AssetId, ResearchToolUtility.GetInputMaterial(m_Tool, 1).AssetId, out StringHash32 outputMaterial)) {
                    ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, Find.NamedAsset<ResearchMaterial>(outputMaterial));
                    m_OutputWasFilled = true;
                    Sfx.Play("Research.Gem.NewCombination");
                } else {
                    ResearchMaterialUtility.BeginExplosions();
                    ResearchMaterialUtility.ExplodeItem(ResearchToolUtility.GetInputMaterialItem(m_Tool, 0), ExplosionStyle.InvalidCombo, 0.5f);
                    ResearchMaterialUtility.ExplodeItem(ResearchToolUtility.GetInputMaterialItem(m_Tool, 1), ExplosionStyle.InvalidCombo, 0.9f);
                    m_OutputWasFilled = false;
                    ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, null);
                }
            } else {
                m_OutputWasFilled = false;
                ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, null);
            }
        }
    }
}