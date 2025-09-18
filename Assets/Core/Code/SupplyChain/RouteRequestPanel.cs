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
    public sealed class RouteRequestPanel : SharedPanel {
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