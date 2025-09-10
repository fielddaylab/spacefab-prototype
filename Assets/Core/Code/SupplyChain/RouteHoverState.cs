using BeauPools;
using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteHoverState : SharedStateComponent {
        public PathNode Node;
        public RouteLineRenderer RouteLine;
        public bool Locked;

        public Vector3? MousePosition;
    }
}