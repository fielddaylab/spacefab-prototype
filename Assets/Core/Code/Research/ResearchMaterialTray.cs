using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Rendering;
using FieldDay.SharedState;
using System;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchMaterialTray : SharedStateComponent {
        public Transform Root;
        public float Spacing;

        [NonSerialized] public RingBuffer<ResearchMaterialItem> Items = new RingBuffer<ResearchMaterialItem>(8, RingBufferMode.Expand);
    }

    static public partial class ResearchMaterialUtility {
        static public void SpawnNewTrayItem(ResearchMaterial material) {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            ResearchPools pools = Find.State<ResearchPools>();
            
            ResearchMaterialItem item = pools.Items.Alloc(tray.Root);
            ApplyPropertiesToRig(item.Renderer, material);
            item.Material = material;
            item.CurrentSlot = null;

            tray.Items.PushBack(item);
        }

        static public void ArrangeTrayItems() {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            int itemCount = tray.Root.childCount;
            if (itemCount > 0) {
                float left = (itemCount - 1) * -0.5f * tray.Spacing;
                for(int i = 0; i < itemCount; i++) {
                    Transform item = tray.Root.GetChild(i);
                    item.localPosition = new Vector3(left + tray.Spacing * i, 0, 0);
                }
            }
        }
    }
}