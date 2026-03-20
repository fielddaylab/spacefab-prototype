using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI.Animation;
using SpaceFab.Research;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    [PreloadOrder(101)]
	public sealed class ResearchKnownPropertyDisplay : MonoBehaviour, IScenePreload {
        [Header("Data Panel")]
        public TMP_Text MaterialTitle;
        public ResearchKnownPropertyRow ElectricProperty;
        public ResearchKnownPropertyRow DopantProperty;
        public ResearchKnownPropertyRow ThermalProperty;
        public ResearchKnownPropertyRow SpecialProperty;
        public GameObject NTypeGroup;
        public GameObject PTypeGroup;
        public RectTransform RowHighlight;
        public PointerListener SubmitButton;
        public ActiveGroup SubmitInProgress;
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
                RowHighlight.gameObject.SetActive(false);
                DisplayCurrent(Find.State<ResearchSelectionState>().Current);
            });

            ElectricProperty.Click.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                RowHighlight.gameObject.SetActive(true);
                RowHighlight.localPosition = ElectricProperty.transform.localPosition;
                Guesser.PopupElectrical(RootId);
            });
            DopantProperty.Click.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                RowHighlight.gameObject.SetActive(true);
                RowHighlight.localPosition = DopantProperty.transform.localPosition;
                Guesser.PopupDopant(RootId);
            });
            ThermalProperty.Click.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                RowHighlight.gameObject.SetActive(true);
                RowHighlight.localPosition = ThermalProperty.transform.localPosition;
                Guesser.PopupThermal(RootId);
            });
            SpecialProperty.Click.onClick.Register(() => {
                SubmitButton.gameObject.SetActive(false);
                RowHighlight.gameObject.SetActive(true);
                RowHighlight.localPosition = SpecialProperty.transform.localPosition;
                Guesser.PopupSpecial(RootId);
            });

            SubmitButton.onClick.Register(() => Routine.Start(this, OnClickSubmit()).TryManuallyUpdate(0));
            SpaceFabGame.Events.Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_KnowledgeUpdated, OnKnowledgeUpdated);
        }

        private void OnDestroy() {
            Game.Events?.DeregisterAllForContext(this);
        }

        private IEnumerator OnClickSubmit() {
            Find.State<ResearchSelectionState>().Locked = true;

            ElectricProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            DopantProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            ThermalProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            SpecialProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            SubmitButton.gameObject.SetActive(false);
            RowHighlight.gameObject.SetActive(false);
            SubmitInProgress.SetActive(true);

            yield return 1;

            ResearchMaterialGuessResult result = ResearchMaterialUtility.ProcessGuess(RootId);
            if (result.Correct != 0) {
                Sfx.Play("Research.Row.Correct");
            } else {
                Sfx.Play("Research.Row.Incorrect");
            }

            if ((result.Correct & ResearchMaterialKnowledge.Electrical) != 0) {
                FlashAnim.Play(ElectricProperty.Flash, Color.white, FlashAnim.Default);
            } else if ((result.Incorrect & ResearchMaterialKnowledge.Electrical) != 0) {
                FlashAnim.Play(ElectricProperty.Flash, Color.red, FlashAnim.Default);
            }

            if ((result.Correct & ResearchMaterialKnowledge.Dopant) != 0) {
                FlashAnim.Play(DopantProperty.Flash, Color.white, FlashAnim.Default);
            } else if ((result.Incorrect & ResearchMaterialKnowledge.Dopant) != 0) {
                FlashAnim.Play(DopantProperty.Flash, Color.red, FlashAnim.Default);
            }

            if ((result.Correct & ResearchMaterialKnowledge.Thermal) != 0) {
                FlashAnim.Play(ThermalProperty.Flash, Color.white, FlashAnim.Default);
            } else if ((result.Incorrect & ResearchMaterialKnowledge.Thermal) != 0) {
                FlashAnim.Play(ThermalProperty.Flash, Color.red, FlashAnim.Default);
            }

            if ((result.Correct & ResearchMaterialKnowledge.Special) != 0) {
                FlashAnim.Play(SpecialProperty.Flash, Color.white, FlashAnim.Default);
            } else if ((result.Incorrect & ResearchMaterialKnowledge.Special) != 0) {
                FlashAnim.Play(SpecialProperty.Flash, Color.red, FlashAnim.Default);
            }

            Find.State<ResearchSelectionState>().Locked = false;

            SubmitInProgress.SetActive(false);
            DisplayCurrent(Find.State<ResearchSelectionState>().Current);
        }

        private void OnKnowledgeUpdated(ResearchMaterialKnowledgePair pair) {
            if (pair.MaterialId != RootId) {
                return;
            }

            NTypeGroup.SetActive((pair.Knowledge & ResearchMaterialKnowledge.DopantMaterialN) != 0);
            PTypeGroup.SetActive((pair.Knowledge & ResearchMaterialKnowledge.DopantMaterialP) != 0);
        }

        private void DisplayNull() {
            RootId = default;
            MaterialTitle.SetText("???");
            
            SelectedValenceAppearance.SetActive(false);
            UnselectedValenceAppearance.SetActive(true);

            ElectricProperty.WriteEmptyRow();
            DopantProperty.WriteEmptyRow();
            ThermalProperty.WriteEmptyRow();
            SpecialProperty.WriteEmptyRow();

            ElectricProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            DopantProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            ThermalProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            SpecialProperty.Click.GetComponent<Graphic>().raycastTarget = false;
            RowHighlight.gameObject.SetActive(false);
            SubmitButton.gameObject.SetActive(false);

            NTypeGroup.SetActive(false);
            PTypeGroup.SetActive(false);
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

            MaterialTitle.SetText((knowledge & ResearchMaterialKnowledge.Name) != 0 ? material.DisplayName : material.UnknownDisplayName);

            ElectricProperty.Click.GetComponent<Graphic>().raycastTarget = (knowledge & ResearchMaterialKnowledge.Electrical) == 0;
            DopantProperty.Click.GetComponent<Graphic>().raycastTarget = (knowledge & ResearchMaterialKnowledge.Dopant) == 0;
            ThermalProperty.Click.GetComponent<Graphic>().raycastTarget = (knowledge & ResearchMaterialKnowledge.Thermal) == 0;
            SpecialProperty.Click.GetComponent<Graphic>().raycastTarget = (knowledge & ResearchMaterialKnowledge.Special) == 0;

            NTypeGroup.SetActive((knowledge & ResearchMaterialKnowledge.DopantMaterialN) != 0);
            PTypeGroup.SetActive((knowledge & ResearchMaterialKnowledge.DopantMaterialP) != 0);

            ElectricProperty.PrepareWrite();
            if ((knowledge & ResearchMaterialKnowledge.Electrical) != 0) {
                WriteChips(ElectricProperty, material.Electrical, true);
            } else if (guesses.Electric != ElectricalTag.Unknown) {
                WriteChips(ElectricProperty, guesses.Electric, false);
            }
            ElectricProperty.FinishWrite((knowledge & ResearchMaterialKnowledge.Electrical) != 0);

            DopantProperty.PrepareWrite();
            if ((knowledge & ResearchMaterialKnowledge.Dopant) != 0) {
                WriteChips(DopantProperty, material.DopantType, true);
            } else if (guesses.Dopant.HasValue) {
                WriteChips(DopantProperty, guesses.Dopant.Value, false);
            }
            DopantProperty.FinishWrite((knowledge & ResearchMaterialKnowledge.Dopant) != 0);

            ThermalProperty.PrepareWrite();
            if ((knowledge & ResearchMaterialKnowledge.Thermal) != 0) {
                WriteChips(ThermalProperty, material.Thermal, true);
            } else if (guesses.Thermal.HasValue) {
                WriteChips(ThermalProperty, guesses.Thermal.Value, false);
            }
            ThermalProperty.FinishWrite((knowledge & ResearchMaterialKnowledge.Thermal) != 0);

            SpecialProperty.PrepareWrite();
            if ((knowledge & ResearchMaterialKnowledge.Special) != 0) {
                WriteChips(SpecialProperty, material.SpecialTags, true);
            } else if (guesses.Special.HasValue) {
                WriteChips(SpecialProperty, guesses.Special.Value, false);
            }
            SpecialProperty.FinishWrite((knowledge & ResearchMaterialKnowledge.Special) != 0);

            if (guesses.Special.HasValue || guesses.Electric != ElectricalTag.Unknown || guesses.Dopant.HasValue || guesses.Thermal.HasValue) {
                SubmitButton.gameObject.SetActive(true);
            } else {
                SubmitButton.gameObject.SetActive(false);
            }

            UnselectedValenceAppearance.SetActive(false);
            SelectedValenceAppearance.SetActive(true);

            ResearchMaterialUtility.PopulateDiagram(ValenceDiagram, material, (knowledge & ResearchMaterialKnowledge.Name) != 0);
        }

        static private void WriteChips(ResearchKnownPropertyRow row, ElectricalTag electrical, bool confirmed) {
            switch(electrical) {
                case ElectricalTag.Conductor: {
                    row.WriteChip("CONDUCTOR", confirmed);
                    break;
                }
                case ElectricalTag.Semiconductor: {
                    row.WriteChip("SEMICONDUCTOR", confirmed);
                    break;
                }
                case ElectricalTag.Insulator: {
                    row.WriteChip("INSULATOR", confirmed);
                    break;
                }
            }
        }

        static private void WriteChips(ResearchKnownPropertyRow row, DopantType dopant, bool confirmed) {
            if (dopant == DopantType.None) {
                row.WriteChip("NOT DOPANT", confirmed);
            } else {
                if ((dopant & DopantType.N) != 0) {
                    row.WriteChip("N-TYPE", confirmed);
                }
                if ((dopant & DopantType.P) != 0) {
                    row.WriteChip("P-TYPE", confirmed);
                }
            }
        }

        static private void WriteChips(ResearchKnownPropertyRow row, ThermalTag thermal, bool confirmed) {
            if (thermal == ThermalTag.None) {
                row.WriteChip("WEAK", confirmed);
            } else {
                if ((thermal & ThermalTag.HighTemp) != 0) {
                    row.WriteChip("RESIST HIGH", confirmed);
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

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            if (ResearchGame.CurrentLevel) {
                ResearchMaterialKnowledge toDisplay = ResearchGame.CurrentLevel.AvailableProperties;
                if ((toDisplay & ResearchMaterialKnowledge.Electrical) == 0) {
                    ElectricProperty.Group.blocksRaycasts = false;
                    ElectricProperty.Group.alpha = 0.25f;
                }
                if ((toDisplay & ResearchMaterialKnowledge.Dopant) == 0) {
                    DopantProperty.Group.blocksRaycasts = false;
                    DopantProperty.Group.alpha = 0.25f;
                }
                if ((toDisplay & ResearchMaterialKnowledge.Thermal) == 0) {
                    ThermalProperty.Group.blocksRaycasts = false;
                    ThermalProperty.Group.alpha = 0.25f;
                }
                if ((toDisplay & ResearchMaterialKnowledge.Special) == 0) {
                    SpecialProperty.Group.blocksRaycasts = false;
                    SpecialProperty.Group.alpha = 0.25f;
                }
            }
            return null;
        }
    }
}