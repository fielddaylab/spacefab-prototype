using BeauPools;
using BeauUtil;
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
                        if (LiveRouteUtility.TryAddNode(m_StateC.SelectedRoute, m_StateB.Node)) {
                            m_StateA.DrawState = RouteDrawState.Started;
                        } else {
                            m_StateA.DrawState = RouteDrawState.Selected;
                        }
                    }
                    break;
                }
                case RouteDrawState.Started: {
                    // TODO: end
                    break;
                }
            }
        }
    }
}