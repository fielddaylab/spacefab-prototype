using System;

namespace SpaceFab.SupplyChain {
    public enum FabMaterial : byte {
        None = 0,

        Insulator,
        Semiconductor,
        DopantN,
        Conductor,
        DopantP,

        Any = 255
    }

    [Serializable]
    public struct FabMaterialSet {
        public int Insulator;
        public int Semiconductor;
        public int DopantN;
        public int DopantP;
        public int Conductor;
    }
}