using BeauPools;
using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.UI;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchSlot : BatchedComponent {
        public Collider2D Region;
        public Transform Root;
        public GameObject EmptyContents;
        public bool AllowSwap;
        public CursorHint Hint;
        public ParticleSystem HoverVfx;

        [NonSerialized] public bool Locked;
        [NonSerialized] public ResearchMaterialItem Item;

        public CastableEvent<ResearchSlot, ResearchMaterialItem> OnSlotUpdated = new CastableEvent<ResearchSlot, ResearchMaterialItem>();
    }

    static public partial class ResearchSlotUtility {
        static public void FillInSlot(ResearchSlot slot, ResearchMaterial material) {
            if (!material) {
                if (slot.Item) {
                    Pool.TryFree(slot.Item);
                    slot.Item = null;
                    slot.Hint.enabled = true;
                    slot.OnSlotUpdated.Invoke(slot, null);
                    if (slot.EmptyContents) {
                        slot.EmptyContents.SetActive(true);
                    }
                }
            } else {
                if (!slot.Item) {
                    slot.Item = Find.State<ResearchPools>().Items.Alloc(slot.Root);
                    slot.Item.CurrentSlot = slot;
                    if (slot.EmptyContents) {
                        slot.EmptyContents.SetActive(false);
                    }
                }

                ResearchMaterialUtility.ApplyPropertiesToRig(slot.Item.Renderer, material);
                slot.Item.Material = material;
                slot.Item.Hint.TooltipHeader = ResearchMaterialUtility.IsNameKnown(material.AssetId) ? material.DisplayName : material.UnknownDisplayName;

                slot.Hint.enabled = false;
                slot.OnSlotUpdated.Invoke(slot, slot.Item);
            }
        }
    }
}