using BeauPools;
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
        public float LineWidth = 0.25f;

        private void Awake() {
            Solid.widthMultiplier = LineWidth;
            Drawing.widthMultiplier = LineWidth;
            Tail.widthMultiplier = LineWidth * 1.25f;
        }
    }

    static public partial class LiveRouteLineUtility {
        static public void UpdateColor(RouteLineRenderer line, Color mainColor) {
            line.Solid.startColor = line.Solid.endColor = mainColor;
            line.Drawing.startColor = line.Drawing.endColor = mainColor;
            line.Tail.startColor = line.Tail.endColor = mainColor.WithAlpha(mainColor.a * 0.5f);
        }

        static public void RegenerateColliders(RouteLineRenderer line) {
            if (line.Solid.positionCount < 2) {
                line.Collider.enabled = false;
            } else {
                using (PooledList<Vector2> points = PooledList<Vector2>.Create()) {
                    for (int i = 0; i < line.Solid.positionCount; i++) {
                        points.Add(line.Solid.GetPosition(i));
                    }
                    if (line.Tail.enabled && line.Tail.positionCount > 1) {
                        points.Add(line.Tail.GetPosition(1));
                    }

                    line.Collider.SetPoints(points);
                    line.Collider.enabled = true;
                }
            }
        }


        #region Solid Line

        static public unsafe void PopSolid(RouteLineRenderer line) {
            LineRenderer renderer = line.Solid;
            int pointCount = renderer.positionCount;
            Assert.True(pointCount > 0);
            renderer.positionCount = pointCount - 1;
            renderer.enabled = renderer.positionCount > 1;
        }

        static public unsafe void AddSolid(RouteLineRenderer line, Vector3 position) {
            LineRenderer renderer = line.Solid;
            renderer.SetPosition(renderer.positionCount++, position);
            renderer.enabled = renderer.positionCount > 1;
        }

        #endregion // Solid Line

        #region Dotted Line

        static public void ShowDottedLine(RouteLineRenderer line, Vector3 position) {
            Assert.True(line.Solid.positionCount > 0);
            line.Drawing.enabled = true;
            line.Drawing.SetPosition(0, line.Solid.GetPosition(line.Solid.positionCount - 1));
            line.Drawing.SetPosition(1, position);
        }

        static public void HideDottedLine(RouteLineRenderer line) {
            line.Drawing.enabled = false;
        }

        #endregion // Dotted Line

        #region Tail

        static public void UpdateTail(RouteLineRenderer line) {
            if (line.Solid.positionCount > 1) {
                line.Tail.SetPosition(0, line.Solid.GetPosition(line.Solid.positionCount - 1));
                line.Tail.SetPosition(1, line.Solid.GetPosition(0));
                line.Tail.enabled = true;
            } else {
                line.Tail.enabled = false;
            }
        }

        #endregion // Tail
    }
}