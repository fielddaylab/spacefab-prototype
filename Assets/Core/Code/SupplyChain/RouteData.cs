using System;
using BeauUtil;
using FieldDay;

namespace SpaceFab.SupplyChain {
    public struct SupplyRouteData {
        public StringHash32 ShipId;
        public SupplyRouteStats Stats;
    }

    public struct SupplyRouteStats {
        public ushort Cost;
        public byte Time;
        public byte Reliability;
        public unsafe fixed byte Materials[4];
    }

    static public partial class SupplyUtility {
        public const byte MaxReliability = 128;

        static public bool EvaluateRoute(in SupplyRouteStats routeData, ref PseudoRandom pseudoRand) {
            return pseudoRand.Bool(routeData.Reliability / (float) MaxReliability);
        }
    }
}