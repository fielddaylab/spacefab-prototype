using System;
using FieldDay.Components;

namespace SpaceFab.SupplyChain {
    public sealed class Port : BatchedComponent {
        public PortType Type;
        [NonSerialized] public PathNode ParentNode;

        private void Awake() {
            ParentNode = GetComponentInParent<PathNode>();
        }
    }

    public enum PortType : byte {
        Supply,
        Purchase,
        Conversion,
    }
}