using FieldDay.Components;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [RequireComponent(typeof(Port), typeof(RouteNode))]
    public sealed class SupplyNode : BatchedComponent {
        public FabMaterial Material;
    }
}