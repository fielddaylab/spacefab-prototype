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
using UnityEngine.UI;

namespace SpaceFab.Research {
	public sealed class ResearchKnownPropertyDisplay : MonoBehaviour {
        [Header("Data Panel")]
        public TMP_Text MaterialTitle;
        public ResearchKnownPropertyRow ElectricProperty;
        public ResearchKnownPropertyRow ThermalProperty;
        public ResearchKnownPropertyRow SpecialProperty;
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

            ElectricProperty.Click.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                Guesser.PopupElectrical(RootId);
            });
            ThermalProperty.Click.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                Guesser.PopupThermal(RootId);
            });
            SpecialProperty.Click.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                Guesser.PopupSpecial(RootId);
            });

            SubmitButton.onClick.Register(() => Routine.Start(this, OnClickSubmit()).TryManuallyUpdate(0));
        }

        private IEnumerator OnClickSubmit() {
            Find.State<ResearchSelectionState>().Locked = true;

            ElectricProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            ThermalProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            SpecialProperty.Click.GetComponent<Graphic>().raycastTarget = false;
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

            ElectricProperty.WriteEmptyRow();
            ThermalProperty.WriteEmptyRow();
            SpecialProperty.WriteEmptyRow();

            ElectricProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            ThermalProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            SpecialProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            SubmitButton.gameObject.SetActive(false);
        }

        private void DisplayCurrent(ResearchMaterial material) {
            if (!material) {
                DisplayNull();
                return;
            }


            StringHash32 rootId = ResearchMaterialUtility.GetRootMaterial(material);
            RootId = rootId;

            ResearchMaterialKnowledge knowledge = ResearchMaterialUtility.GetKnownCategories(rootId);
            ResearchMaterialGuessState guesses = ResearchMaterialUtility.GetGuess(rootId);

            MaterialTitle.SetText(knowledge == ResearchMaterialKnowledge.All ? material.DisplayName : material.UnknownDisplayName);

            ElectricProperty.Click.GetComponent<Graphic>().raycastTarget = (knowledge & ResearchMaterialKnowledge.Electrical) == 0;
            ThermalProperty.Click.GetComponent<Graphic>().raycastTarget = (knowledge & ResearchMaterialKnowledge.Thermal) == 0;
            SpecialProperty.Click.GetComponent<Graphic>().raycastTarget = (knowledge & ResearchMaterialKnowledge.Special) == 0;

            ElectricProperty.PrepareWrite();
            if ((knowledge & ResearchMaterialKnowledge.Electrical) != 0) {
                WriteChips(ElectricProperty, material.Electrical, material.DopantType, true);
            } else if (guesses.Electric != ElectricalTag.Unknown || guesses.Dopant != DopantType.None) {
                WriteChips(ElectricProperty, guesses.Electric, guesses.Dopant, false);
            }
            ElectricProperty.FinishWrite();

            ThermalProperty.PrepareWrite();
            if ((knowledge & ResearchMaterialKnowledge.Thermal) != 0) {
                WriteChips(ThermalProperty, material.Thermal, true);
            } else if (guesses.Thermal.HasValue) {
                WriteChips(ThermalProperty, guesses.Thermal.Value, false);
            }
            ThermalProperty.FinishWrite();

            SpecialProperty.PrepareWrite();
            if ((knowledge & ResearchMaterialKnowledge.Special) != 0) {
                WriteChips(SpecialProperty, material.SpecialTags, true);
            } else if (guesses.Special.HasValue) {
                WriteChips(SpecialProperty, guesses.Special.Value, false);
            }
            SpecialProperty.FinishWrite();

            if (guesses.Special.HasValue || guesses.Electric != ElectricalTag.Unknown || guesses.Dopant != DopantType.None || guesses.Thermal.HasValue) {
                SubmitButton.gameObject.SetActive(true);
            } else {
                SubmitButton.gameObject.SetActive(false);
            }

            UnselectedValenceAppearance.SetActive(false);
            SelectedValenceAppearance.SetActive(true);

            ResearchMaterialUtility.PopulateDiagram(ValenceDiagram, material, knowledge == ResearchMaterialKnowledge.All);
        }

        static private void WriteChips(ResearchKnownPropertyRow row, ElectricalTag electrical, DopantType dopant, bool confirmed) {
            switch(electrical) {
                case ElectricalTag.Conductor: {
                    row.WriteChip("COND", confirmed);
                    break;
                }
                case ElectricalTag.Semiconductor: {
                    row.WriteChip("SEMI", confirmed);
                    break;
                }
                case ElectricalTag.Insulator: {
                    row.WriteChip("INSL", confirmed);
                    break;
                }
            }

            switch(dopant) {
                case DopantType.N: {
                    row.WriteChip("N.DOPE", confirmed);
                    break;
                }
                case DopantType.P: {
                    row.WriteChip("P.DOPE", confirmed);
                    break;
                }
            }
        }

        static private void WriteChips(ResearchKnownPropertyRow row, ThermalTag thermal, bool confirmed) {
            if (thermal == ThermalTag.None) {
                row.WriteChip("WEAK", confirmed);
            } else {
                if ((thermal & ThermalTag.HighTemp) != 0) {
                    row.WriteChip("HIGH", confirmed);
                }
                if ((thermal & ThermalTag.LowTemp) != 0) {
                    row.WriteChip("LOW", confirmed);
                }
            }
        }

        static private void WriteChips(ResearchKnownPropertyRow row, SpecialTag special, bool confirmed) {
            if (special == SpecialTag.None) {
                row.WriteChip("NONE", confirmed);
            } else {
                if ((special & SpecialTag.LightEmitting) != 0) {
                    row.WriteChip("LIGHT", confirmed);
                }
                if ((special & SpecialTag.HighMobility) != 0) {
                    row.WriteChip("H.MOB", confirmed);
                }
                if ((special & SpecialTag.HighVoltage) != 0) {
                    row.WriteChip("H.VOLT", confirmed);
                }
            }
        }
    }
}