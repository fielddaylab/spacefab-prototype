using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.HID;
using FieldDay.Physics;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [SysUpdate(GameLoopPhase.LateUpdate, 99)]
    public sealed class RouteDrawerSystem : SharedStateSystemBehaviour<RouteDrawerState, RouteHoverState, RouteShipSelectionState> {
        public override void ProcessWork(float deltaTime) {
            if (m_StateC.SelectedRoute == null) {
                return;
            }

            switch(m_StateA.DrawState) {
                case RouteDrawState.NotStarted: {
                    if (m_StateB.Node && Game.Input.IsMousePressed(MouseButton.Left)) {
                        if ((m_StateB.Node.Flags & PathNodeFlags.IsDestination) != 0) {
                            bool added = LiveRouteUtility.TryAddNode(m_StateC.SelectedRoute, m_StateB.Node);
                            Assert.True(added);
                            m_StateA.DrawState = RouteDrawState.Started;
                        }
                    }
                    break;
                }
                case RouteDrawState.Started:
                case RouteDrawState.InProgress: {
                    if (m_StateB.MousePosition.HasValue) {
                        LiveRouteLineUtility.ShowDottedLine(m_StateC.SelectedRoute.Line, m_StateB.MousePosition.Value);
                    } else {
                        LiveRouteLineUtility.HideDottedLine(m_StateC.SelectedRoute.Line);
                    }

                    if (Game.Input.IsMousePressed(MouseButton.Left)) {
                        PathNode nodeToAdd = m_StateB.Node;
                        if (!nodeToAdd && m_StateB.MousePosition.HasValue) {
                            nodeToAdd = m_StateA.TempPathNodePool.Alloc();
                            nodeToAdd.transform.localPosition = m_StateB.MousePosition.Value;
                            nodeToAdd.gameObject.SetActive(true);
                        }

                        if (nodeToAdd) {
                            LiveRouteUtility.TryAddNode(m_StateC.SelectedRoute, nodeToAdd);
                        }
                    } else if (Game.Input.IsMousePressed(MouseButton.Right)) {
                        if (m_StateC.SelectedRoute.NodeCount > 0) {
                            PathNode node = LiveRouteUtility.PopNode(m_StateC.SelectedRoute);
                            Pool.TryFree(node);

                            if (m_StateC.SelectedRoute.NodeCount == 1) {
                                m_StateA.DrawState = RouteDrawState.Started;
                            } else if (m_StateC.SelectedRoute.NodeCount == 0) {
                                m_StateA.DrawState = RouteDrawState.NotStarted;
                                LiveRouteLineUtility.HideDottedLine(m_StateC.SelectedRoute.Line);
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}