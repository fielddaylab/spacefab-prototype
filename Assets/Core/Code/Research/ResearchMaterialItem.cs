using BeauPools;
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

        protected override void OnDisable() {
            base.OnDisable();
            ExplosionRoutine.Stop();
        }
    }

    static public partial class ResearchMaterialUtility {
        static public void ExplodeItem(ResearchMaterialItem item, ExplosionStyle style, float delay = 0) {
            Assert.True(item.CurrentSlot != null);
            if (item.Clickable) {
                item.Clickable.enabled = false;
            }
            item.ExplosionRoutine.Replace(item, ExplosionRoutine(item, style)).DelayBy(delay);
            BeginExplosions();
        }

        static private IEnumerator ExplosionRoutine(ResearchMaterialItem item, ExplosionStyle style) {
            item.Renderer.Renderer.sharedMaterial = Find.State<ResearchPools>().PreExplodeItemMaterial;
            yield return item.transform.MoveTo(item.transform.localPosition.x + 0.1f, 0.3f, Axis.X, Space.Self).Wave(Wave.Function.Sin, 6);
            Sfx.Play("Research.Gem.Explode");
            ResearchSlotUtility.FillInSlot(item.CurrentSlot, null);
        }

        static public void BeginExplosions() {
            ExplosionState expState = Find.State<ExplosionState>();
            if (!expState.AreAnyExploding) {
                expState.AreAnyExploding = true;
                expState.StateTimer = expState.PreExplosionCooldown;
                Game.Input.PauseAll();
            }
        }

        static public void BeginExplosions(float timeWindow) {
            ExplosionState expState = Find.State<ExplosionState>();
            if (!expState.AreAnyExploding) {
                expState.AreAnyExploding = true;
                expState.StateTimer = timeWindow;
            }
        }
    }

    public enum ExplosionStyle {
        Default,
        InvalidCombo,
        VoltageBreakdown,
        TemperatureBreakdown,
    }
}