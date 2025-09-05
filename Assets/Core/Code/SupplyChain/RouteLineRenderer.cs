using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteLineRenderer : BatchedComponent {
        public LineRenderer Solid;
        public LineRenderer Drawing;
        public LineRenderer Tail;
        public EdgeCollider2D Collider;
    }

    static public partial class LiveRouteLineUtility {
        static public unsafe void PopSolid(RouteLineRenderer line) {
            LineRenderer renderer = line.Solid;
            int pointCount = renderer.positionCount;
            Assert.True(pointCount > 1);
            renderer.positionCount = pointCount - 1;
        }

        static public unsafe void AddSolid(RouteLineRenderer line, Vector3 position) {
            LineRenderer renderer = line.Solid;
            renderer.SetPosition(renderer.positionCount++, position);
        }
    }
}