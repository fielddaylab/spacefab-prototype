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
    public sealed class RouteRequestPanel : SharedPanel, IRegistrationCallbacks {
        public RouteMaterialWidget[] Resources;
        public TMP_Text SellPrice;
        public GameObject HintGroup;
        public TMP_Text HintLabel;

        [NonSerialized] public FabMaterial[] ResourceMap = new FabMaterial[10];
        [NonSerialized] public int ResourceCount = 0;

        public void PopulateResources(FabMaterialSet materials) {
            var sprites = Find.GlobalAsset<SupplyChainSprites>();

            ResourceCount = 0;

            PopulateCategory(materials.A, FabMaterial.A, sprites);
            PopulateCategory(materials.B, FabMaterial.B, sprites);
            PopulateCategory(materials.C, FabMaterial.C, sprites);
            PopulateCategory(materials.D, FabMaterial.D, sprites);
            PopulateCategory(materials.E, FabMaterial.E, sprites);

            for (int i = ResourceCount; i < Resources.Length; i++) {
                ResourceMap[i] = FabMaterial.None;
                Resources[i].gameObject.SetActive(false);
            }
        }

        public void PopulateHint(SupplyChainLevel level) {
            if (!level || string.IsNullOrEmpty(level.Hint)) {
                HintGroup.SetActive(false);
            } else {
                HintLabel.SetText(string.Format(level.Hint, level.SellPrice));
                HintGroup.SetActive(true);
            }
        }

        private unsafe void OnRouteStatsUpdated() {
            LiveRoutesState routesState = Find.State<LiveRoutesState>();
            var sprites = Find.GlobalAsset<SupplyChainSprites>();

            FabMaterialSet materials = default;
            for(int i = 0; i < routesState.RouteCount; i++) {
                SupplyUtility.AccumulateMaterials(ref materials, routesState.Routes[i].Stats);
            }

            for(int i = 0; i < ResourceCount; i++) {
                FabMaterial material = ResourceMap[i];
                Image icon = Resources[i].Icon;

                bool isFilled = false;
                switch (material) {
                    case FabMaterial.A: {
                        if (materials.A > 0) {
                            materials.A--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.B: {
                        if (materials.B > 0) {
                            materials.B--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.C: {
                        if (materials.C > 0) {
                            materials.C--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.D: {
                        if (materials.D > 0) {
                            materials.D--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.E: {
                        if (materials.E > 0) {
                            materials.E--;
                            isFilled = true;
                        }
                        break;
                    }
                }

                icon.sprite = isFilled ? sprites.MaterialSprite(material) : sprites.MaterialSpriteOutline(material);
            }
        }

        void IRegistrationCallbacks.OnDeregister() {
            Game.Events.DeregisterAllForContext(this);
        }

        void IRegistrationCallbacks.OnRegister() {
            Game.Events.Register(SupplyChainGame.Events.RouteStatsUpdated, OnRouteStatsUpdated);
        }

        private void PopulateCategory(int count, FabMaterial material, SupplyChainSprites sprites) {
            while (count-- > 0) {
                int index = ResourceCount++;
                ResourceMap[index] = material;
                Resources[index].gameObject.SetActive(true);
                Resources[index].Icon.sprite = sprites.MaterialSpriteOutline(material);
            }
        }
    }
}