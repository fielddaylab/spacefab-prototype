using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Systems;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [SysUpdate(GameLoopPhase.LateUpdate, 150)]
    public sealed class PortDetailsDisplaySystem : ComponentSystemBehaviour<PortDetailsDisplayState> {
        public override void ProcessWorkForComponent(PortDetailsDisplayState component, float deltaTime) {
            RouteDrawerState routeDrawer = Find.State<RouteDrawerState>();
            RouteHoverState hoverState = Find.State<RouteHoverState>();
            LiveRoutesState liveState = Find.State<LiveRoutesState>();
            PortPools pools = Find.State<PortPools>();
            SupplyChainSprites sprites = Find.GlobalAsset<SupplyChainSprites>();

            bool pathHighlight = false;// routeDrawer.DrawState == RouteDrawState.InProgress;

            foreach (var state in m_Components) {
                PortDetailsMode desiredMode;
                if (pathHighlight && state.Highlight.HasPath) {
                    desiredMode = PortDetailsMode.Interactive;
                } else if (hoverState.Node == state.Node) {
                    desiredMode = PortDetailsMode.Hover;
                } else {
                    desiredMode = PortDetailsMode.Off;
                }

                PortUtility.SetDetailsMode(state, desiredMode, pools, sprites);
            }
        }
    }
}