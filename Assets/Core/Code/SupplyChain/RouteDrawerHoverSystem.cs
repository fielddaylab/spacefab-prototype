using BeauPools;
using BeauUtil;
using FieldDay;
using FieldDay.Physics;
using FieldDay.SharedState;
using FieldDay.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class RouteDrawerHoverSystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork,
                new SysUpdate(GameLoopPhase.LateUpdate, 90),
                new SysPermissions().ReadWriteShared<RouteHoverState>()
                    .ReadWrite<PathNode>());
        }

        static private void ProcessWork(float deltaTime) {
            ECS.GetState(out RouteHoverState hoverState);
            
            if (hoverState.Locked) {
                hoverState.MousePosition = null;
                return;
            }

            PathNode hoverNode = null;
            if (!Game.Input.IsPointerOverCanvas() && MouseControls.TryGetWorldPosition2D(out Vector2 worldPos)) {
                hoverState.MousePosition = worldPos;
                Collider2D node = Physics2D.OverlapCircle(worldPos, 0, LayerMasks.SupplyNode_Mask);
                if (node != null) {
                    hoverNode = node.ResolveComponent<PathNode>();
                }
            } else {
                hoverState.MousePosition = null;
            }

            if (hoverState.Node != hoverNode) {
                if (hoverState.Node && hoverState.Node.Highlight) {
                    hoverState.Node.Highlight.HoverHighlight.enabled = false;
                    hoverState.Node.Highlight.HasHover = false;
                }

                hoverState.Node = hoverNode;

                if (hoverNode && hoverNode.Highlight) {
                    hoverNode.Highlight.HoverHighlight.enabled = true;
                    hoverNode.Highlight.HasHover = true;
                }
            }

        }
    }
}