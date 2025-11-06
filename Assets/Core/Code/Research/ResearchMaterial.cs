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
        public Sprite Diagram;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 Parent;

        [Header("Properties")]
        public ElectricalTag Electrical;
        public ThermalTag Thermal;
        public SpecialTag SpecialTags;

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

    [Flags]
    public enum ThermalTag : uint {
        Unknown = 0,
        HighTemp = 0x01,
        LowTemp = 0x02,
        Sensitive = 0x04
    }

    [Flags]
    public enum SpecialTag : uint {
        Unknown = 0,
        HighMobility = 0x01,
        LightEmitting = 0x02,
        HighVoltage = 0x04
    }

    public enum DopantType : uint {
        Unknown = 0,
        N,
        P
    }

    static public partial class ResearchMaterialUtility {
        static public float GetCurrent(ResearchMaterial material, float voltage, float temperature) {
            // TODO: implement correctly
            float multiplier = (material.SpecialTags & SpecialTag.HighMobility) != 0 ? 1.5f : 1;
            switch(material.Electrical) {
                case ElectricalTag.Dopant: {
                    return 0;
                }
                case ElectricalTag.Insulator: {
                    return 0;
                }
                case ElectricalTag.Conductor: {
                    return multiplier * voltage * (0.2f + 0.8f * (1 - temperature));
                }
                case ElectricalTag.Semiconductor: {
                    return multiplier * voltage * (0.2f + 0.8f * temperature);
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
            if ((material.Thermal & ThermalTag.HighTemp) == 0 && temperature >= 0.75f) {
                return false;
            }
            if ((material.Thermal & ThermalTag.LowTemp) == 0 && temperature <= 0.25f) {
                return false;
            }
            return true;
        }

        static public bool IsStableAtVoltage(ResearchMaterial material, float voltage) {
            if (Math.Abs(voltage) >= 0.75f && (material.SpecialTags & SpecialTag.HighVoltage) == 0) {
                return false;
            }

            return true;
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
                case SpecialTag.HighVoltage: {
                    return "High Voltage";
                }
                case SpecialTag.Unknown: {
                    return "???";
                }
            }

            Assert.Fail("no tag label");
            return string.Empty;
        }
    }

    [Serializable]
    public struct ResearchMaterialPair {
        public ResearchMaterial Base;
        public ResearchMaterial Dopant;
    }
}