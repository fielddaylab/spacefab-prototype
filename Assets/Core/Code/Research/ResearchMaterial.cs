using System;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Research Material")]
    public sealed class ResearchMaterial : NamedAsset {
        public string DisplayName;
        public string ChemicalSymbol;
        public Material Material;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 Parent;

        [Header("Properties")]
        public ElectricalTag Electrical;
        public ThermalTag Thermal;
        public SpecialTag[] SpecialTags;

        [Header("Fields")]
        public float DielectricStrength;
        public DopantType DopantType;
    }

    public enum ElectricalTag : uint {
        Unknown = 0,
        Conductor,
        Insulator,
        Semiconductor,
        Dopant
    }

    public enum ThermalTag : uint {
        Unknown = 0,
        HighTemp,
        LowTemp,
        ExtremeTemp,
        Sensitive
    }

    public enum SpecialTag : uint {
        Unknown = 0,
        HighMobility,
        LightEmitting
    }

    public enum DopantType : uint {
        Unknown = 0,
        N,
        P
    }

    static public partial class ResearchMaterialUtility {
        static public float GetCurrent(ResearchMaterial material, float voltage, float temperature) {
            // TODO: implement correctly
            switch(material.Electrical) {
                case ElectricalTag.Dopant: {
                    return 0;
                }
                case ElectricalTag.Insulator: {
                    return 0;
                }
                case ElectricalTag.Conductor: {
                    return voltage * (0.2f + 0.8f * (1 - temperature));
                }
                case ElectricalTag.Semiconductor: {
                    return voltage * (0.2f + 0.8f * temperature);
                }
                default: {
                    Assert.Fail("unknown electrical mode");
                    return 0;
                }
            }
        }

        static public bool BehavesAsInsulator(ResearchMaterial material) {
            switch(material.Electrical) {
                case ElectricalTag.Dopant:
                case ElectricalTag.Insulator:
                    return true;

                default:
                    return false;
            }
        }

        static public bool IsStableAtTemperature(ResearchMaterial material, float temperature) {
            switch(material.Thermal) {
                case ThermalTag.LowTemp:
                    return temperature <= 0.75f;
                case ThermalTag.HighTemp:
                    return temperature >= 0.25f;
                case ThermalTag.Sensitive:
                    return temperature >= 0.25f && temperature <= 0.75f;
                default:
                    return true;
            }
        }

        static public string GetTagLabel(ElectricalTag tag, DopantType dopantType) {
            switch(tag) {
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
                    switch(dopantType) {
                        case DopantType.Unknown: {
                            return "Dopant";
                        }
                        case DopantType.N: {
                            return "Dopant (N)";
                        }
                        case DopantType.P: {
                            return "Dopant (P)";
                        }
                    }
                    break;
                }
                case ElectricalTag.Unknown: {
                    return "???";
                }
            }

            Assert.Fail("no tag label");
            return string.Empty;
        }

        static public string GetTagLabel(ThermalTag tag) {
            switch (tag) {
                case ThermalTag.LowTemp: {
                    return "Low Temp";
                }
                case ThermalTag.HighTemp: {
                    return "High Temp";
                }
                case ThermalTag.ExtremeTemp: {
                    return "Extreme Temps";
                }
                case ThermalTag.Sensitive: {
                    return "Sensitive";
                }
                case ThermalTag.Unknown: {
                    return "???";
                }
            }

            Assert.Fail("no tag label");
            return string.Empty;
        }

        static public string GetTagLabel(SpecialTag tag) {
            switch (tag) {
                case SpecialTag.LightEmitting: {
                    return "Light-Emitting";
                }
                case SpecialTag.HighMobility: {
                    return "High Mobility";
                }
                case SpecialTag.Unknown: {
                    return "???";
                }
            }

            Assert.Fail("no tag label");
            return string.Empty;
        }
    }
}