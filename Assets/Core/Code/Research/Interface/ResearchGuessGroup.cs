using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Scenes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchGuessGroup : MonoBehaviour, IScenePreload {
        public ResearchGuessButtonWidget[] Buttons;

        public CastableEvent<ResearchSelectionList> OnSelectionUpdated = new CastableEvent<ResearchSelectionList>(1);
        [NonSerialized] public BitSet32 SelectedIndices;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            int idx = 0;
            foreach(var button in Buttons) {
                button.Listener.onClick.Register(OnButtonClicked);
                button.Listener.UserData = button;
                button.Index = idx++;
            }
            return null;
        }

        public void PopulateInitialSelection(ResearchSelectionList selection) {
            BitSet32 newSelections = default;
            foreach(var button in Buttons) {
                if (selection.Contains(button.Id)) {
                    button.SelectionHighlight.SetActive(true);
                    newSelections.Set(button.Index);
                } else {
                    button.SelectionHighlight.SetActive(false);
                }
            }

            SelectedIndices = newSelections;
        }

        private void OnButtonClicked(PointerListener.EventData evt) {
            ResearchGuessButtonWidget widget = (ResearchGuessButtonWidget) evt.Source.UserData;
            if (SelectedIndices.IsSet(widget.Index)) {
                SelectedIndices.Unset(widget.Index);
                widget.SelectionHighlight.SetActive(false);
            } else {
                SelectedIndices.Set(widget.Index);
                widget.SelectionHighlight.SetActive(true);

                switch(widget.ButtonType) {
                    case ResearchGuessButtonType.DeselectOther: {
                        foreach(var bit in SelectedIndices) {
                            if (bit != widget.Index) {
                                SelectedIndices.Unset(bit);
                                Buttons[bit].SelectionHighlight.SetActive(false);
                            }
                        }
                        break;
                    }
                    case ResearchGuessButtonType.Exclusive: {
                        foreach (var bit in SelectedIndices) {
                            if (bit != widget.Index) {
                                if (Buttons[bit].Group == widget.Group || Buttons[bit].ButtonType == ResearchGuessButtonType.DeselectOther) {
                                    SelectedIndices.Unset(bit);
                                    Buttons[bit].SelectionHighlight.SetActive(false);
                                }
                            }
                        }
                        break;
                    }
                    case ResearchGuessButtonType.Default: {
                        foreach (var bit in SelectedIndices) {
                            if (bit != widget.Index) {
                                if (Buttons[bit].ButtonType == ResearchGuessButtonType.DeselectOther) {
                                    SelectedIndices.Unset(bit);
                                    Buttons[bit].SelectionHighlight.SetActive(false);
                                }
                            }
                        }
                        break;
                    }
                }
            }

            OnSelectionUpdated.Invoke(GetSelectionList());
        }

        private ResearchSelectionList GetSelectionList() {
            ResearchSelectionList list = ResearchSelectionList.Alloc(Buttons.Length);
            foreach(var bit in SelectedIndices) {
                list.Add(Buttons[bit].Id);
            }
            return list;
        }

        static public ResearchSelectionList GetElectricalGuessList(ResearchMaterialGuessState guessState) {
            ResearchSelectionList list = ResearchSelectionList.Alloc(4);
            switch(guessState.Electric) {
                case ElectricalTag.Conductor:
                    list.Add("Conductor");
                    break;
                case ElectricalTag.Semiconductor:
                    list.Add("Semiconductor");
                    break;
                case ElectricalTag.Insulator: {
                    list.Add("Insulator");
                    break;
                }
            }

            return list;
        }

        static public ResearchSelectionList GetDopantGuessList(ResearchMaterialGuessState guessState) {
            if (!guessState.Dopant.HasValue) {
                return default;
            }

            ResearchSelectionList list = ResearchSelectionList.Alloc(3);
            DopantType dopantGuess = guessState.Dopant.Value;
            if (dopantGuess == DopantType.None) {
                list.Add("NotADopant");
            } else {
                if ((dopantGuess & DopantType.N) != 0) {
                    list.Add("DopantN");
                }
                if ((dopantGuess & DopantType.P) != 0) {
                    list.Add("DopantP");
                }
            }

            return list;
        }

        static public ResearchSelectionList GetThermalGuessList(ResearchMaterialGuessState guessState) {
            if (!guessState.Thermal.HasValue) {
                return default;
            }

            ResearchSelectionList list = ResearchSelectionList.Alloc(3);
            ThermalTag thermalGuess = guessState.Thermal.Value;
            if (thermalGuess == ThermalTag.None) {
                list.Add("Sensitive");
            } else {
                if ((thermalGuess & ThermalTag.HighTemp) != 0) {
                    list.Add("HighTemp");
                }
            }

            return list;
        }

        static public ResearchSelectionList GetSpecialGuessList(ResearchMaterialGuessState guessState) {
            if (!guessState.Special.HasValue) {
                return default;
            }

            ResearchSelectionList list = ResearchSelectionList.Alloc(4);
            SpecialTag specialGuess = guessState.Special.Value;
            if (specialGuess == SpecialTag.None) {
                list.Add("Nothing");
            } else {
                if ((specialGuess & SpecialTag.HighMobility) != 0) {
                    list.Add("HighMobility");
                }
                if ((specialGuess & SpecialTag.HighVoltage) != 0) {
                    list.Add("HighVoltage");
                }
                if ((specialGuess & SpecialTag.LightEmitting) != 0) {
                    list.Add("LightEmitting");
                }
            }

            return list;
        }

        static public void PopulateElectricalGuess(ref ResearchMaterialGuessState guessState, ResearchSelectionList list) {
            if (list.Contains("Conductor")) {
                guessState.Electric = ElectricalTag.Conductor;
            } else if (list.Contains("Semiconductor")) {
                guessState.Electric = ElectricalTag.Semiconductor;
            } else if (list.Contains("Insulator")) {
                guessState.Electric = ElectricalTag.Insulator;
            } else {
                guessState.Electric = ElectricalTag.Unknown;
            }
        }

        static public void PopulateDopantGuess(ref ResearchMaterialGuessState guessState, ResearchSelectionList list) {
            DopantType dopantType = default;
            if (list.Length == 0) {
                guessState.Dopant = null;
            } else {
                if (list.Contains("DopantN")) {
                    dopantType |= DopantType.N;
                }
                if (list.Contains("DopantP")) {
                    dopantType |= DopantType.P;
                }
                if (list.Contains("NotADopant")) {
                    dopantType = DopantType.None;
                }
                guessState.Dopant = dopantType;
            }
        }

        static public void PopulateThermalGuess(ref ResearchMaterialGuessState guessState, ResearchSelectionList list) {
            ThermalTag thermalTags = default;
            if (list.Length == 0) {
                guessState.Thermal = null;
            } else {
                if (list.Contains("HighTemp")) {
                    thermalTags |= ThermalTag.HighTemp;
                }
                if (list.Contains("Sensitive")) {
                    thermalTags = ThermalTag.None;
                }
                guessState.Thermal = thermalTags;
            }
        }

        static public void PopulateSpecialGuess(ref ResearchMaterialGuessState guessState, ResearchSelectionList list) {
            SpecialTag specialTags = default;
            if (list.Length == 0) {
                guessState.Special = null;
            } else {
                if (list.Contains("HighMobility")) {
                    specialTags |= SpecialTag.HighMobility;
                }
                if (list.Contains("HighVoltage")) {
                    specialTags |= SpecialTag.HighVoltage;
                }
                if (list.Contains("LightEmitting")) {
                    specialTags |= SpecialTag.LightEmitting;
                }
                if (list.Contains("Nothing")) {
                    specialTags = SpecialTag.None;
                }
                guessState.Special = specialTags;
            }
        }
    }
}