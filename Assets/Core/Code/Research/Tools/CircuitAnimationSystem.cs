using FieldDay;
using FieldDay.Components;
using FieldDay.Systems;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class CircuitAnimationSystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork, SysUpdate.Default(),
                new SysPermissions().ReadWrite<CircuitRenderer>());
        }

        static private readonly float[] RotSpeeds = { 24, -30.2f, 17 };

        static private void ProcessWork(float deltaTime) {
            foreach(var obj in Find.Components<CircuitRenderer>()) {
                if (obj.CircuitSpriteSpeed == 0) {
                    return;
                }

                obj.CircuitSpriteTimer += deltaTime * Math.Abs(obj.CircuitSpriteSpeed) * obj.AnimSpeedMultiplier;
                int framesAdvanced = (int) obj.CircuitSpriteTimer;
                if (framesAdvanced > 0) {
                    obj.CircuitSpriteTimer -= framesAdvanced;
                    int maxFrames = obj.CircuitSpriteSequence.Length;
                    obj.CircuitSpriteIndex = (obj.CircuitSpriteIndex + framesAdvanced * Math.Sign(obj.CircuitSpriteSpeed) + maxFrames) % maxFrames;
                    obj.CircuitFlow.sprite = obj.CircuitSpriteSequence[obj.CircuitSpriteIndex];
                }

                for(int i = 0; i < obj.BulbShines.Length; i++) {
                    obj.BulbShines[i].transform.Rotate(new Vector3(0, 0, RotSpeeds[i] * deltaTime), Space.Self);
                }
            }
        }
    }
}