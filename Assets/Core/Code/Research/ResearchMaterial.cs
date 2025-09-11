using System;
using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Research Material")]
    public sealed class ResearchMaterial : NamedAsset {
        public string DisplayName;
        public string ChemicalSymbol;
        public Material Material;

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
}