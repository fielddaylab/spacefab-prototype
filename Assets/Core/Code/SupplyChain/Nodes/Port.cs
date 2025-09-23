using System;
using FieldDay.Components;

namespace SpaceFab.SupplyChain {
    public sealed class Port : BatchedComponent {
        public PortType Type;
        public PortVisuals Visuals;

        [NonSerialized] public PathNode ParentNode;
        [NonSerialized] public LiveRouteData Owner;
    }

    public enum PortType : byte {
        Supply,
        Purchase,
        Conversion,
    }
}