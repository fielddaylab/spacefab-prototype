using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Scenes;
using FieldDay.SharedState;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class SupplyChainGame : SceneController {
        [AssetName(typeof(RouteShip))] public StringHash32[] Ships;

        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            var routePanel = Find.Panel<RouteShipPanel>();
            routePanel.PopulateShips(Ships);
            return null;
        }
    }
}