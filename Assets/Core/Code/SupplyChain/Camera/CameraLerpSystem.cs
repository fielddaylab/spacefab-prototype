using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Debugging;
using FieldDay.Rendering;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class CameraLerpSystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork, new SysUpdate(GameLoopPhase.LateUpdate, -100000),
                new SysPermissions().ReadWriteShared<CameraControlState>());
        }
        
        static private void ProcessWork(float deltaTime) {
            Find.State(out CameraControlState camState);

            Vector2 frameSize = CameraUtility.GetFrustumSize(camState.Camera, 0);
            Rect region = Geom.BoundsToRect(camState.Region.bounds);

            Vector2 currentPos = camState.CameraPosition.position;
            Vector2 targetPos = camState.TargetPosition;
            targetPos = Geom.Constrain(targetPos, frameSize, region);

            camState.TargetPosition = targetPos;

            DebugDraw.AddPoint(camState.TargetPosition, 0.05f, Color.red);

            currentPos = Vector2.LerpUnclamped(currentPos, targetPos, TweenUtil.Lerp(camState.InterpolationStrength, 1, deltaTime));
            currentPos = Geom.Constrain(currentPos, frameSize, region);
            camState.CameraPosition.SetPosition(currentPos, Axis.XY, Space.World);
        }
    }
}