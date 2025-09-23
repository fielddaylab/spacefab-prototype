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
        public Image[] Resources;

        [NonSerialized] public FabMaterial[] ResourceMap = new FabMaterial[10];
        [NonSerialized] public int ResourceCount = 0;

        public void PopulateResources(FabMaterialSet materials) {
            var sprites = Find.GlobalAsset<SupplyChainSprites>();

            ResourceCount = 0;

            PopulateCategory(materials.Insulator, FabMaterial.Insulator, sprites);
            PopulateCategory(materials.Semiconductor, FabMaterial.Semiconductor, sprites);
            PopulateCategory(materials.Conductor, FabMaterial.Conductor, sprites);
            PopulateCategory(materials.DopantN, FabMaterial.DopantN, sprites);
            PopulateCategory(materials.DopantP, FabMaterial.DopantP, sprites);

            for (int i = ResourceCount; i < Resources.Length; i++) {
                ResourceMap[i] = FabMaterial.None;
                Resources[i].gameObject.SetActive(false);
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
                Image icon = Resources[i];

                bool isFilled = false;
                switch (material) {
                    case FabMaterial.Insulator: {
                        if (materials.Insulator > 0) {
                            materials.Insulator--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.Semiconductor: {
                        if (materials.Semiconductor > 0) {
                            materials.Semiconductor--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.Conductor: {
                        if (materials.Conductor > 0) {
                            materials.Conductor--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.DopantN: {
                        if (materials.DopantN > 0) {
                            materials.DopantN--;
                            isFilled = true;
                        }
                        break;
                    }
                    case FabMaterial.DopantP: {
                        if (materials.DopantP > 0) {
                            materials.DopantP--;
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
                Resources[index].sprite = sprites.MaterialSpriteOutline(material);
            }
        }
    }
}