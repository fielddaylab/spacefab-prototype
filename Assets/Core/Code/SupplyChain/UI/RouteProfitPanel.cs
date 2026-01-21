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
        public PointerListener FinalizeButton;

        public TMP_Text ProfitLabel;
        public TMP_Text TimeLabel;
        public Image DefenseDisplay;

        [NonSerialized] public FabMaterialSet DesiredMaterials;
        [NonSerialized] public int SellPrice;

        private unsafe void OnRouteStatsUpdated() {
            LiveRoutesState routesState = Find.State<LiveRoutesState>();
            var sprites = Find.GlobalAsset<SupplyChainSprites>();
            var math = Find.GlobalAsset<SupplyChainMath>();

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

            bool materialsFulfilled = (materials.A >= DesiredMaterials.A
                && materials.E >= DesiredMaterials.E
                && materials.B >= DesiredMaterials.B
                && materials.C >= DesiredMaterials.C
                && materials.D >= DesiredMaterials.D);
                
            using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                if (profit < 0) {
                    psb.Builder.Append('-');
                }
                psb.Builder.Append('$').AppendNoAlloc(Math.Abs(profit));
                ProfitLabel.SetText(psb);
                TimeLabel.SetText(time.ToStringLookup());
            }

            for(int i = sprites.DefenseSprites.Length; i-- > 0;) {
                if (i == 0 || probability > math.Reliabilities[i]) {
                    DefenseDisplay.sprite = sprites.DefenseSprites[i];
                    break;
                }
            }

            if (NotFulfilledGroup) {
                NotFulfilledGroup.SetActive(!materialsFulfilled);
            }
            if (FulfilledGroup) {
                FulfilledGroup.SetActive(materialsFulfilled);
            }
        }

        void IRegistrationCallbacks.OnDeregister() {
            Game.Events.DeregisterAllForContext(this);
        }

        void IRegistrationCallbacks.OnRegister() {
            Game.Events.Register(SupplyChainGame.Events.RouteStatsUpdated, OnRouteStatsUpdated);

            FinalizeButton.onClick.AddListener(() => {
                Game.Scenes.LoadMainScene(SceneReference.FromName("SupplyChainLoader"));
            });

            Game.Scenes.QueueOnLoad(OnRouteStatsUpdated);
        }
    }
}