using BeauPools;
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
    public sealed class RouteProfitPanel : SharedPanel, IRegistrationCallbacks {
        public TMP_Text Label;

        [NonSerialized] public FabMaterialSet DesiredMaterials;
        [NonSerialized] public int SellPrice;

        private unsafe void OnRouteStatsUpdated() {
            LiveRoutesState routesState = Find.State<LiveRoutesState>();
            var sprites = Find.GlobalAsset<SupplyChainSprites>();

            FabMaterialSet materials = default;
            int cost = 0;
            int time = 0;
            double probability = 1;
            for(int i = 0; i < routesState.RouteCount; i++) {
                var stats = routesState.Routes[i].Stats;
                cost += (int) stats.Cost;
                time = Math.Max(time, stats.Time);
                probability *= stats.Reliability / (double) SupplyUtility.MaxReliability;
                SupplyUtility.AccumulateMaterials(ref materials, stats);
            }
            int profit = SellPrice - cost;

            if (materials.Insulator >= DesiredMaterials.Insulator
                && materials.Conductor >= DesiredMaterials.Conductor
                && materials.Semiconductor >= DesiredMaterials.Semiconductor
                && materials.DopantN >= DesiredMaterials.DopantN
                && materials.DopantP >= DesiredMaterials.DopantP) {
                using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    if (profit < 0) {
                        psb.Builder.Append('-');
                    }
                    psb.Builder.Append('$').AppendNoAlloc(Math.Abs(profit)).Append(" in ").AppendNoAlloc(time).Append("C (").AppendNoAlloc((int) (100 * probability)).Append("%)");
                    Label.SetText(psb);
                }
            } else {
                Label.SetText("---");
            }
        }

        void IRegistrationCallbacks.OnDeregister() {
            Game.Events.DeregisterAllForContext(this);
        }

        void IRegistrationCallbacks.OnRegister() {
            Game.Events.Register(SupplyChainGame.Events.RouteStatsUpdated, OnRouteStatsUpdated);
        }
    }
}