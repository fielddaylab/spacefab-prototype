using BeauRoutine;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.UI;
using System;
using System.Collections;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchMaterialItem : BatchedComponent {
        public ResearchMaterialRig Renderer;
        public Collider2D Clickable;
        public CursorHint Hint;

        [NonSerialized] public ResearchMaterial Material;
        [NonSerialized] public ResearchSlot CurrentSlot;

        public Routine ExplosionRoutine;
    }

    static public partial class ResearchMaterialUtility {
        static public void ExplodeItem(ResearchMaterialItem item) {
            Assert.True(item.CurrentSlot != null);
            if (item.Clickable) {
                item.Clickable.enabled = false;
            }
            item.ExplosionRoutine.Replace(item, ExplosionRoutine(item));
        }

        static private IEnumerator ExplosionRoutine(ResearchMaterialItem item) {
            item.Renderer.Renderer.sharedMaterial = Find.State<ResearchPools>().PreExplodeItemMaterial;
            yield return item.transform.MoveTo(item.transform.localPosition.x + 0.1f, 0.3f, Axis.X, Space.Self).Wave(Wave.Function.Sin, 6); 
            Sfx.Play("Research.Gem.Explode");
            ResearchSlotUtility.FillInSlot(item.CurrentSlot, null);
        }
    }
}