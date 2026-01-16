using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Components;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceFab.SupplyChain {
    public sealed class RouteShipPanel : SharedPanel, IScenePreload {
        public RouteShipWidget[] ShipWidgets;
        public Vector2 ShipWidgetSelectedOffset;
        
        [NonSerialized] public int ShipCount;

        public void PopulateShips(StringHash32[] ships) {
            ShipCount = ships.Length;
            for(int i = 0; i < ShipCount; i++) {
                RouteShipUtility.PopulateWidget(ShipWidgets[i], Find.NamedAsset<RouteShip>(ships[i]));
                ShipWidgets[i].gameObject.SetActive(true);
            }
            for (int i = ShipCount; i < ShipWidgets.Length; i++) {
                ShipWidgets[i].gameObject.SetActive(false);
            }
        }

        public void UpdateShipSelectionVisuals(RouteShipWidget widget, bool selected) {
            widget.Positioner.Offset0 = selected ? ShipWidgetSelectedOffset : default;
        }

        protected override void OnDestroy() {
            Game.Events?.DeregisterAllForContext(this);
            base.OnDestroy();
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            foreach(var ship in ShipWidgets) {
                ship.CursorHint.onClick.Register(OnShipClicked);
            }
            SpaceFabGame.Events.Register<StringHash32>(SupplyChainGame.Events.RouteStatsUpdated, OnRouteStatsUpdated);
            return null;
        }

        private void OnRouteStatsUpdated(StringHash32 shipId) {
            LiveRoutesState routesState = Find.State<LiveRoutesState>();

            int highestTime = 0;
            int lowestReliability = SupplyUtility.MaxReliability + 1;
            int routeCount = 0;

            for (int i = 0; i < routesState.RouteCount; i++) {
                LiveRouteData liveRoute = routesState.Routes[i];
                if (liveRoute.Stats.Time <= 0) {
                    continue;
                }

                highestTime = Math.Max(liveRoute.Stats.Time, highestTime);
                lowestReliability = Math.Min(liveRoute.Stats.Reliability, lowestReliability);
                routeCount++;
            }

            for (int i = 0; i < ShipCount; i++) {
                RouteShipWidget widget = ShipWidgets[i];
                LiveRouteData routeData = LiveRouteUtility.GetLiveRoute(widget.ShipId);
                if (widget.ShipId == shipId) {
                    RouteShipUtility.PopulateWidgetRouteStats(widget, routeData.Stats);
                }
                RouteShipUtility.PopulateWidgetBottleneckAlerts(widget, routeCount > 1 && routeData.Stats.Time == highestTime, routeCount > 1 && routeData.Stats.Reliability == lowestReliability);
            }
        }

        private void OnShipClicked(PointerListener.EventData evt) {
            var widget = evt.Source.GetComponentInParent<RouteShipWidget>();
            RouteShipUtility.SelectShipFromWidget(widget);
        }
    }
}