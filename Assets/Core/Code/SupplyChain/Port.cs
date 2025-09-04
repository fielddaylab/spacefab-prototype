using FieldDay.Components;

namespace SpaceFab.SupplyChain {
    public sealed class Port : BatchedComponent {
        public PortType Type;
    }

    public enum PortType : byte {
        Supply,
        Purchase,
        Conversion,
    }
}