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
    public sealed class RouteDrawerSystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork, new SysUpdate(GameLoopPhase.LateUpdate, 99),
                new SysPermissions().ReadShared<RouteHoverState>()
                    .ReadWriteShared<RouteShipSelectionState>()
                    .ReadWriteShared<RouteDrawerState>()
                    .ReadWriteShared<LiveRoutesState>());
        }

        static private void ProcessWork(float deltaTime) {
            Find.State(out RouteDrawerState m_StateA, out RouteHoverState m_StateB, out RouteShipSelectionState m_StateC);

            LiveRouteData route = m_StateC.SelectedRoute;

            if (route == null) {
                if (!Game.Input.IsMousePressed(MouseButton.Left)) {
                    return;
                }

                if (m_StateB.Node) {
                    if ((m_StateB.Node.Flags & PathNodeFlags.IsDestination) == 0) {
                        LiveRoutesState routes = Find.State<LiveRoutesState>();
                        for(int i = 0; i < routes.RouteCount; i++) {
                            if (LiveRouteUtility.IsNodeInPath(routes.Routes[i], m_StateB.Node)) {
                                RouteShipUtility.SelectShipFromId(routes.Routes[i].ShipId);
                                break;
                            }
                        }
                    }
                } else if (m_StateB.RouteLine) {
                    RouteShipUtility.SelectShipFromId(m_StateB.RouteLine.ShipId);
                }
                return;
            }

            switch(m_StateA.DrawState) {
                case RouteDrawState.Started:
                case RouteDrawState.InProgress: {
                    if (m_StateB.MousePosition.HasValue) {
                        LiveRouteLineUtility.ShowDottedLine(route.Line, m_StateB.MousePosition.Value);
                    } else {
                        LiveRouteLineUtility.HideDottedLine(route.Line);
                    }

                    if (Game.Input.IsMousePressed(MouseButton.Left)) {
                        PathNode nodeToAdd = m_StateB.Node;
                        if (!nodeToAdd && m_StateA.AllowTemporaryNodes && m_StateB.MousePosition.HasValue) {
                            nodeToAdd = m_StateA.TempPathNodePool.Alloc();
                            nodeToAdd.transform.localPosition = m_StateB.MousePosition.Value;
                            nodeToAdd.transform.localEulerAngles = new Vector3(0, 0, RNG.Instance.NextFloat(-180, 180));
                            nodeToAdd.gameObject.SetActive(true);
                        }

                        Vector3 clickPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                        RaycastHit2D hit = Physics2D.Raycast(clickPos, Vector2.zero);

                        if (nodeToAdd) {
                            if ((nodeToAdd.Flags & PathNodeFlags.IsDestination) != 0) {
                                RouteShipUtility.AttemptFinishRoute();
                            } else if (LiveRouteUtility.TryRemoveNode(route, nodeToAdd)) {
                                // remove feedback
                            } else if (LiveRouteUtility.TryReconnectRoute(route, nodeToAdd)) {
                                // reconnect feedback
                            } else {
                                if (!LiveRouteUtility.TryAddNode(route, nodeToAdd)) {
                                    Pool.TryFree(nodeToAdd);
                                }
                            }
                        } else if (hit.collider != null) {
                                int tempNodeIndex = LiveRouteLineUtility.FindClosestSegment(route.Line, clickPos);

                                if (tempNodeIndex != -1) {
                                    LiveRouteUtility.SplitRoute(route, tempNodeIndex);
                                }
                        } else if (!Game.Input.IsPointerOverCanvas()) {
                            RouteShipUtility.AttemptFinishRoute();
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
                                RouteShipUtility.SelectShipFromWidget(null);
                            }
                        }
                    } else if (Game.Input.IsKeyPressed(KeyCode.Space)) {
                        RouteShipUtility.AttemptFinishRoute();
                    }
                    break;
                }
            }
        }
    }
}