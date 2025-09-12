using BeauRoutine;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class CircuitRenderer : BatchedComponent {
        public SpriteRenderer CircuitFlow;
        public Sprite[] CircuitSpriteSequence;

        public SpriteRenderer Bulb;
        public Sprite BulbOffSprite;
        public Sprite BulbOnSprite;
        public SpriteRenderer BulbShine;
        public float AnimSpeedMultiplier = 4;

        [NonSerialized] public int CircuitSpriteIndex;
        [NonSerialized] public float CircuitSpriteSpeed;
        [NonSerialized] public float CircuitSpriteTimer;
    }

    static public partial class CircuitUtility {
        static public void SetLightStrength(CircuitRenderer circuit, float strength) {
            strength = Mathf.Abs(strength);

            circuit.Bulb.sprite = strength > 0 ? circuit.BulbOnSprite : circuit.BulbOffSprite;
            circuit.BulbShine.enabled = strength > 0;
            circuit.BulbShine.SetAlpha(strength);
        }

        static public void SetFlowSpeed(CircuitRenderer circuit, float speed) {
            circuit.CircuitFlow.enabled = speed != 0;
            if (speed == 0) {
                circuit.CircuitSpriteTimer = 0;
            } else {
                circuit.CircuitSpriteSpeed = speed;
            }
        }
    }
}