using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay.Assets;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.SupplyChain {
    public sealed class RouteShipWidget : MonoBehaviour, IScenePreload {
        #region Types

        [Serializable]
        public struct StatBar {
            public EllipseGraphic[] Stats;
        }

        #endregion // Types

        #region Inspector

        public LayoutOffset Positioner;
        public Color32 RouteColor;

        [Header("Hovering")]
        public CursorHint CursorHint;
        public GameObject HoverObject;

        [Header("Info")]
        public TMP_Text NameDisplay;
        public Image IconDisplay;

        [Header("Stats")]
        public CanvasGroup StatsGroup;
        public StatBar SpeedStat;
        public StatBar CapacityStat;
        public StatBar DefenseStat;
        public StatBar CostStat;

        [Header("Route")]
        public CanvasGroup RouteGroup;
        public TMP_Text RouteTime;
        public TMP_Text RouteCost;
        public TMP_Text RouteReliability;

        #endregion // Inspector

        [NonSerialized] public RouteShip ShipAsset;
        [NonSerialized] public StringHash32 ShipId;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            CursorHint.UserData = this;
            CursorHint.onPointerEnter.Register(OnCursorHoverStart);
            CursorHint.onPointerExit.Register(OnCursorHoverEnd);
            return null;
        }

        private void OnCursorHoverStart() {
            HoverObject.SetActive(true);
        }

        private void OnCursorHoverEnd() {
            HoverObject.SetActive(false);
        }
    }

    static public partial class RouteShipUtility {
        static public void PopulateWidget(RouteShipWidget widget, RouteShip ship) {
            widget.ShipAsset = ship;

            if (ship) {
                widget.ShipId = ship.AssetId;

                widget.IconDisplay.sprite = ship.Icon;
                widget.NameDisplay.SetText(ship.DisplayName);

                PopulateWidgetStats(widget.SpeedStat, ship.Speed);
                PopulateWidgetStats(widget.CapacityStat, ship.Capacity);
                PopulateWidgetStats(widget.DefenseStat, ship.Defense);
                PopulateWidgetStats(widget.CostStat, ship.Cost);
            } else {
                widget.ShipId = default;
            }
        }

        static public void PopulateWidgetStats(in RouteShipWidget.StatBar statBar, int statValue) {
            for (int i = 0; i < statBar.Stats.Length; i++) {
                statBar.Stats[i].Outline = i >= statValue;
            }
        }

        static public void PopulateWidgetRouteStats(RouteShipWidget widget, SupplyRouteStats stats) {
            if (stats.Cost <= 0) {
                widget.RouteGroup.gameObject.SetActive(false);
                return;
            }

            using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                psb.Builder.Append("$").AppendNoAlloc(stats.Cost);
                widget.RouteCost.SetText(psb.Builder);

                psb.Builder.Clear();
                psb.Builder.AppendNoAlloc(stats.Time).Append("C");

                widget.RouteTime.SetText(psb.Builder);

                float percentage = 100f * stats.Reliability / SupplyUtility.MaxReliability;

                psb.Builder.Clear();
                psb.Builder.AppendNoAlloc((int)percentage).Append("%");

                widget.RouteReliability.SetText(psb.Builder);
            }

            widget.RouteGroup.gameObject.SetActive(true);
        }
    }
}