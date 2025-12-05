using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.UI;
using System;
using System.Collections;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class CombinerTool : MonoBehaviour {
        public SpriteRenderer[] AtomSlots;
        public float AtomSizeScale = 100;
        [NonSerialized] private ResearchTool m_Tool;
        public SpriteRenderer Background;
        public Color32 BackgroundDisabledColor = Color.white;
        public GameObject SemiconductorWarning;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.Slots[0].OnSlotUpdated.Register(OnFirstSlotUpdated);
            m_Tool.Slots[1].OnSlotUpdated.Register(OnSecondSlotUpdated);
        }

        private void OnFirstSlotUpdated(ResearchSlot _, ResearchMaterialItem item) {

        }

        private void OnSecondSlotUpdated(ResearchSlot _, ResearchMaterialItem item) {

        }

        private void OnSlotFillUpdated() {
            bool hasFirst = m_Tool.Slots[0].Item != null;
            bool hasSecond = m_Tool.Slots[1].Item != null;

            bool hasValidFirst = false;
            ResearchMaterial firstMaterial = ResearchToolUtility.GetInputMaterial(m_Tool, 0);
            if (hasFirst) {
                StringHash32 materialId = firstMaterial.AssetId;
                ResearchMaterialKnowledge knowledge = ResearchMaterialUtility.GetKnownCategories(materialId);
                hasValidFirst = (knowledge & ResearchMaterialKnowledge.Electrical) != 0 && firstMaterial.Electrical == ElectricalTag.Semiconductor;
            }

            if (!hasValidFirst) {
                m_Tool.Slots[1].Locked = true;
                ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
                GuiCommands.SetActive(m_Tool.Slots[0].gameObject, false);
                GuiCommands.SetActive(SemiconductorWarning, hasFirst);
                ClearAtomicView();
                Background.color = BackgroundDisabledColor;
            } else {
                m_Tool.Slots[1].Locked = false;
                GuiCommands.SetActive(SemiconductorWarning, false);
                Background.color = Color.white;
            }

            //if (m_Tool.AllSlotsFilled) {
            //    var recipeBook = Find.GlobalAsset<ResearchMaterialRecipeBook>();
            //    if (recipeBook.TryGetResult(ResearchToolUtility.GetInputMaterial(m_Tool, 0).AssetId, ResearchToolUtility.GetInputMaterial(m_Tool, 1).AssetId, out StringHash32 outputMaterial)) {
            //        ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, Find.NamedAsset<ResearchMaterial>(outputMaterial));
            //        m_OutputWasFilled = true;
            //        VfxUtility.PlayFromPool(Find.State<ResearchPools>().ShineEffectPool, m_Tool.OutputSlot.transform);
            //        Sfx.Play("Research.Gem.NewCombination");
            //    } else {
            //        ResearchMaterialUtility.BeginExplosions();
            //        ResearchMaterialUtility.ExplodeItem(ResearchToolUtility.GetInputMaterialItem(m_Tool, 0), ExplosionStyle.InvalidCombo, 0.5f);
            //        ResearchMaterialUtility.ExplodeItem(ResearchToolUtility.GetInputMaterialItem(m_Tool, 1), ExplosionStyle.InvalidCombo, 0.9f);
            //        m_OutputWasFilled = false;
            //        ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, null);
            //    }
            //} else {
            //    m_OutputWasFilled = false;
            //    ResearchSlotUtility.FillInSlot(m_Tool.OutputSlot, null);
            //}
        }

        private void ClearAtomicView() {
            foreach(var atomSlot in AtomSlots) {
                atomSlot.enabled = false;
            }
        }
    }
}