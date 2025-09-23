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
        [NonSerialized] public PathNode Node;
        [NonSerialized] public RouteLineRenderer RouteLine;
        [NonSerialized] public Port Port;
        [NonSerialized] public bool Locked;

        [NonSerialized] public Vector3? MousePosition;
    }
}