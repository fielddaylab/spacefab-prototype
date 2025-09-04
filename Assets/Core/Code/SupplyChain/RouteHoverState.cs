using BeauPools;
using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;
using System.Collections.Generic;

namespace SpaceFab.SupplyChain {
    public sealed class RouteHoverState : SharedStateComponent {
        public PathNode Node;
        public bool Locked;
    }
}