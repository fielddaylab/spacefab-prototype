using FieldDay.Components;
using FieldDay.Systems;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class CircuitAnimationSystem : ComponentSystemBehaviour<CircuitRenderer> {
        public override void ProcessWork(float deltaTime) {
            foreach(var obj in m_Components) {
                if (obj.CircuitSpriteSpeed == 0) {
                    return;
                }

                obj.CircuitSpriteTimer += deltaTime * Math.Abs(obj.CircuitSpriteSpeed);
                int framesAdvanced = (int) obj.CircuitSpriteTimer;
                if (framesAdvanced > 0) {
                    obj.CircuitSpriteTimer -= framesAdvanced;
                    int maxFrames = obj.CircuitSpriteSequence.Length;
                    obj.CircuitSpriteIndex = (obj.CircuitSpriteIndex + framesAdvanced * Math.Sign(obj.CircuitSpriteSpeed) + maxFrames) % maxFrames;
                    obj.CircuitFlow.sprite = obj.CircuitSpriteSequence[obj.CircuitSpriteIndex];
                }
            }
        }
    }
}