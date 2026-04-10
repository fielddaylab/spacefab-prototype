using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
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
        [Serializable]
        public struct AtomSelector {
            public CursorHint Cursor;
            public SpriteRenderer Atom;
            public GameObject Selected;
        }

        public SpriteRenderer[] AtomSlots;

        [Header("Background")]
        public SpriteRenderer Background;
        public Color32 BackgroundDisabledColor = Color.white;
        public GameObject SemiconductorWarning;

        [Header("Selection")]
        public AtomSelector LeftSelector;
        public AtomSelector RightSelector;

        [Header("Valence")]
        public GameObject ValenceGroup;
        public SpriteRenderer[] ValencePips;
        public TMP_Text ValenceMinusOne;
        public TMP_Text ValencePlusOne;
        public Color32 BlankValencePipColor;
        public Color32 IncorrectValencePipColor;
        public Color32 CorrectNTypeValencePipColor;
        public Color32 CorrectPTypeValencePipColor;
        public TMP_Text FeedbackText;

        [Header("Voltage")]
        [Range(0, 1)] public float Temperature;
        public VoltageControl Voltage;

        [NonSerialized] private ResearchTool m_Tool;
        [NonSerialized] private int m_DopingIndex;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.Slots[0].OnSlotUpdated.Register(OnFirstSlotUpdated);
            m_Tool.Slots[1].OnSlotUpdated.Register(OnSecondSlotUpdated);

            LeftSelector.Cursor.onClick.Register(OnLeftAtomClicked);
            RightSelector.Cursor.onClick.Register(OnRightAtomClicked);

            Voltage.OnVoltageModified.Register(UpdateVoltage);
        }

        private void OnLeftAtomClicked() {
            m_DopingIndex = 0;
            SetSelectedAtomVisuals(m_DopingIndex);
            DisplayAtomicView(m_Tool.Slots[0].Item.Material.Atoms, m_DopingIndex);
            UpdateValencePips(m_Tool.Slots[0].Item.Material.Atoms[0].ValenceElectrons, 0);
            HideFeedback();
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
        }

        private void OnRightAtomClicked() {
            m_DopingIndex = 1;
            SetSelectedAtomVisuals(m_DopingIndex);
            DisplayAtomicView(m_Tool.Slots[0].Item.Material.Atoms, m_DopingIndex);
            UpdateValencePips(m_Tool.Slots[0].Item.Material.Atoms[1].ValenceElectrons, 0);
            HideFeedback();
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
        }

        private void OnFirstSlotUpdated(ResearchSlot _, ResearchMaterialItem item) {
            HideFeedback();

            bool hasMaterial = item != null;
            bool validMaterial = hasMaterial;
            if (validMaterial) {
                ResearchMaterial material = item.Material;
                validMaterial = ResearchMaterialUtility.GetKnownProperties(material.AssetId).HasCategory(ResearchChipCategory.PropertyElectric);
            }

            if (!validMaterial) {
                ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
                m_Tool.Slots[1].Locked = true;
                GuiCommands.SetActive(m_Tool.Slots[1].gameObject, false);
                GuiCommands.SetActive(SemiconductorWarning, hasMaterial);
                LeftSelector.Cursor.gameObject.SetActive(false);
                RightSelector.Cursor.gameObject.SetActive(false);
                ClearAtomicView();
                ResearchMaterialUtility.UpdateContextMaterial(null);
                Background.color = BackgroundDisabledColor;
                m_DopingIndex = 0;
                m_Tool.DopingState = DopantType.None;
                return;
            }

            Assert.True(item.Material.Atoms.Length <= 2);
            ResearchMaterialUtility.UpdateContextMaterial(item.Material);

            if (item.Material.Atoms.Length > 1) {
                ResearchSprites sprites = Find.GlobalAsset<ResearchSprites>();

                LeftSelector.Cursor.gameObject.SetActive(true);
                RightSelector.Cursor.gameObject.SetActive(true);

                LeftSelector.Atom.sprite = sprites.AtomIcons[(int) item.Material.Atoms[0].Appearance];
                LeftSelector.Atom.transform.SetScale(ResearchMaterialUtility.CalculateAtomicSizeFactor(item.Material.Atoms[0].Size));
                LeftSelector.Atom.color = item.Material.Atoms[0].Color;

                RightSelector.Atom.sprite = sprites.AtomIcons[(int) item.Material.Atoms[1].Appearance];
                RightSelector.Atom.transform.SetScale(ResearchMaterialUtility.CalculateAtomicSizeFactor(item.Material.Atoms[1].Size));
                RightSelector.Atom.color = item.Material.Atoms[1].Color;
            } else {
                LeftSelector.Cursor.gameObject.SetActive(false);
                RightSelector.Cursor.gameObject.SetActive(false);
            }

            m_Tool.Slots[1].Locked = false;
            ResearchSlotUtility.FillInSlot(m_Tool.Slots[1], null);
            GuiCommands.SetActive(m_Tool.Slots[1].gameObject, true);
            GuiCommands.SetActive(SemiconductorWarning, false);
            Background.color = Color.white;
            m_DopingIndex = 0;
            m_Tool.DopingState = DopantType.None;
            SetSelectedAtomVisuals(0);
            DisplayAtomicView(item.Material.Atoms, m_DopingIndex);
            UpdateValencePips(item.Material.Atoms[m_DopingIndex].ValenceElectrons, 0);
        }

        private void OnSecondSlotUpdated(ResearchSlot _, ResearchMaterialItem item) {
            if (!m_Tool.Slots[0].Item) {
                return;
            }

            if (item == null) {
                m_Tool.DopingState = DopantType.None;
                UpdateValencePips(m_Tool.Slots[0].Item.Material.Atoms[m_DopingIndex].ValenceElectrons, 0);
                return;
            }

            m_Tool.DopingState = DopantType.None;

            ResearchMaterial targetMaterial = m_Tool.Slots[0].Item.Material;
            AtomicStructure targetAtom = targetMaterial.Atoms[m_DopingIndex];
            ResearchMaterial dopant = item.Material;
            StringHash32 dopantId = dopant.AssetId;
            AtomicStructure dopantAtom = dopant.Atoms[0];

            if (targetMaterial.DopantN == dopantId && dopantAtom.ValenceElectrons == targetAtom.ValenceElectrons + 1) {
                ShowFeedback("N-Type Dopant", CorrectNTypeValencePipColor);
                m_Tool.DopingState = DopantType.N;
            } else if (targetMaterial.DopantP == dopantId && dopantAtom.ValenceElectrons == targetAtom.ValenceElectrons - 1) {
                ShowFeedback("P-Type Dopant", CorrectPTypeValencePipColor);
                m_Tool.DopingState = DopantType.P;
            } else if (dopant.Atoms.Length > 1) {
                ShowFeedback("Polyelemental", IncorrectValencePipColor);
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.TooBig);
            } else if (dopantAtom.Size >= targetAtom.Size) {
                ShowFeedback("Atomic Size Too Big", IncorrectValencePipColor);
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.TooBig);
            } else if (dopantAtom.ValenceElectrons < targetAtom.ValenceElectrons - 1) {
                ShowFeedback("Too Few Electrons", IncorrectValencePipColor);
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.InvalidCombo);
            } else if (dopantAtom.ValenceElectrons > targetAtom.ValenceElectrons + 1) {
                ShowFeedback("Too Many Electrons", IncorrectValencePipColor);
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.InvalidCombo);
            } else {
                ShowFeedback("Unknown Error", IncorrectValencePipColor);
                ResearchMaterialUtility.ExplodeItem(item, ExplosionStyle.InvalidCombo);
            }

            UpdateVoltage();
            UpdateValencePips(targetAtom.ValenceElectrons, dopantAtom.ValenceElectrons);
        }

        private void ClearAtomicView() {
            foreach(var atomSlot in AtomSlots) {
                atomSlot.enabled = false;
            }

            ValenceGroup.SetActive(false);
        }

        private void HideFeedback() {
            FeedbackText.enabled = false;
        }

        private void UpdateVoltage() {
            if (!m_Tool.AllSlotsFilled || m_Tool.DopingState != DopantType.None) {
                ClearVoltage();
                return;
            }

            var input = m_Tool.Slots[0].Item.Material;
            float current = ResearchMaterialUtility.GetCurrent(input, Voltage.InputVoltage, Temperature, m_Tool.DopingState);

            //if (m_Tool.DopingState == DopantType.N && current > 0) {

            //}

            CircuitUtility.SetLightStrength(m_Tool.Circuit, current);
            CircuitUtility.SetFlowSpeed(m_Tool.Circuit, current);

            ResearchToolUtility.SetLightEmissionStrength(m_Tool.SlotsEffectPosition, (input.SpecialTags & SpecialTag.LightEmitting) != 0 ? current : 0);
            ResearchToolUtility.SetHighMobilityStrength(m_Tool.SlotsEffectPosition, (input.SpecialTags & SpecialTag.HighMobility) != 0 ? current : 0);
        }

        private void ClearVoltage() {
            CircuitUtility.SetLightStrength(m_Tool.Circuit, 0);
            CircuitUtility.SetFlowSpeed(m_Tool.Circuit, 0);
            ResearchToolUtility.SetLightEmissionStrength(null, 0);
            ResearchToolUtility.SetHighMobilityStrength(null, 0);
        }
        
        private void ShowFeedback(string text, Color color) {
            FeedbackText.enabled = true;
            FeedbackText.SetText(text);
            FeedbackText.color = color;
        }

        private void DisplayAtomicView(AtomicStructure[] atoms, int offset) {
            ResearchSprites sprites = Find.GlobalAsset<ResearchSprites>();

            for (int i = 0; i < AtomSlots.Length; i++) {
                AtomicStructure atomData = atoms[(offset + i + 1) % atoms.Length];
                SpriteRenderer renderer = AtomSlots[i];
                renderer.sprite = sprites.AtomIcons[(int) atomData.Appearance];
                renderer.transform.SetScale(ResearchMaterialUtility.CalculateAtomicSizeFactor(atomData.Size));
                renderer.color = atomData.Color;
                renderer.enabled = true;
            }

            ValenceGroup.SetActive(true);
            int targetValence = atoms[offset].ValenceElectrons;
            //ValenceMinusOne.transform.localPosition = ValencePips[targetValence - 1 - 1].transform.localPosition;
            //ValencePlusOne.transform.localPosition = ValencePips[targetValence - 1 + 1].transform.localPosition;
        }

        private void UpdateValencePips(int targetValence, int currentValence) {
            Color32 currentColor = IncorrectValencePipColor;
            Color32 defaultColor = BlankValencePipColor;
            Color32 excessColor = default(Color32);
            if (currentValence == targetValence + 1) {
                currentColor = CorrectNTypeValencePipColor;
            } else if (currentValence == targetValence - 1) {
                currentColor = CorrectPTypeValencePipColor;
            }

            for(int i = 0; i < ValencePips.Length; i++) {
                ValencePips[i].color = (i < currentValence) ? currentColor : ((i < (targetValence)) ? defaultColor : excessColor);
            }
        }

        private void SetSelectedAtomVisuals(int index) {
            LeftSelector.Cursor.GetComponent<Collider2D>().enabled = index != 0;
            LeftSelector.Selected.SetActive(index == 0);
            RightSelector.Cursor.GetComponent<Collider2D>().enabled = index != 1;
            RightSelector.Selected.SetActive(index == 1);
        }
    }
}