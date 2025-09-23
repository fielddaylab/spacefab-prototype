using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Debugging;
using FieldDay.HID;
using FieldDay.Physics;
using FieldDay.SharedState;
using FieldDay.Systems;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [SysUpdate(GameLoopPhase.LateUpdate, 99)]
    public sealed class RouteDrawerSystem : SharedStateSystemBehaviour<RouteDrawerState, RouteHoverState, RouteShipSelectionState> {
        public override void ProcessWork(float deltaTime) {
            LiveRouteData route = m_StateC.SelectedRoute;

            if (route == null) {
                return;
            }

            switch(m_StateA.DrawState) {
                case RouteDrawState.NotStarted: {
                    if (m_StateB.Node && Game.Input.IsMousePressed(MouseButton.Left)) {
                        if ((m_StateB.Node.Flags & PathNodeFlags.IsDestination) != 0) {
                            bool added = LiveRouteUtility.TryAddNode(route, m_StateB.Node);
                            Assert.True(added);
                            m_StateA.DrawState = RouteDrawState.Started;
                            CursorHint.DefaultCursor = "DrawCursor";
                        }
                    }
                    break;
                }
                case RouteDrawState.Selected: {
                    if (m_StateB.Node && Game.Input.IsMousePressed(MouseButton.Left)) {
                        if (LiveRouteUtility.IsNodeInPath(route, m_StateB.Node)) {
                            m_StateA.DrawState = RouteDrawState.InProgress;
                            CursorHint.DefaultCursor = "DrawCursor";
                        }
                    } else if (Game.Input.IsKeyPressed(KeyCode.E)) {
                        m_StateA.DrawState = RouteDrawState.InProgress;
                        CursorHint.DefaultCursor = "DrawCursor";
                    }
                    break;
                }
                case RouteDrawState.Started:
                case RouteDrawState.InProgress: {
                    if (m_StateB.MousePosition.HasValue) {
                        LiveRouteLineUtility.ShowDottedLine(route.Line, m_StateB.MousePosition.Value);
                    } else {
                        LiveRouteLineUtility.HideDottedLine(route.Line);
                    }

                    if (Game.Input.IsMousePressed(MouseButton.Left)) {
                        PathNode nodeToAdd = m_StateB.Node;
                        if (!nodeToAdd && m_StateB.MousePosition.HasValue) {
                            nodeToAdd = m_StateA.TempPathNodePool.Alloc();
                            nodeToAdd.transform.localPosition = m_StateB.MousePosition.Value;
                            nodeToAdd.transform.localEulerAngles = new Vector3(0, 0, RNG.Instance.NextFloat(-180, 180));
                            nodeToAdd.gameObject.SetActive(true);
                        }

                        if (nodeToAdd) {
                            if ((nodeToAdd.Flags & PathNodeFlags.IsDestination) != 0) {
                                RouteShipUtility.AttemptFinishRoute();
                            } else {
                                if (!LiveRouteUtility.TryAddNode(route, nodeToAdd)) {
                                    Pool.TryFree(nodeToAdd);
                                }
                            }
                        }
                    } else if (Game.Input.IsMousePressed(MouseButton.Right) || Game.Input.IsKeyPressed(KeyCode.Backspace)) {
                        if (route.NodeCount > 0) {
                            PathNode node = LiveRouteUtility.PopNode(route);
                            Pool.TryFree(node);

                            if (route.NodeCount == 1) {
                                m_StateA.DrawState = RouteDrawState.Started;
                                CursorHint.DefaultCursor = "DrawCursor";
                            } else if (route.NodeCount == 0) {
                                m_StateA.DrawState = RouteDrawState.NotStarted;
                                CursorHint.DefaultCursor = null;
                                LiveRouteLineUtility.HideDottedLine(route.Line);
                            }
                        }
                    } else if (Game.Input.IsKeyPressed(KeyCode.E)) {
                        RouteShipUtility.AttemptFinishRoute();
                    }
                    break;
                }
            }

            if (Game.IsDevBuild) {
                SupplyRouteStats stats = route.Stats;
                using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    psb.Builder.Append("Route ").Append(route.ShipId.ToDebugString())
                        .Append(": $").AppendNoAlloc(stats.Cost)
                        .Append(", ").AppendNoAlloc(stats.Time).Append(" cycles")
                        .Append(", ").AppendNoAlloc((int) (100f * stats.Reliability / SupplyUtility.MaxReliability)).Append("%");
                    DebugDraw.AddViewportText(new Vector2(0.5f, 0), new Vector2(0, 16), psb.Builder, Color.yellow, 0, TextAnchor.LowerCenter, DebugTextStyle.BackgroundDark);
                }
            }
        }
    }
}