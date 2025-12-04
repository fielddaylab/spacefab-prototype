using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.SharedState;
using SpaceFab.Research;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
	public sealed class ResearchKnownPropertyDisplay : MonoBehaviour {
        [Header("Data Panel")]
		public TMP_Text MaterialTitle;
        public TMP_Text ElectricProperty;
        public TMP_Text ThermalProperty;
        public TMP_Text SpecialProperty;
        public PointerListener ElectricClick;
        public PointerListener ThermalClick;
        public PointerListener SpecialClick;
        public PointerListener SubmitButton;
        public ResearchGuessDisplay Guesser;

        [Header("Valence Panel")]
        public ResearchValenceDiagram ValenceDiagram;
        public ActiveGroup UnselectedValenceAppearance;
        public ActiveGroup SelectedValenceAppearance;

        [NonSerialized] public StringHash32 RootId;

        private void Awake() {
            DisplayNull();

            Find.State<ResearchSelectionState>().OnUpdated.Register(DisplayCurrent);
            Guesser.OnClose.Register(() => {
                DisplayCurrent(Find.State<ResearchSelectionState>().Current);
            });

            //ElectricClick.onClick.Register(() => {
            //    SubmitButton.gameObject.SetActive(false);
            //    Guesser.PopupElectrical(RootId);
            //});
            //ThermalClick.onClick.Register(() => {
            //    SubmitButton.gameObject.SetActive(false);
            //    Guesser.PopupThermal(RootId);
            //});
            //SpecialClick.onClick.Register(() => {
            //    SubmitButton.gameObject.SetActive(false);
            //    Guesser.PopupSpecial(RootId);
            //});

            //SubmitButton.onClick.Register(() => Routine.Start(this, OnClickSubmit()).TryManuallyUpdate(0));
        }

        private IEnumerator OnClickSubmit() {
            Find.State<ResearchSelectionState>().Locked = true;

            ElectricClick.GetComponent<Collider2D>().enabled = false;
            ThermalClick.GetComponent<Collider2D>().enabled = false;
            SpecialClick.GetComponent<Collider2D>().enabled = false;
            SubmitButton.gameObject.SetActive(false);

            yield return 1;

            bool areAllCorrect = ResearchMaterialUtility.ProcessGuess(RootId);
            if (areAllCorrect) {
                // TODO: play some fanfare or something
            }

            Find.State<ResearchSelectionState>().Locked = false;

            DisplayCurrent(Find.State<ResearchSelectionState>().Current);
        }

        private void DisplayNull() {
            RootId = default;
            MaterialTitle.SetText("???");
            SelectedValenceAppearance.SetActive(false);
            UnselectedValenceAppearance.SetActive(true);
            //ElectricProperty.SetText("???");
            //ThermalProperty.SetText("???");
            //SpecialProperty.SetText("???");
            //ElectricClick.GetComponent<Collider2D>().enabled = false;
            //ThermalClick.GetComponent<Collider2D>().enabled = false;
            //SpecialClick.GetComponent<Collider2D>().enabled = false;;
            //SubmitButton.gameObject.SetActive(false);
        }

        private void DisplayCurrent(ResearchMaterial material) {
            if (!material) {
                DisplayNull();
                return;
            }

            MaterialTitle.SetText(material.DisplayName);

            StringHash32 rootId = ResearchMaterialUtility.GetRootMaterial(material);
            RootId = rootId;

            ResearchMaterialKnowledge knowledge = ResearchMaterialUtility.GetKnownCategories(rootId);
            ResearchMaterialGuessState guesses = ResearchMaterialUtility.GetGuess(rootId);

            //using (PooledStringBuilder psb = PooledStringBuilder.Create()) {

            //    if ((knowledge & ResearchMaterialKnowledge.Electrical) != 0) {
            //        ResearchMaterialUtility.GetTagLabel(psb, material.Electrical, material.DopantType);
            //        ElectricProperty.SetText(psb);
            //        ElectricClick.GetComponent<Collider2D>().enabled = false;
            //    } else if (guesses.Electric != ElectricalTag.Unknown) {
            //        ResearchMaterialUtility.GetTagLabel(psb, guesses.Electric, guesses.Dopant);
            //        psb.Builder.Append(" (?)");
            //        ElectricProperty.SetText(psb);
            //        ElectricClick.GetComponent<Collider2D>().enabled = true;
            //    } else {
            //        ElectricProperty.SetText("???");
            //        ElectricClick.GetComponent<Collider2D>().enabled = true;
            //    }

            //    psb.Builder.Clear();

            //    if ((knowledge & ResearchMaterialKnowledge.Thermal) != 0) {
            //        ResearchMaterialUtility.GetTagLabel(psb, material.Thermal);
            //        ThermalProperty.SetText(psb);
            //        ThermalClick.GetComponent<Collider2D>().enabled = false;
            //    } else if (guesses.Thermal.HasValue) {
            //        ResearchMaterialUtility.GetTagLabel(psb, guesses.Thermal.Value);
            //        psb.Builder.Append(" (?)");
            //        ThermalProperty.SetText(psb);
            //        ThermalClick.GetComponent<Collider2D>().enabled = true;
            //    } else {
            //        ThermalProperty.SetText("???");
            //        ThermalClick.GetComponent<Collider2D>().enabled = true;
            //    }

            //    psb.Builder.Clear();

            //    if ((knowledge & ResearchMaterialKnowledge.Special) != 0) {
            //        ResearchMaterialUtility.GetTagLabel(psb, material.SpecialTags);
            //        SpecialProperty.SetText(psb);
            //        SpecialClick.GetComponent<Collider2D>().enabled = false;
            //    } else if (guesses.Special.HasValue) {
            //        ResearchMaterialUtility.GetTagLabel(psb, guesses.Special.Value);
            //        psb.Builder.Append(" (?)");
            //        SpecialProperty.SetText(psb);
            //        SpecialClick.GetComponent<Collider2D>().enabled = true;
            //    } else {
            //        SpecialProperty.SetText("???");
            //        SpecialClick.GetComponent<Collider2D>().enabled = true;
            //    }
            //}

            //if (guesses.Special.HasValue || guesses.Electric != ElectricalTag.Unknown || guesses.Thermal.HasValue) {
            //    SubmitButton.gameObject.SetActive(true);
            //} else {
            //    SubmitButton.gameObject.SetActive(false);
            //}

            UnselectedValenceAppearance.SetActive(false);
            SelectedValenceAppearance.SetActive(true);

            ResearchMaterialUtility.PopulateDiagram(ValenceDiagram, material, knowledge == ResearchMaterialKnowledge.All);
        }
    }
}