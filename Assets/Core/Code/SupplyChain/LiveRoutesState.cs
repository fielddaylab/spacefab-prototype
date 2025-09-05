using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;

namespace SpaceFab.SupplyChain {
    public sealed class LiveRoutesState : SharedStateComponent, IRegistrationCallbacks {
        public const int MaxShips = 4;

        public RouteLineRenderer[] RouteLines;
        
        [NonSerialized] public LiveRouteData[] Routes = new LiveRouteData[MaxShips];
        [NonSerialized] public int RouteCount;
        [NonSerialized] public HashSet<Port> UsedPorts = SetUtils.Create<Port>(16);


        void IRegistrationCallbacks.OnDeregister() {
        }

        void IRegistrationCallbacks.OnRegister() {
            for (int i = 0; i < Routes.Length; i++) {
                Routes[i] = new LiveRouteData();
                Routes[i].Line = RouteLines[i];
            }
        }
    }

    static public partial class LiveRouteUtility {
        static public bool HasLiveRoute(StringHash32 shipId) {
            LiveRoutesState state = Find.State<LiveRoutesState>();
            for(int i = 0; i < state.RouteCount; i++) {
                if (state.Routes[i].ShipId == shipId) {
                    return true;
                }
            }

            return false;
        }

        static public bool HasLiveRoute(StringHash32 shipId, out LiveRouteData liveRoute) {
            LiveRoutesState state = Find.State<LiveRoutesState>();
            for (int i = 0; i < state.RouteCount; i++) {
                if (state.Routes[i].ShipId == shipId) {
                    liveRoute = state.Routes[i];
                    return true;
                }
            }

            liveRoute = null;
            return false;
        }

        static public LiveRouteData GetLiveRoute(StringHash32 shipId) {
            LiveRoutesState state = Find.State<LiveRoutesState>();
            for (int i = 0; i < state.RouteCount; i++) {
                if (state.Routes[i].ShipId == shipId) {
                    return state.Routes[i];
                }
            }

            Assert.True(state.RouteCount < LiveRoutesState.MaxShips, "Exceeded maximum number of ships allowed");
            LiveRouteData route = state.Routes[state.RouteCount++];
            route.ShipId = shipId;
            return route;
        }
    
        static public void ClearLiveRoute(LiveRouteData route) {
            LiveRoutesState state = Find.State<LiveRoutesState>();

            for(int i = 0; i < route.NodeCount; i++) {
                
            }

            for(int i = 0; i < route.PortCount; i++) {
                state.UsedPorts.Remove(route.Ports[i]);
                route.Ports[i] = null;
            }

            route.Line.gameObject.SetActive(false);

            route.PortCount = route.NodeCount = 0;
        }
    }
}