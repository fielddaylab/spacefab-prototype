using FieldDay.Components;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteNode : BatchedComponent {
        public uint Cost;
        [Range(0, 3)] public uint Reliability;
        public uint ProductionTime;
    }
}