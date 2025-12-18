using System;
using BeauRoutine;
using FieldDay.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay.UI.Animation {
    public sealed class FlashAnim : LiteAnimator<Graphic> {

        private readonly float m_Duration;
        private readonly Curve m_Ease;

        public FlashAnim(float duration, Curve ease = Curve.Linear) {
            m_Duration = duration;
            m_Ease = ease;
        }

        static private void PrepareState(ref LiteAnimatorState state, Color color, float duration, float delay, Curve easing) {
            state.Duration = duration;
            state.TimeRemaining = duration + delay;
            state.InitParamA.Color = color;
            state.Easing = easing;
        }

        public override void InitAnimation(Graphic target, ref LiteAnimatorState state) {
            if (state.TimeRemaining <= state.Duration) {
                target.enabled = true;
                target.color = state.InitParamA.Color;
            }
        }

        public override void ResetAnimation(Graphic target, ref LiteAnimatorState state) {
            target.enabled = false;
        }

        public override bool UpdateAnimation(Graphic target, ref LiteAnimatorState state, float deltaTime) {
            state.TimeRemaining -= deltaTime;
            float percent = state.Easing.Evaluate(1 - Math.Max(0, state.TimeRemaining / state.Duration));
            if (percent >= 0) {
                Color newColor = state.InitParamA.Color;
                newColor.a *= (1f - percent);
                target.color = newColor;
                target.enabled = newColor.a > 0;
            }
            return state.TimeRemaining > 0;
        }

        static public readonly FlashAnim Default = new FlashAnim(12 / 60f, Curve.Linear);

        static public AnimHandle Play(Graphic graphic, Color color, FlashAnim config, float delay = 0, GameLoopPhase phase = GameLoopPhase.Update) {
            LiteAnimatorState animState = new LiteAnimatorState();
            PrepareState(ref animState, color, config.m_Duration, delay, config.m_Ease);
            return Game.Animation.AddLiteAnimator(config, graphic, animState, phase);
        }
    }
}