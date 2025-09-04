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
                return;
            }

            PathNode hoverNode = null;
            if (MouseControls.TryGetWorldPosition2D(out Vector2 worldPos)) {
                Collider2D node = Physics2D.OverlapCircle(worldPos, 0.005f, LayerMasks.SupplyNode_Mask);
                if (node != null) {
                    hoverNode = node.ResolveComponent<PathNode>();
                }
            }

            if (m_State.Node != hoverNode) {
                if (m_State.Node && m_State.Node.Highlight) {
                    m_State.Node.Highlight.HoverHighlight.enabled = false;
                }

                m_State.Node = hoverNode;

                if (hoverNode && hoverNode.Highlight) {
                    hoverNode.Highlight.HoverHighlight.enabled = true;
                }
            }
        }
    }
}