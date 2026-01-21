using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Components;
using FieldDay.SharedState;
using FieldDay.UI;
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
            RouteDrawerState drawer = Find.State<RouteDrawerState>();
            RouteDrawerUI drawInterface = Find.Panel<RouteDrawerUI>();

            if (state.SelectedWidget != shipWidget) {
                if (state.SelectedWidget) {
                    panel.UpdateShipSelectionVisuals(state.SelectedWidget, false);
                    DeselectCurrentRoute();
                }
                state.SelectedWidget = shipWidget;
                if (state.SelectedWidget) {
                    state.SelectedShip = shipWidget.ShipAsset;
                    state.SelectedRoute = LiveRouteUtility.GetLiveRoute(state.SelectedShip.AssetId);
                    state.SelectedRoute.LineColor = shipWidget.RouteColor;
                    state.SelectedRoute.CarryingCapacity = state.SelectedShip.Capacity;
                    LiveRouteLineUtility.UpdateColor(state.SelectedRoute.Line, shipWidget.RouteColor);
                    panel.UpdateShipSelectionVisuals(state.SelectedWidget, true);
                    SelectCurrentRoute();
                    drawInterface.Background.color = shipWidget.RouteColor;
                    drawInterface.Show();
                } else {
                    state.SelectedShip = null;
                    state.SelectedRoute = null;
                    drawInterface.Hide();
                }
            }
        }

        static private void SelectCurrentRoute() {
            RouteShipSelectionState state = Find.State<RouteShipSelectionState>();
            RouteDrawerState drawer = Find.State<RouteDrawerState>();

            LiveRouteData route = state.SelectedRoute;
            Assert.NotNull(route);

            drawer.DrawState = RouteDrawState.Started;
            CursorHint.DefaultCursor = "DrawCursor";

            for (int i = 0; i < route.NodeCount; i++) {
                LiveRouteUtility.SetNodeOwner(route.Nodes[i], route);
            }

            for(int i = 0; i < route.PortCount; i++) {
                
            }

            for(int i = 0; i < route.HazardCount; i++) {
                LiveRouteUtility.SetHazardOwner(route.IntersectingHazards[i], route);
            }

            if (route.NodeCount == 0) {
                LiveRouteUtility.TryAddNode(route, drawer.StartingNode);
            }
        }

        static private void DeselectCurrentRoute() {
            RouteShipSelectionState state = Find.State<RouteShipSelectionState>();
            RouteDrawerState drawer = Find.State<RouteDrawerState>();

            LiveRouteData route = state.SelectedRoute;
            Assert.NotNull(route);

            LiveRouteLineUtility.HideDottedLine(route.Line);

            bool hasNonTempNode = false;
            for(int i = 1; i < route.NodeCount; i++) {
                if ((route.Nodes[i].Flags & PathNodeFlags.IsTemporary) == 0) {
                    hasNonTempNode = true;
                    break;
                }
            }

            if (!hasNonTempNode) {
                while (route.NodeCount > 1 && (route.Nodes[route.NodeCount - 1].Flags & PathNodeFlags.IsTemporary) != 0) {
                    PathNode tempNode = LiveRouteUtility.PopNode(route);
                    drawer.TempPathNodePool.Free(tempNode);
                }
            }

            if (route.NodeCount == 1) {
                LiveRouteUtility.PopNode(route);
            }

            for(int i = 0; i < route.NodeCount; i++) {
                LiveRouteUtility.SetNodeOwner(route.Nodes[i], null);
            }

            for (int i = 0; i < route.HazardCount; i++) {
                LiveRouteUtility.SetHazardOwner(route.IntersectingHazards[i], null);
            }

            LiveRouteLineUtility.UpdateColor(route.Line, ((Color) route.LineColor).WithAlpha(0.4f));

            drawer.DrawState = RouteDrawState.NotStarted;
            CursorHint.DefaultCursor = null;
        }

        static public void AttemptFinishRoute() {
            RouteShipSelectionState state = Find.State<RouteShipSelectionState>();
            RouteDrawerState drawer = Find.State<RouteDrawerState>();

            LiveRouteData route = state.SelectedRoute;
            Assert.NotNull(route);

            LiveRouteLineUtility.HideDottedLine(route.Line);

            bool hasNonTempNode = false;
            for (int i = 1; i < route.NodeCount; i++) {
                if ((route.Nodes[i].Flags & PathNodeFlags.IsTemporary) == 0) {
                    hasNonTempNode = true;
                    break;
                }
            }

            if (!hasNonTempNode) {
                while (route.NodeCount > 1 && (route.Nodes[route.NodeCount - 1].Flags & PathNodeFlags.IsTemporary) != 0) {
                    PathNode tempNode = LiveRouteUtility.PopNode(route);
                    drawer.TempPathNodePool.Free(tempNode);
                }
            }

            if (route.NodeCount == 1) {
                LiveRouteUtility.PopNode(route);
            }

            SelectShipFromWidget(null);
        }
    }
}