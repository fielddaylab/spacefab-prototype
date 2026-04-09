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
        public SpriteRenderer[] BulbShines;
        public float AnimSpeedMultiplier = 4;

        [NonSerialized] public int CircuitSpriteIndex;
        [NonSerialized] public float CircuitSpriteSpeed;
        [NonSerialized] public float CircuitSpriteTimer;
    }

    static public partial class CircuitUtility {
        static public void SetLightStrength(CircuitRenderer circuit, float strength) {
            strength = Mathf.Abs(strength);

            circuit.Bulb.sprite = strength > 0.1f ? circuit.BulbOnSprite : circuit.BulbOffSprite;
            for(int i = 0; i < circuit.BulbShines.Length; i++) {
                circuit.BulbShines[i].enabled = strength > 0;
                circuit.BulbShines[i].SetAlpha(strength * strength);
            }
        }

        static public void SetFlowSpeed(CircuitRenderer circuit, float speed) {
            circuit.CircuitFlow.SetAlpha(Math.Abs(speed));
            circuit.CircuitFlow.enabled = speed != 0;
            if (speed == 0) {
                circuit.CircuitSpriteTimer = 0;
            } else {
                circuit.CircuitSpriteSpeed = speed;
            }
        }
    }
}