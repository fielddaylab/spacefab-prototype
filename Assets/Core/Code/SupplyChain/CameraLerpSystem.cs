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
    [SysUpdate(GameLoopPhase.LateUpdate, -100000)]
    public sealed class CameraLerpSystem : SharedStateSystemBehaviour<CameraControlState> {
        public override void ProcessWork(float deltaTime) {
            Vector2 frameSize = CameraUtility.GetFrustumSize(m_State.Camera, 0);
            Rect region = Geom.BoundsToRect(m_State.Region.bounds);

            Vector2 currentPos = m_State.CameraPosition.position;
            Vector2 targetPos = m_State.TargetPosition;
            targetPos = Geom.Constrain(targetPos, frameSize, region);

            m_State.TargetPosition = targetPos;

            DebugDraw.AddPoint(m_State.TargetPosition, 0.05f, Color.red);

            currentPos = Vector2.LerpUnclamped(currentPos, targetPos, TweenUtil.Lerp(m_State.InterpolationStrength, 1, deltaTime));
            currentPos = Geom.Constrain(currentPos, frameSize, region);
            m_State.CameraPosition.SetPosition(currentPos, Axis.XY, Space.World);
        }
    }
}