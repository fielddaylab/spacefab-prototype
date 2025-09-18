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
		public TMP_Text MaterialTitle;
        public TMP_Text ElectricProperty;
        public TMP_Text ThermalProperty;
        public TMP_Text SpecialProperty;
        public PointerListener ElectricClick;
        public PointerListener ThermalClick;
        public PointerListener SpecialClick;
        public PointerListener SubmitButton;
        public ResearchGuessDisplay Guesser;

        [NonSerialized] public StringHash32 RootId;

        private void Awake() {
            DisplayNull();

            Find.State<ResearchSelectionState>().OnUpdated.Register(DisplayCurrent);
            Guesser.OnClose.Register(() => {
                DisplayCurrent(Find.State<ResearchSelectionState>().Current);
            });

            ElectricClick.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                Guesser.PopupElectrical(RootId);
            });
            ThermalClick.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                Guesser.PopupThermal(RootId);
            });

            SubmitButton.onClick.Register(() => Routine.Start(this, OnClickSubmit()).TryManuallyUpdate(0));
        }

        private IEnumerator OnClickSubmit() {
            Find.State<ResearchSelectionState>().Locked = true;

            ElectricClick.GetComponent<Collider2D>().enabled = false;
            ThermalClick.GetComponent<Collider2D>().enabled = false;
            SpecialClick.GetComponent<Collider2D>().enabled = false; ;
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
            MaterialTitle.SetText("???");
            ElectricProperty.SetText("???");
            ThermalProperty.SetText("???");
            SpecialProperty.SetText("???");
            ElectricClick.GetComponent<Collider2D>().enabled = false;
            ThermalClick.GetComponent<Collider2D>().enabled = false;
            SpecialClick.GetComponent<Collider2D>().enabled = false;;
            RootId = default;
            SubmitButton.gameObject.SetActive(false);
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

            if ((knowledge & ResearchMaterialKnowledge.Electrical) != 0) {
                ElectricProperty.SetText(ResearchMaterialUtility.GetTagLabel(material.Electrical, material.DopantType));
                ElectricClick.GetComponent<Collider2D>().enabled = false;
            } else if (guesses.Electric != ElectricalTag.Unknown) {
                ElectricProperty.SetText(ResearchMaterialUtility.GetTagLabel(guesses.Electric, guesses.Dopant) + " (?)");
                ElectricClick.GetComponent<Collider2D>().enabled = true;
            } else {
                ElectricProperty.SetText("???");
                ElectricClick.GetComponent<Collider2D>().enabled = true;
            }

            if ((knowledge & ResearchMaterialKnowledge.Thermal) != 0) {
                ThermalProperty.SetText(ResearchMaterialUtility.GetTagLabel(material.Thermal));
                ThermalClick.GetComponent<Collider2D>().enabled = false;
            } else if (guesses.Thermal != ThermalTag.Unknown) {
                ThermalProperty.SetText(ResearchMaterialUtility.GetTagLabel(guesses.Thermal) + " (?)");
                ThermalClick.GetComponent<Collider2D>().enabled = true;
            } else {
                ThermalProperty.SetText("???");
                ThermalClick.GetComponent<Collider2D>().enabled = true;
            }

            //if ((knowledge & ResearchMaterialKnowledge.Special) != 0) {
            //    SpecialProperty.SetText(ResearchMaterialUtility.GetTagLabel(material.SpecialTags));
            //    SpecialClick.GetComponent<Collider2D>().enabled = false;
            //} else if (guesses.Thermal != ThermalTag.Unknown) {
            //    SpecialProperty.SetText(ResearchMaterialUtility.GetTagLabel(guesses.Thermal) + " (?)");
            //    SpecialClick.GetComponent<Collider2D>().enabled = true;
            //} else {
            //    SpecialProperty.SetText("???");
            //    SpecialClick.GetComponent<Collider2D>().enabled = true;
            //}

            if (guesses.Special != SpecialTag.Unknown || guesses.Electric != ElectricalTag.Unknown || guesses.Thermal != ThermalTag.Unknown) {
                SubmitButton.gameObject.SetActive(true);
            } else {
                SubmitButton.gameObject.SetActive(false);
            }
        }
    }
}