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

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            foreach(var ship in ShipWidgets) {
                ship.CursorHint.onClick.Register(OnShipClicked);
            }
            return null;
        }

        private void OnShipClicked(PointerEventData evt) {
            var widget = evt.pointerClick.GetComponentInParent<RouteShipWidget>();
            RouteShipUtility.SelectShipFromWidget(widget);
        }
    }
}