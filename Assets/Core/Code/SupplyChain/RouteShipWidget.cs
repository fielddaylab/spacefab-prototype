using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.SupplyChain {
    public sealed class RouteShipWidget : MonoBehaviour, IScenePreload {
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
        public GuiCounter SpeedStat;
        public GuiCounter CostStat;

        [Header("Route")]
        public CanvasGroup RouteGroup;
        public Graphic RouteColorIndicator;
        public GuiCounter RouteTime;
        public TMP_Text RouteCost;
        public Image RouteReliability;
        public GameObject[] RouteMaterialSlots;
        public Image[] RouteMaterials;
        public GameObject TimeAlertGroup;
        public GameObject ReliabilityAlertGroup;

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

                widget.SpeedStat.SetValue(ship.Speed, false);
                widget.CostStat.SetValue(ship.Cost, false);

                widget.RouteColorIndicator.color = widget.RouteColor;

                for(int i = 0; i < widget.RouteMaterialSlots.Length; i++) {
                    widget.RouteMaterialSlots[i].SetActive(ship.Capacity > i);
                }
            } else {
                widget.ShipId = default;
            }
        }

        static public void PopulateWidgetRouteStats(RouteShipWidget widget, SupplyRouteStats stats) {
            if (stats.Cost <= 0) {
                widget.RouteGroup.gameObject.SetActive(false);

                for(int i = 0; i < widget.RouteMaterials.Length; i++) {
                    widget.RouteMaterials[i].gameObject.SetActive(false);
                }
                return;
            }

            SupplyChainSprites supplySprites = Find.GlobalAsset<SupplyChainSprites>();
            SupplyChainMath supplyMath = Find.GlobalAsset<SupplyChainMath>();

            using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                psb.Builder.Append("$").AppendNoAlloc(stats.Cost);
                widget.RouteCost.SetText(psb.Builder);

                widget.RouteTime.SetValue(stats.Time, false);

                float percentage = (float) stats.Reliability / SupplyUtility.MaxReliability;

                int defenseIndex = supplyMath.GetReliabilityIndex(percentage);
                widget.RouteReliability.sprite = supplySprites.DefenseSprite(defenseIndex);
            }

            unsafe {
                int materialCount = 0;
                PopulateMaterialCategory(widget, FabMaterial.Insulator, stats.Materials[(int) FabMaterial.Insulator - 1], ref materialCount, supplySprites);
                PopulateMaterialCategory(widget, FabMaterial.Semiconductor, stats.Materials[(int) FabMaterial.Semiconductor - 1], ref materialCount, supplySprites);
                PopulateMaterialCategory(widget, FabMaterial.Conductor, stats.Materials[(int) FabMaterial.Conductor - 1], ref materialCount, supplySprites);
                PopulateMaterialCategory(widget, FabMaterial.DopantN, stats.Materials[(int) FabMaterial.DopantN - 1], ref materialCount, supplySprites);
                PopulateMaterialCategory(widget, FabMaterial.DopantP, stats.Materials[(int) FabMaterial.DopantP - 1], ref materialCount, supplySprites);

                for(int i = materialCount; i < widget.RouteMaterials.Length; i++) {
                    widget.RouteMaterials[i].gameObject.SetActive(false);
                }
            }

            widget.RouteGroup.gameObject.SetActive(true);
        }

        static public void PopulateWidgetBottleneckAlerts(RouteShipWidget widget, bool isTimeBottleneck, bool isReliabilityBottleneck) {
            widget.TimeAlertGroup.SetActive(isTimeBottleneck);
            widget.ReliabilityAlertGroup.SetActive(isReliabilityBottleneck);
        }

        static private unsafe void PopulateMaterialCategory(RouteShipWidget widget, FabMaterial material, int count, ref int totalMaterials, SupplyChainSprites sprites) {
            while (count-- > 0) {
                int index = totalMaterials++;
                widget.RouteMaterials[index].gameObject.SetActive(true);
                widget.RouteMaterials[index].sprite = sprites.MaterialSpriteTiny(material);
            }
        }
    }
}