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
    [SysUpdate(GameLoopPhase.LateUpdate, 90)]
    public sealed class RouteDrawerHoverSystem : SharedStateSystemBehaviour<RouteHoverState> {
        public override void ProcessWork(float deltaTime) {
            if (m_State.Locked) {
                m_State.MousePosition = null;
                return;
            }

            PathNode hoverNode = null;
            if (!Game.Input.IsPointerOverCanvas() && MouseControls.TryGetWorldPosition2D(out Vector2 worldPos)) {
                m_State.MousePosition = worldPos;
                Collider2D node = Physics2D.OverlapCircle(worldPos, 0.005f, LayerMasks.SupplyNode_Mask);
                if (node != null) {
                    hoverNode = node.ResolveComponent<PathNode>();
                }
            } else {
                m_State.MousePosition = null;
            }

            if (m_State.Node != hoverNode) {
                if (m_State.Node && m_State.Node.Highlight) {
                    m_State.Node.Highlight.HoverHighlight.enabled = false;
                    m_State.Node.Highlight.HasHover = false;
                }

                m_State.Node = hoverNode;

                if (hoverNode && hoverNode.Highlight) {
                    hoverNode.Highlight.HoverHighlight.enabled = true;
                    hoverNode.Highlight.HasHover = true;
                }
            }

        }
    }
}