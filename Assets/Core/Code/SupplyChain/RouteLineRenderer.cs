using BeauUtil;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteLineRenderer : BatchedComponent {
        public LineRenderer Solid;
        public LineRenderer Drawing;
        public LineRenderer Tail;
    }
}