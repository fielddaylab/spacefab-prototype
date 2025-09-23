using BeauPools;
using BeauUtil;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;

namespace SpaceFab.SupplyChain {
    public sealed class PortPools : SharedStateComponent {
        [Serializable] public sealed class DisplayPool : SerializablePool<PortDetailsDisplay> { }

        public DisplayPool SupplyPort;
        public DisplayPool ConversionPort;
    }
}