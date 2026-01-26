using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class SupplyChainGame : SceneController {
        static public class Events {
            static public readonly StringHash32 RouteStatsUpdated = "SupplyChain::RouteStatsUpdated";
        }

        [AssetName(typeof(RouteShip))] public StringHash32[] Ships;
        public FabMaterialSet RequiredMaterials;
        public int SellPrice = 10;
        public SceneReference DefaultScene;

        static public SupplyChainLevel CurrentLevel { get; private set; }

        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            Game.Scenes.GetLoadContext(out var context);
            SceneReference toLoad = DefaultScene;
            StringHash32 levelId = context.Task.Name;
            if (!levelId.IsEmpty) {
                SupplyChainLevel level = Find.NamedAsset<SupplyChainLevel>(levelId);
                CurrentLevel = level;
                Ships = level.Ships;
                RequiredMaterials = level.RequiredMaterials;
                SellPrice = level.SellPrice;
                toLoad = level.Scene;
            }

            Game.Scenes.LoadAuxScene(toLoad, default);
            return null;
        }

        protected override void OnSceneEnable() {
            var routePanel = Find.Panel<RouteShipPanel>();
            routePanel.PopulateShips(Ships);
            var requestPanel = Find.Panel<RouteRequestPanel>();
            requestPanel.PopulateResources(RequiredMaterials);
            requestPanel.PopulateHint(CurrentLevel);
            //requestPanel.SellPrice.SetText("Sell Price: $" + SellPrice.ToStringLookup());
            var profitPanel = Find.Panel<RouteProfitPanel>();
            profitPanel.DesiredMaterials = RequiredMaterials;
            profitPanel.SellPrice = SellPrice;

            var lineDrawerSystem = Find.State<RouteDrawerState>();
            int nodeIndex = 0;
            foreach (var node in Find.Components<PathNode>()) {
                node.NodeBitIndex = nodeIndex++;
                if ((node.Flags & PathNodeFlags.IsDestination) != 0) {
                    lineDrawerSystem.StartingNode = node;
                }
            }
            Assert.True(nodeIndex <= 64, "Overflowed limit of 64 nodes in one level");
        }

        protected override void OnSceneUnload() {
            CursorHint.DefaultCursor = null;
            CurrentLevel = null;
        }
    }
}