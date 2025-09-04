using BeauPools;
using BeauUtil;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;

namespace SpaceFab.SupplyChain {
    public sealed class RouteDrawerState : SharedStateComponent {
        [Serializable] public sealed class PathNodePool : SerializablePool<PathNode> { }
        [Serializable] public sealed class RouteLinePool : SerializablePool<RouteLineRenderer> { }

        public PathNodePool TempPathNodePool;

        [NonSerialized] public RouteDrawState DrawState;
    }

    public enum RouteDrawState {
        NotStarted,
        Started,
        Selected
    }
}