using FieldDay.Components;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteNode : BatchedComponent {
        public string DisplayName;
        public uint Cost;
        [Range(0, 3)] public uint Reliability;
        public uint ProductionTime;
    }
}