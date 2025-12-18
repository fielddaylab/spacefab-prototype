using System;

namespace SpaceFab.SupplyChain {
    public enum FabMaterial : byte {
        None = 0,

        A,
        B,
        C,
        D,
        E,

        Any = 255
    }

    [Serializable]
    public struct FabMaterialSet {
        public int A;
        public int B;
        public int C;
        public int D;
        public int E;
    }
}