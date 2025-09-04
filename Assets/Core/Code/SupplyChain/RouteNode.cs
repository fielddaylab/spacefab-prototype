using FieldDay.Components;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteNode : BatchedComponent {
        public uint Cost;
        public uint Reliability;
        public uint ProductionTime;
    }
}