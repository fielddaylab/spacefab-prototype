using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Collections;
using FieldDay.Components;
using FieldDay.Rendering;
using FieldDay.SharedState;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchMaterialTray : SharedStateComponent, IRegistrationCallbacks {
        public Transform Root;
        public float Spacing;
        public Transform SelectionHighlight;

        [NonSerialized] public RingBuffer<ResearchMaterialItem> Items = new RingBuffer<ResearchMaterialItem>(8, RingBufferMode.Expand);

        void IRegistrationCallbacks.OnDeregister() {
            Game.Events?.DeregisterAllForContext(this);
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

            //SpaceFabGame.Events.Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_KnowledgeUpdated, OnKnowledgeUpdated)
            //    .Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_GoalHintRequested, OnGoalHintRequested);
        }

        private void OnKnowledgeUpdated(ResearchMaterialKnowledgePair pair) {
            //if ((pair.Chip) != 0) {
            //    ResearchMaterial material = Find.NamedAsset<ResearchMaterial>(pair.MaterialId);
            //    ResearchMaterialUtility.UpdateMaterialDisplayInTray(material);

            //    foreach(var slot in Find.Components<ResearchSlot>()) {
            //        if (slot.Item != null && slot.Item.Material == material) {
            //            slot.Item.Hint.TooltipHeader = material.DisplayName;
            //        }
            //    }
            //}
        }

        //private void OnGoalHintRequested(ResearchMaterialKnowledgePair pair) {
        //    ResearchMaterial material = Find.NamedAsset<ResearchMaterial>(pair.MaterialId);
        //    ResearchMaterialItem item = ResearchMaterialUtility.FindTrayItemForMaterial(material);
        //    if (item != null) {
        //        item.FlashRoutine.Replace(item, ItemFlashRoutine(item));
        //    }
        //}

        static private IEnumerator ItemFlashRoutine(ResearchMaterialItem item) {
            item.Flash.SetAlpha(0);
            item.Flash.Visible = true;
            yield return Tween.ZeroToOne(item.Flash.SetAlpha, 0.12f).YoyoLoop(2);
            item.Flash.Visible = false;
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
            item.Hint.TooltipHeader = IsNameKnown(material.AssetId) ? material.DisplayName : material.UnknownDisplayName;
            item.Material = material;
            item.CurrentSlot = null;

            float radius = item.Renderer.RendererPosition.localScale.x / 2;
            item.Clickable.radius = radius;
            item.GetComponent<LayoutSizeInfo>().Size = new Vector3(radius * 2, radius * 2, 0.1f);

            tray.Items.PushBack(item);

            return item;
        }

        static public void UpdateMaterialDisplayInTray(ResearchMaterial material) {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            foreach (var item in tray.Items) {
                if (item.Material == material) {
                    item.Hint.TooltipHeader = material.DisplayName;
                    break;
                }
            }
        }

        static public void ArrangeTrayItems() {
            ResearchMaterialTray tray = Find.State<ResearchMaterialTray>();
            LayoutOptions options = default;
            options.Spacing = tray.Spacing;
            options.Source = LayoutSource.Size;
            options.NormalizedAlignment = 0.5f;

            using(TempReferenceBuffer<Transform> transforms = TempReferenceBuffer<Transform>.Create(tray.Items.Count)) {
                foreach(var item in tray.Items) {
                    transforms.Add(item.transform);
                }
                Positioning.AxisLayout(transforms, options, 0, Axis.Y);
            }
        }
    }
}