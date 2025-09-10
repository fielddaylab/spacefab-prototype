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
    [SysUpdate(GameLoopPhase.UnscaledLateUpdate, 999)]
    public sealed class ParallaxLayerSystem : ComponentSystemBehaviour<ParallaxLayer> {
        public override void ProcessWork(float deltaTime) {
            Vector2 cameraPos = Game.Rendering.PrimaryCamera.transform.position;
            foreach(var c in m_Components) {
                c.transform.SetPosition(cameraPos * c.Scale, Axis.XY, Space.World);
            }
        }
    }
}