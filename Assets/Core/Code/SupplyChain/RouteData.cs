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
        public const byte MaxReliability = 200;
        public const int MaxDefense = 5;
        public const int MaterialTypeCount = 5;

        static public bool EvaluateRoute(in SupplyRouteStats routeData, ref PseudoRandom pseudoRand) {
            return pseudoRand.Bool(routeData.Reliability / (float) MaxReliability);
        }

        static public unsafe void AccumulateMaterials(ref FabMaterialSet materials, in SupplyRouteStats stats) {
            materials.A += stats.Materials[(int) FabMaterial.A - 1];
            materials.B += stats.Materials[(int) FabMaterial.B - 1];
            materials.C += stats.Materials[(int) FabMaterial.C - 1];
            materials.D += stats.Materials[(int) FabMaterial.E - 1];
            materials.E += stats.Materials[(int) FabMaterial.D - 1];
        }
    }
}