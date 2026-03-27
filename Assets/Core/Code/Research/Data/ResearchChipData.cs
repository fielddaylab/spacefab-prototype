using BeauUtil;
using Unity.IL2CPP.CompilerServices;

namespace SpaceFab.Research {
    /// <summary>
    /// Chip ids
    /// </summary>
    public enum ResearchChipId : byte {
        None = 0,

        CurrentFlows,
        CurrentBlocked,

        HeatIncreases,
        HeatDecreases,
        HeatDoesNotAffect,

        ExtremeHeatExplode,
        ExtremeHeatResistant,

        AtomicRadiusLess,
        AtomicRadiusGreater,

        ValenceOneLess,
        ValenceOneMore,

        DiodeWithNType,
        DiodeWithPType,

        ConductivityIncrease,
        NoConductivityIncrease,

        LightEmitting,
        ElectronMobility,
        VoltageResistant,

        ConductorNaive,
        InsulatorNaive,
        Insulator,
        Conductor,
        Semiconductor,
        HiTempConductor,
        HiTempSemiconductor,
        PTypeDopant,
        PTypeDopant_Alt,
        NTypeDopant,
        NTypeDopant_Alt,
        LightEmittingSemiconductor,
        HighVoltageSemiconductor,
        HighMobilitySemiconductor,
    }

    /// <summary>
    /// Research chip category
    /// </summary>
    public enum ResearchChipCategory : byte {
        None,
        BaseConductivity,
        ThermalConductivity,
        ThermalResistance,
        Radius,
        Valence,
        Diode,
        DopingConductivity,
        SpecialLight,
        SpecialMobility,
        SpecialVoltage,
        PropertyElectricNaive,
        PropertyElectric,
        PropertyThermal,
        PropertyDopantN,
        PropertyDopantP,
        PropertySpecial,
    }

    public struct ResearchChipMetadata {
        public ResearchChipCategory Category;
        public ResearchChipId AliasFor;
        public ResearchChipId DependencyA;
        public ResearchChipId DependencyB;

        public string Label;
    }

    [Il2CppEagerStaticClassConstruction]
    static public class ResearchChipUtility {
        static private readonly ResearchChipMetadata[] MetadataTable = new ResearchChipMetadata[] {
            default,

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.BaseConductivity,
                Label = "Allows current flow"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.BaseConductivity,
                Label = "Blocks current flow"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.ThermalConductivity,
                Label = "Heat increases current"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.ThermalConductivity,
                Label = "Heat decreases current"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.ThermalConductivity,
                Label = "Heat does not affect current"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.ThermalResistance,
                Label = "Explodes under extreme heat"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.ThermalResistance,
                Label = "Withstands extreme heat"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.Radius,
                Label = "Atomic radius less than {0}"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.Radius,
                Label = "Atomic radius greater than {0}"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.Valence,
                Label = "1 less valence electron than {0}"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.Valence,
                Label = "1 more valence electron than {0}"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.Diode,
                Label = "In {0}, forms a diode with a known N-type"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.Diode,
                Label = "In {0}, forms a diode with a known P-type"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.DopingConductivity,
                Label = "Increases the conductivity of {0}"
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.DopingConductivity,
                Label = "Does not increase the conductivity of {0}"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.SpecialLight,
                Label = "Diodes emit light"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.SpecialMobility,
                Label = "Current is extremely strong"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.SpecialVoltage,
                Label = "Withstands extreme voltage"
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyElectricNaive,
                Label = "Conductor",

                DependencyA = ResearchChipId.CurrentFlows
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyElectricNaive,
                Label = "Insulator",

                DependencyA = ResearchChipId.CurrentBlocked
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyElectric,
                Label = "Insulator",

                DependencyA = ResearchChipId.CurrentBlocked,
                DependencyB = ResearchChipId.HeatDoesNotAffect,
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyElectric,
                Label = "Conductor",

                DependencyA = ResearchChipId.CurrentFlows,
                DependencyB = ResearchChipId.HeatDecreases,
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyElectric,
                Label = "Semiconductor",

                DependencyA = ResearchChipId.CurrentFlows,
                DependencyB = ResearchChipId.HeatIncreases,
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyThermal,
                Label = "Hi-Temp Conductor",

                DependencyA = ResearchChipId.Conductor,
                DependencyB = ResearchChipId.ExtremeHeatResistant,
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyThermal,
                Label = "Hi-Temp Semiconductor",

                DependencyA = ResearchChipId.Semiconductor,
                DependencyB = ResearchChipId.ExtremeHeatResistant,
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyDopantP,
                Label = "P-Type Dopant for {0}",

                DependencyA = ResearchChipId.AtomicRadiusLess,
                DependencyB = ResearchChipId.ValenceOneLess,
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyDopantP,
                AliasFor = ResearchChipId.PTypeDopant,
                Label = "P-Type Dopant for {0}",

                DependencyA = ResearchChipId.ConductivityIncrease,
                DependencyB = ResearchChipId.DiodeWithNType,
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyDopantN,
                Label = "N-Type Dopant for {0}",

                DependencyA = ResearchChipId.AtomicRadiusLess,
                DependencyB = ResearchChipId.ValenceOneMore,
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.PropertyDopantN,
                AliasFor = ResearchChipId.NTypeDopant,
                Label = "P-Type Dopant for {0}",

                DependencyA = ResearchChipId.ConductivityIncrease,
                DependencyB = ResearchChipId.DiodeWithPType,
            },

            new ResearchChipMetadata() {
                Category = ResearchChipCategory.SpecialLight,
                Label = "Light-Emitting Semiconductor",

                DependencyA = ResearchChipId.Semiconductor,
                DependencyB = ResearchChipId.LightEmitting,
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.SpecialVoltage,
                Label = "High Voltage Semiconductor",

                DependencyA = ResearchChipId.Semiconductor,
                DependencyB = ResearchChipId.VoltageResistant,
            },
            new ResearchChipMetadata() {
                Category = ResearchChipCategory.SpecialMobility,
                Label = "High Mobility Semiconductor",

                DependencyA = ResearchChipId.Semiconductor,
                DependencyB = ResearchChipId.HighMobilitySemiconductor,
            },
        };

        static private readonly StringHash32[] CategoryStyleTable = new StringHash32[] {
            default,
            "ObsStyleConductivity",
            "ObsStyleThermalConductivity",
            "ObsStyleThermalResistance",
            "ObsStyleDopantRadius",
            "ObsStyleDopantValence",
            "ObsStyleDopantDiode",
            "ObsStyleDopantConductivity",
            "ObsStyleSpecial",
            "ObsStyleSpecial",
            "ObsStyleSpecial",
            "PropStyleElectric",
            "PropStyleElectric",
            "PropStyleThermal",
            "PropStyleN",
            "PropStyleP",
            "PropStyleSpecial"
        };

        static private readonly BitSet32 CategoryRequiresContextTable = new BitSet32(
            (1 << (int) ResearchChipCategory.Radius)
            | (1 << (int) ResearchChipCategory.Valence)
            | (1 << (int) ResearchChipCategory.Diode)
            | (1 << (int) ResearchChipCategory.DopingConductivity)
            | (1 << (int) ResearchChipCategory.PropertyDopantN)
            | (1 << (int) ResearchChipCategory.PropertyDopantP)
        );

        static private readonly BitSet32 CategoryAllowsMultipleTable = new BitSet32(
            (1 << (int) ResearchChipCategory.PropertySpecial)
        );

        static public ResearchChipMetadata Metadata(ResearchChipId chip) {
            return MetadataTable[(int)chip];
        }

        static public ResearchChipCategory Category(ResearchChipId chip) {
            return MetadataTable[(int)chip].Category;
        }

        static public StringHash32 CategoryStyleId(ResearchChipCategory category) {
            return CategoryStyleTable[(int)category];
        }

        static public bool CategoryRequiresContext(ResearchChipCategory category) {
            return CategoryRequiresContextTable.IsSet((int)category);
        }

        static public bool CategoryIsExclusive(ResearchChipCategory category) {
            return !CategoryAllowsMultipleTable.IsSet((int)category);
        }
    }
}