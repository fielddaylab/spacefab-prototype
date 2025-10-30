using BeauUtil;
using BeauUtil.UI;
using FieldDay.Scenes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchGuessGroup : MonoBehaviour, IScenePreload {
        public Transform SelectionArrow;
        public ResearchGuessButtonWidget[] Buttons;

        public CastableEvent<StringHash32> OnSelectionUpdated = new CastableEvent<StringHash32>(1);
        [NonSerialized] public StringHash32 SelectionId;
        [NonSerialized] public int SelectionIndex = -1;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            int idx = 0;
            foreach(var button in Buttons) {
                button.Listener.onClick.Register(OnButtonClicked);
                button.Listener.UserData = button;
                button.Index = idx++;
            }
            return null;
        }

        public void PopulateInitialSelection(StringHash32 id) {
            foreach(var button in Buttons) {
                if (button.Data == id) {
                    button.Collider.enabled = false;
                    SelectionArrow.position = button.Position.position;
                    SelectionIndex = button.Index;
                } else {
                    button.Collider.enabled = true;
                }
            }

            SelectionArrow.gameObject.SetActive(!id.IsEmpty);
            
            SelectionId = id;
            if (id.IsEmpty) {
                SelectionIndex = -1;
            }
        }

        private void OnButtonClicked(PointerListener.EventData evt) {
            ResearchGuessButtonWidget widget = (ResearchGuessButtonWidget) evt.Source.UserData;
            if (SelectionId == widget.Data) {
                SelectionArrow.gameObject.SetActive(false);
                widget.Collider.enabled = true;
                SelectionId = StringHash32.Null;
                OnSelectionUpdated.Invoke(SelectionId);
            } else {
                if (SelectionIndex >= 0) {
                    Buttons[SelectionIndex].Collider.enabled = true;
                }
                widget.Collider.enabled = false;
                SelectionId = widget.Data;
                SelectionIndex = widget.Index;
                SelectionArrow.position = widget.Position.position;
                SelectionArrow.gameObject.SetActive(true);
                OnSelectionUpdated.Invoke(SelectionId);
            }
        }

        static public StringHash32 GetElectricalGuessId(ResearchMaterialGuessState guessState) {
            switch(guessState.Electric) {
                case ElectricalTag.Conductor: {
                    return "Conductor";
                }
                case ElectricalTag.Semiconductor: {
                    return "Semiconductor";
                }
                case ElectricalTag.Insulator: {
                    return "Insulator";
                }
                case ElectricalTag.Dopant: {
                    switch(guessState.Dopant) {
                        case DopantType.N: {
                            return "DopantN";
                        }
                        case DopantType.P: {
                            return "DopantP";
                        }
                        default: {
                            return "Dopant";
                        }
                    }
                }
                case ElectricalTag.Unknown:
                default: {
                    return StringHash32.Null;
                }
            }
        }

        static public void PopulateElectricalGuess(ref ResearchMaterialGuessState guessState, StringHash32 id) {
            if (id == "Conductor") {
                guessState.Dopant = DopantType.Unknown;
                guessState.Electric = ElectricalTag.Conductor;
            } else if (id == "Insulator") {
                guessState.Dopant = DopantType.Unknown;
                guessState.Electric = ElectricalTag.Insulator;
            } else if (id == "Semiconductor") {
                guessState.Dopant = DopantType.Unknown;
                guessState.Electric = ElectricalTag.Semiconductor;
            } else if (id == "Dopant") {
                guessState.Dopant = DopantType.Unknown;
                guessState.Electric = ElectricalTag.Dopant;
            } else if (id == "DopantN") {
                guessState.Dopant = DopantType.N;
                guessState.Electric = ElectricalTag.Dopant;
            } else if (id == "DopantP") {
                guessState.Dopant = DopantType.P;
                guessState.Electric = ElectricalTag.Dopant;
            } else {
                guessState.Dopant = DopantType.Unknown;
                guessState.Electric = ElectricalTag.Unknown;
            }
        }

        static public StringHash32 GetThermalGuessId(ResearchMaterialGuessState guessState) {
            switch (guessState.Thermal) {
                case ThermalTag.HighTemp: {
                    return "HighTemp";
                }
                case ThermalTag.LowTemp: {
                    return "LowTemp";
                }
                //case ThermalTag.ExtremeTemp: {
                //    return "ExtremeTemp";
                //}
                case ThermalTag.Sensitive: {
                    return "Sensitive";
                }
                case ThermalTag.Unknown:
                default: {
                    return StringHash32.Null;
                }
            }
        }

        static public void PopulateThermalGuess(ref ResearchMaterialGuessState guessState, StringHash32 id) {
            if (id == "HighTemp") {
                guessState.Thermal = ThermalTag.HighTemp;
            } else if (id == "LowTemp") {
                guessState.Thermal = ThermalTag.LowTemp;
            } else if (id == "ExtremeTemp") {
                //guessState.Thermal = ThermalTag.ExtremeTemp;
            } else if (id == "Sensitive") {
                guessState.Thermal = ThermalTag.Sensitive;
            } else {
                guessState.Thermal = ThermalTag.Unknown;
            }
        }
    }
}