using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Rendering;
using FieldDay.SharedState;
using System;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchMaterialTray : SharedStateComponent, IRegistrationCallbacks {
        public Transform Root;
        public float Spacing;
        public Transform SelectionHighlight;

        [NonSerialized] public RingBuffer<ResearchMaterialItem> Items = new RingBuffer<ResearchMaterialItem>(8, RingBufferMode.Expand);

        void IRegistrationCallbacks.OnDeregister() {
            Game.Events.DeregisterAllForContext(this);
        }

        void IRegistrationCallbacks.OnRegister() {
            SelectionHighlight.gameObject.SetActive(false);
            Find.State<ResearchSelectionState>().OnUpdated.Register((m) => {
                ResearchMaterialItem item = ResearchMaterialUtility.FindTrayItemForMaterial(m);
                if (item != null) {
                    SelectionHighlight.gameObject.SetActive(true);
                    SelectionHighlight.position = item.transform.position;
                } else {
                    SelectionHighlight.gameObject.SetActive(false);
                }
            });

            SpaceFabGame.Events.Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_KnowledgeUpdated, OnKnowledgeUpdated);
        }

        private void OnKnowledgeUpdated(ResearchMaterialKnowledgePair pair) {
            if ((pair.Knowledge & ResearchMaterialKnowledge.Name) != 0) {
                ResearchMaterial material = Find.NamedAsset<ResearchMaterial>(pair.MaterialId);
                ResearchMaterialUtility.UpdateMaterialDisplayInTray(material);

                foreach(var slot in Find.Components<ResearchSlot>()) {
                    if (slot.Item != null && slot.Item.Material == material) {
                        slot.Item.Hint.TooltipHeader = material.DisplayName;
                        slot.Item.Renderer.Label.SetText(material.ChemicalSymbol);
                    }
                }

                ResearchDragState dragState = Find.State<ResearchDragState>();
                if (dragState.CurrentlyDragging == material) {
                    dragState.DragRenderer.Label.SetText(material.ChemicalSymbol);
                }
            }
        }
    }

    static public partial class ResearchMaterialUtility {
        static public ResearchMaterialItem FindTrayItemForMaterial(ResearchMaterial material) {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            return tray.Items.Find((a, b) => a.Material == b, material);
        }

        static public ResearchMaterialItem SpawnNewTrayItem(ResearchMaterial material) {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            ResearchPools pools = Find.State<ResearchPools>();
            
            ResearchMaterialItem item = pools.Items.Alloc(tray.Root);
            ApplyPropertiesToRig(item.Renderer, material);
            item.Hint.TooltipHeader = material.UnknownDisplayName;
            item.Material = material;
            item.CurrentSlot = null;

            tray.Items.PushBack(item);

            return item;
        }

        static public void UpdateMaterialDisplayInTray(ResearchMaterial material) {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            foreach (var item in tray.Items) {
                if (item.Material == material) {
                    item.Hint.TooltipHeader = material.DisplayName;
                    item.Renderer.Label.SetText(material.ChemicalSymbol);
                    break;
                }
            }
        }

        static public void ArrangeTrayItems() {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            int itemCount = tray.Root.childCount;
            if (itemCount > 0) {
                float left = (itemCount - 1) * -0.5f * tray.Spacing;
                for(int i = 0; i < itemCount; i++) {
                    Transform item = tray.Root.GetChild(i);
                    item.localPosition = new Vector3(0, left + tray.Spacing * i, 0);
                }
            }
        }
    }
}