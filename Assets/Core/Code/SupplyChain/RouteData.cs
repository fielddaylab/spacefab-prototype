using System;
using BeauUtil;
using FieldDay;

namespace SpaceFab.SupplyChain {
    public struct SupplyRouteData {
        public StringHash32 ShipId;
        public SupplyRouteStats Stats;
    }

    public struct SupplyRouteStats {
        public uint Cost;
        public byte Time;
        public byte Reliability;
        public unsafe fixed byte Materials[SupplyUtility.MaterialTypeCount];
    }

    static public partial class SupplyUtility {
        public const byte MaxReliability = 128;
        public const int MaxDefense = 5;
        public const int MaterialTypeCount = 5;

        static public bool EvaluateRoute(in SupplyRouteStats routeData, ref PseudoRandom pseudoRand) {
            return pseudoRand.Bool(routeData.Reliability / (float) MaxReliability);
        }

        static public unsafe void AccumulateMaterials(ref FabMaterialSet materials, in SupplyRouteStats stats) {
            materials.Insulator += stats.Materials[(int) FabMaterial.Insulator - 1];
            materials.Semiconductor += stats.Materials[(int) FabMaterial.Semiconductor - 1];
            materials.DopantN += stats.Materials[(int) FabMaterial.DopantN - 1];
            materials.DopantP += stats.Materials[(int) FabMaterial.DopantP - 1];
            materials.Conductor += stats.Materials[(int) FabMaterial.Conductor - 1];
        }
    }
}