using BeauPools;
using BeauRoutine;
using BeauRoutine.Extensions;
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
            item.ExplosionRoutine.Replace(item, ExplosionRoutine(item, style, delay));
            BeginExplosions();
        }

        static private IEnumerator ExplosionRoutine(ResearchMaterialItem item, ExplosionStyle style, float delay) {
            ResearchPools pools = Find.State<ResearchPools>();

            if (style == ExplosionStyle.VoltageBreakdown) {
                VfxUtility.PlayFromPool(pools.BoltZapEffectPool, item.transform);
            }
            if (delay > 0) {
                yield return delay;
            }

            switch (style) {
                case ExplosionStyle.TooBig: {
                    TransformState localPos = TransformState.LocalState(item.transform);
                    yield return item.transform.MoveTo(item.transform.localPosition.y + 0.15f, 0.3f, Axis.Y, Space.Self).Wave(Wave.Function.Sin, 4);
                    Sfx.Play("Research.Gem.Explode");
                    KinematicAnimation anim = Find.State<ExplosionState>().TooBigAnimation;
                    yield return anim.Simulate(item.transform, 0.5f, Space.Self);
                    localPos.Apply(item.transform);
                    ResearchSlotUtility.FillInSlot(item.CurrentSlot, null);
                    yield return 0.05f;
                    break;
                }

                case ExplosionStyle.InvalidCombo: {
                    yield return item.transform.MoveTo(item.transform.localPosition.x + 0.1f, 0.3f, Axis.X, Space.Self).Wave(Wave.Function.Sin, 3);
                    Sfx.Play("Research.Gem.Explode");
                    VfxUtility.PlayFromPool(pools.ExplosionEffectPool, item.transform);
                    yield return 0.05f;
                    ResearchSlotUtility.FillInSlot(item.CurrentSlot, null);
                    break;
                }

                default: {
                    item.Renderer.Renderer.sharedMaterial = Find.State<ResearchPools>().PreExplodeItemMaterial;
                    yield return item.transform.MoveTo(item.transform.localPosition.x + 0.1f, 0.27f, Axis.X, Space.Self).Wave(Wave.Function.Sin, 6);
                    VfxUtility.PlayFromPool(pools.ExplosionEffectPool, item.transform);
                    Sfx.Play("Research.Gem.Explode");
                    yield return 0.05f;
                    ResearchSlotUtility.FillInSlot(item.CurrentSlot, null);
                    break;
                }
            }    
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
        TemperatureBreakdownHot,
        TemperatureBreakdownCold,
        TooBig,
    }
}