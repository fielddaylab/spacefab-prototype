using FieldDay.Components;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [RequireComponent(typeof(Port), typeof(RouteNode))]
    public sealed class ConversionNode : BatchedComponent {
        public FabMaterial Input;
        public FabMaterial Output;
    }
}