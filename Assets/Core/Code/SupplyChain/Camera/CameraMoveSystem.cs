using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Rendering;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class CameraMoveSystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork,
                SysUpdate.Default(),
                new SysPermissions().ReadWriteShared<CameraControlState>());
        }

        static private void ProcessWork(float deltaTime) {
            Find.State(out CameraControlState camState);
            
            Vector2 adjust = default;
            float moveSpeed = deltaTime * camState.MovementSpeed;
            if (Game.Input.IsKeyDown(KeyCode.A)) {
                adjust.x -= 1;
            }
            if (Game.Input.IsKeyDown(KeyCode.D)) {
                adjust.x += 1;
            }
            if (Game.Input.IsKeyDown(KeyCode.S)) {
                adjust.y -= 1;
            }
            if (Game.Input.IsKeyDown(KeyCode.W)) {
                adjust.y += 1;
            }

            if (adjust.x != 0 || adjust.y != 0) {
                adjust.Normalize();
                adjust.x *= moveSpeed;
                adjust.y *= moveSpeed;

                camState.TargetPosition += adjust;
            }
        }
    }
}