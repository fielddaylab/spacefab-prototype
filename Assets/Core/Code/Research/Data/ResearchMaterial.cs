using System;
using System.Text;
using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Research Material")]
    public sealed class ResearchMaterial : NamedAsset, IRegistrationCallbacks {
        public string DisplayName;
        public string UnknownDisplayName;

        [Header("Atomic Info")]
        public string ChemicalSymbol;
        public string SampleNumber;
        public AtomicStructure[] Atoms;

        [Header("Properties")]
        [Range(0, 2)] public float ConductionMultiplier = 1;
        [Range(0, 2)] public float ThermalMultiplier = 1;
        [Range(0, 1)] public float MaxTemperature = 0.6f;
        [Range(0, 1)] public float MaxVoltage = 0.6f;
        public SpecialTag SpecialTags;

        [Header("Doping")]
        public DopantType DopantType;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 DopantN;
        [AssetName(typeof(ResearchMaterial))] public StringHash32 DopantP;

        [NonSerialized] public Color32 GemColor;
        [NonSerialized] public float GemScale;

        void IRegistrationCallbacks.OnDeregister() {
        }

        void IRegistrationCallbacks.OnRegister() {
            GemColor = ResearchMaterialUtility.CalculateMaterialColor(this);
            GemScale = ResearchMaterialUtility.CalculateMaterialScaleFactor(this);
        }
    }

    [Flags]
    public enum SpecialTag : uint {
        None = 0,
        LightEmitting = 0x01,
        HighMobility = 0x02
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
        public byte Count;
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
        static public float GetCurrent(ResearchMaterial material, float voltage, float temperature, DopantType dopingState) {
            float conductionMultiplier = material.ConductionMultiplier;
            if (dopingState != DopantType.None) {
                conductionMultiplier = 0.8f;
            }
            if ((material.SpecialTags & SpecialTag.HighMobility) != 0) {
                conductionMultiplier *= 1.5f;
            }

            float thermalMultiplier = 1 + temperature * (material.ThermalMultiplier - 1);
            return voltage * thermalMultiplier * conductionMultiplier;
        }

        static public bool BehavesAsInsulator(ResearchMaterial material) {
            return material.ConductionMultiplier <= 0.2f;
        }

        static public bool IsStableAtTemperature(ResearchMaterial material, float temperature) {
            return temperature <= material.MaxTemperature;
        }

        static public bool IsStableAtVoltage(ResearchMaterial material, float voltage) {
            return Math.Abs(voltage) <= material.MaxVoltage;
        }
    }
}