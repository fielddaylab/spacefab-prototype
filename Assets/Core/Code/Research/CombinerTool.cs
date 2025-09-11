using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Components;
using System;
using System.Collections;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class CombinerTool : MonoBehaviour {
        [NonSerialized] private ResearchTool m_Tool;

        private Routine m_ExplodeRoutine;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);
        }

        private IEnumerator ExplodeRoutine() {
            Game.Input.PauseAll();
            yield return 0.5f;
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[0], null);
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
            Game.Input.ResumeAll();
        }

        private void OnSlotFillUpdated() {
            if (m_Tool.AllSlotsFilled) {
                var recipeBook = Find.GlobalAsset<ResearchMaterialRecipeBook>();
                if (recipeBook.TryGetResult(ResearchToolUtility.GetInputMaterial(m_Tool, 0).AssetId, ResearchToolUtility.GetInputMaterial(m_Tool, 1).AssetId, out StringHash32 outputMaterial)) {
                    ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, Find.NamedAsset<ResearchMaterial>(outputMaterial));
                } else {
                    m_ExplodeRoutine.Replace(this, ExplodeRoutine());
                    ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, null);
                }
            } else {
                ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, null);
            }
        }
    }
}