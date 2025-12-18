using System;
using System.Text;
using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Research Material")]
    public sealed class ResearchMaterial : NamedAsset {
        public string DisplayName;
        public string UnknownDisplayName;
        public Material Material;

        [Header("Atomic Info")]
        public string ChemicalSymbol;
        public string SampleNumber;
        public AtomicStructure[] Atoms;

        [AssetName(typeof(ResearchMaterial))] public StringHash32 Parent;

        [Header("Properties")]
        public ElectricalTag Electrical;
        public ThermalTag Thermal;
        public SpecialTag SpecialTags;

        [Header("Fields")]
        public DopantType DopantType;
        [Range(0, 2)] public float ConductionMultiplier = 1;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 DopantN;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 DopantP;
    }

    public enum ElectricalTag : uint {
        Unknown = 0,
        Conductor,
        Insulator,
        Semiconductor
    }

    [Flags]
    public enum ThermalTag : uint {
        None = 0,
        HighTemp = 0x01,
        LowTemp = 0x02,
    }

    [Flags]
    public enum SpecialTag : uint {
        None = 0,
        HighMobility = 0x01,
        LightEmitting = 0x02,
        HighVoltage = 0x04
    }

    [Flags]
    public enum DopantType : uint {
        None = 0,
        N = 0x01,
        P = 0x02
    }

    [Serializable]
    public struct AtomicStructure {
        [Range(1, 200)] public byte Size;
        [Range(0, 8)] public byte ValenceElectrons;
        public AtomicAppearance Appearance;
        public Color32 Color;
        public string Symbol;
    }

    public enum AtomicAppearance : byte {
        Triangle,
        Square,
        Diamond,
        Pentagon,
        Hexagon,
        Circle
    }

    static public partial class ResearchMaterialUtility {
        static public float GetCurrent(ResearchMaterial material, float voltage, float temperature) {
            // TODO: implement correctly
            float multiplier = material.ConductionMultiplier * ((material.SpecialTags & SpecialTag.HighMobility) != 0 ? 1.5f : 1);

            switch(material.Electrical) {
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
                case ElectricalTag.Insulator:
                    return true;

                default:
                    return false;
            }
        }

        static public bool IsStableAtTemperature(ResearchMaterial material, float temperature) {
            if ((material.Thermal & ThermalTag.HighTemp) == 0 && temperature > 0.8f) {
                return false;
            }
            if ((material.Thermal & ThermalTag.LowTemp) == 0 && temperature < 0.2f) {
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
    }
}