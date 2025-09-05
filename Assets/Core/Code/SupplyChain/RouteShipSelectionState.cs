using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Components;
using FieldDay.SharedState;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteShipSelectionState : SharedStateComponent {
        [NonSerialized] public RouteShipWidget SelectedWidget;
        [NonSerialized] public RouteShip SelectedShip;
        [NonSerialized] public LiveRouteData SelectedRoute;
    }

    static public partial class RouteShipUtility {
        static public void SelectShipFromWidget(RouteShipWidget shipWidget) {
            RouteShipSelectionState state = Find.State<RouteShipSelectionState>();
            RouteShipPanel panel = Find.Panel<RouteShipPanel>();
            LiveRoutesState routes = Find.State<LiveRoutesState>();

            if (state.SelectedWidget != shipWidget) {
                if (state.SelectedWidget != null) {
                    panel.UpdateShipSelectionVisuals(state.SelectedWidget, false);
                }
                state.SelectedWidget = shipWidget;
                if (state.SelectedWidget) {
                    state.SelectedShip = shipWidget.ShipAsset;
                    panel.UpdateShipSelectionVisuals(state.SelectedWidget, true);
                    state.SelectedRoute = LiveRouteUtility.GetLiveRoute(state.SelectedShip.AssetId);
                    state.SelectedRoute.LineColor = shipWidget.RouteColor;
                } else {
                    state.SelectedShip = null;
                    state.SelectedRoute = null;
                }
            }
        }
    }
}