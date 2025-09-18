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
        static public class Events {
            static public readonly StringHash32 RouteStatsUpdated = "SupplyChain::RouteStatsUpdated";
        }

        [AssetName(typeof(RouteShip))] public StringHash32[] Ships;
        public FabMaterialSet RequiredMaterials;

        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            var routePanel = Find.Panel<RouteShipPanel>();
            routePanel.PopulateShips(Ships);
            var requestPanel = Find.Panel<RouteRequestPanel>();
            requestPanel.PopulateResources(RequiredMaterials);
            return null;
        }
    }
}