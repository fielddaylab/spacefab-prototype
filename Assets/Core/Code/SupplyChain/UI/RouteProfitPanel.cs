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
using FieldDay.UI.Widgets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceFab.SupplyChain {
    public sealed class RouteProfitPanel : SharedPanel, IRegistrationCallbacks {
        public GameObject NotFulfilledGroup;

        public GameObject FulfilledGroup;
        public TMP_Text ProfitLabel;
        public GuiCounter TimeLabel;

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
                if (stats.Time <= 0) {
                    continue;
                }

                cost += (int) stats.Cost;
                time = Math.Max(time, stats.Time);
                probability *= stats.Reliability / (double) SupplyUtility.MaxReliability;
                SupplyUtility.AccumulateMaterials(ref materials, stats);
            }
            int profit = SellPrice - cost;

            if (materials.A >= DesiredMaterials.A
                && materials.E >= DesiredMaterials.E
                && materials.B >= DesiredMaterials.B
                && materials.C >= DesiredMaterials.C
                && materials.D >= DesiredMaterials.D) {
                NotFulfilledGroup.SetActive(false);
                FulfilledGroup.SetActive(true);
                using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    if (profit < 0) {
                        psb.Builder.Append('-');
                    }
                    psb.Builder.Append('$').AppendNoAlloc(Math.Abs(profit));
                    ProfitLabel.SetText(psb);
                    TimeLabel.SetValue(time, false);
                }
            } else {
                FulfilledGroup.SetActive(false);
                NotFulfilledGroup.SetActive(true);
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