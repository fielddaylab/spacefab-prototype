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
        private Routine m_ExplodeRoutine;

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

        private IEnumerator ExplodeRoutine() {
            Game.Input.PauseAll();
            yield return 0.5f;
            Sfx.Play("Research.Gem.Explode");
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[0], null);
            yield return 0.15f;
            Sfx.Play("Research.Gem.Explode");
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
            Game.Input.ResumeAll();
        }

        private void OnSlotFillUpdated() {
            if (m_Tool.AllSlotsFilled) {
                var recipeBook = Find.GlobalAsset<ResearchMaterialRecipeBook>();
                if (recipeBook.TryGetResult(ResearchToolUtility.GetInputMaterial(m_Tool, 0).AssetId, ResearchToolUtility.GetInputMaterial(m_Tool, 1).AssetId, out StringHash32 outputMaterial)) {
                    ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, Find.NamedAsset<ResearchMaterial>(outputMaterial));
                    m_OutputWasFilled = true;
                    Sfx.Play("Research.Gem.NewCombination");
                } else {
                    m_ExplodeRoutine.Replace(this, ExplodeRoutine());
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