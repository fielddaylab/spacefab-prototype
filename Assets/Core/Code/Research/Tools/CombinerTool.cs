using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class CombinerTool : MonoBehaviour {
        public SpriteRenderer[] AtomSlots;
        public float AtomSizeScale = 100;

        [Header("Background")]
        public SpriteRenderer Background;
        public Color32 BackgroundDisabledColor = Color.white;
        public GameObject SemiconductorWarning;

        [Header("Valence")]
        public GameObject ValenceGroup;
        public SpriteRenderer[] ValencePips;
        public TMP_Text ValenceMinusOne;
        public TMP_Text ValencePlusOne;
        public Color32 BlankValencePipColor;
        public Color32 IncorrectValencePipColor;
        public Color32 CorrectNTypeValencePipColor;
        public Color32 CorrectPTypeValencePipColor;
        public Color32 ExcessValencePipColor;

        [NonSerialized] private ResearchTool m_Tool;
        [NonSerialized] private int m_DopingIndex;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.Slots[0].OnSlotUpdated.Register(OnFirstSlotUpdated);
            m_Tool.Slots[1].OnSlotUpdated.Register(OnSecondSlotUpdated);
        }

        private void OnFirstSlotUpdated(ResearchSlot _, ResearchMaterialItem item) {
            bool hasMaterial = item != null;
            bool validMaterial = hasMaterial;
            if (validMaterial) {
                ResearchMaterial material = item.Material;
                validMaterial = (material.Electrical == ElectricalTag.Semiconductor && (ResearchMaterialUtility.GetKnownCategories(material.AssetId) & ResearchMaterialKnowledge.Electrical) != 0);
            }

            if (!validMaterial) {
                ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
                m_Tool.Slots[1].Locked = true;
                GuiCommands.SetActive(m_Tool.Slots[1].gameObject, false);
                GuiCommands.SetActive(SemiconductorWarning, hasMaterial);
                ClearAtomicView();
                Background.color = BackgroundDisabledColor;
                m_DopingIndex = 0;
                return;
            }

            m_Tool.Slots[1].Locked = false;
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
            GuiCommands.SetActive(m_Tool.Slots[1].gameObject, true);
            GuiCommands.SetActive(SemiconductorWarning, false);
            Background.color = Color.white;
            m_DopingIndex = 0;
            DisplayAtomicView(item.Material.Atoms, m_DopingIndex);
            UpdateValencePips(item.Material.Atoms[m_DopingIndex].ValenceElectrons, 0);
        }

        private void OnSecondSlotUpdated(ResearchSlot _, ResearchMaterialItem item) {
            if (!m_Tool.Slots[0].Item) {
                return;
            }

            if (item == null) {
                UpdateValencePips(m_Tool.Slots[0].Item.Material.Atoms[m_DopingIndex].ValenceElectrons, 0);
                return;
            }

            ResearchMaterial targetMaterial = m_Tool.Slots[0].Item.Material;
            AtomicStructure targetAtom = targetMaterial.Atoms[m_DopingIndex];
            ResearchMaterial dopant = item.Material;
            StringHash32 dopantId = dopant.AssetId;
            AtomicStructure dopantAtom = dopant.Atoms[0];

            if (targetMaterial.DopantN == dopantId) {
                // set correct
            } else if (targetMaterial.DopantP == dopantId) {
                // set correct
            } else if (dopant.Atoms.Length > 1 || dopantAtom.Size > targetAtom.Size) {
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.TooBig);
            } else if (dopantAtom.ValenceElectrons < targetAtom.ValenceElectrons - 1 || dopantAtom.ValenceElectrons > targetAtom.ValenceElectrons + 1) {
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.InvalidCombo);
            } else {
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.InvalidCombo);
            }

            UpdateValencePips(targetAtom.ValenceElectrons, dopantAtom.ValenceElectrons);
        }

        private void ClearAtomicView() {
            foreach(var atomSlot in AtomSlots) {
                atomSlot.enabled = false;
            }

            ValenceGroup.SetActive(false);
        }

        private void DisplayAtomicView(AtomicStructure[] atoms, int offset) {
            ResearchSprites sprites = Find.GlobalAsset<ResearchSprites>();

            for (int i = 0; i < AtomSlots.Length; i++) {
                AtomicStructure atomData = atoms[(offset + i) % atoms.Length];
                SpriteRenderer renderer = AtomSlots[i];
                renderer.sprite = sprites.AtomIcons[(int) atomData.Appearance];
                renderer.transform.SetScale(atomData.Size / AtomSizeScale);
                renderer.color = atomData.Color;
                renderer.enabled = true;
            }

            ValenceGroup.SetActive(true);
            int targetValence = atoms[offset].ValenceElectrons;
            ValenceMinusOne.transform.localPosition = ValencePips[targetValence - 1 - 1].transform.localPosition;
            ValencePlusOne.transform.localPosition = ValencePips[targetValence - 1 + 1].transform.localPosition;
            ValenceMinusOne.SetText((targetValence - 1).ToStringLookup());
            ValencePlusOne.SetText((targetValence + 1).ToStringLookup());
        }

        private void UpdateValencePips(int targetValence, int currentValence) {
            Color32 currentColor = IncorrectValencePipColor;
            Color32 defaultColor = BlankValencePipColor;
            Color32 excessColor = ExcessValencePipColor;
            if (currentValence == targetValence - 1) {
                currentColor = CorrectNTypeValencePipColor;
            } else if (currentValence == targetValence + 1) {
                currentColor = CorrectPTypeValencePipColor;
            }

            for(int i = 0; i < ValencePips.Length; i++) {
                ValencePips[i].color = (i < currentValence) ? currentColor : ((i < (targetValence + 1)) ? defaultColor : excessColor);
            }
        }
    }
}