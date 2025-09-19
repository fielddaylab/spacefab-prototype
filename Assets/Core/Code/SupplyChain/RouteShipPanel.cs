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
            widget.StatsGroup.gameObject.SetActive(selected);
            widget.Positioner.Offset0 = selected ? ShipWidgetSelectedOffset : default;
        }

        protected override void OnDestroy() {
            Game.Events.DeregisterAllForContext(this);
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
            LiveRouteData routeData = LiveRouteUtility.GetLiveRoute(shipId);

            for(int i = 0; i < ShipCount; i++) {
                RouteShipWidget widget = ShipWidgets[i];
                if (widget.ShipId == shipId) {
                    RouteShipUtility.PopulateWidgetRouteStats(widget, routeData.Stats);
                }
            }
        }

        private void OnShipClicked(PointerListener.EventData evt) {
            var widget = evt.Source.GetComponentInParent<RouteShipWidget>();
            RouteShipUtility.SelectShipFromWidget(widget);
        }
    }
}