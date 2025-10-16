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
    [SysUpdate(GameLoopPhase.LateUpdate, 100)]
    public sealed class PortSelectorSystem : SharedStateSystemBehaviour<RouteDrawerState, RouteHoverState, RouteShipSelectionState> {
        public override void ProcessWork(float deltaTime) {
            LiveRouteData route = m_StateC.SelectedRoute;
            Port port = m_StateB.Port;

            if (port == null || route == null || m_StateA.DrawState != RouteDrawState.Selected) {
                return;
            }

            if (Game.Input.IsMousePressed(MouseButton.Left)) {
                RouteShip ship = Find.NamedAsset<RouteShip>(route.ShipId);
                if (port.Owner == route) {
                    LiveRouteUtility.RemovePort(route, port);
                } else if (port.Owner != null && port.Owner != route) {
                    // refuse
                } else if (port.Type == PortType.Supply && LiveRouteUtility.CountPortsOfType(route, PortType.Supply) >= ship.Capacity) {
                    // refuse
                } else {
                    LiveRouteUtility.TryAddPort(route, port);
                }
            }
        }
    }
}