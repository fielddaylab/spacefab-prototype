using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Systems;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class PortDetailsDisplaySystem : SystemModule {
        protected override unsafe void RegisterSystems(ref SystemRegistrationTable ecs) {
            ecs.Register(&ProcessWork,
                new SysUpdate(GameLoopPhase.LateUpdate, 150),
                new SysPermissions().ReadWrite<PortDetailsDisplayState>()
                    .ReadWriteShared<PortPools>()
                    .ReadShared<LiveRoutesState>()
                    .ReadShared<RouteHoverState>()
                    .ReadShared<RouteDrawerState>()
                );
        }
        
        static private void ProcessWork(float deltaTime) {
            RouteDrawerState routeDrawer = Find.State<RouteDrawerState>();
            RouteHoverState hoverState = Find.State<RouteHoverState>();
            LiveRoutesState liveState = Find.State<LiveRoutesState>();
            PortPools pools = Find.State<PortPools>();
            SupplyChainSprites sprites = Find.GlobalAsset<SupplyChainSprites>();

            bool pathHighlight = false;// routeDrawer.DrawState == RouteDrawState.InProgress;

            foreach (var state in Find.Components<PortDetailsDisplayState>()) {
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