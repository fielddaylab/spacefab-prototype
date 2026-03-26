using BeauPools;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Physics;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class ParallaxLayerSystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork,
                new SysUpdate(GameLoopPhase.UnscaledLateUpdate, 999),
                new SysPermissions().Write<ParallaxLayer>());
        }

        static private void ProcessWork(float deltaTime) {
            Vector2 cameraPos = Game.Rendering.PrimaryCamera.transform.position;

            foreach(var c in Find.Components<ParallaxLayer>()) {
                c.transform.SetPosition(cameraPos * c.Scale, Axis.XY, Space.World);
            }
        }
    }
}