using System.Collections.Generic;
using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class LiveRouteData {
        public const int MaxNodes = 32;
        public const int MaxPorts = 16;

        public StringHash32 ShipId;
        public SupplyRouteStats Stats;
        public Color32 LineColor;

        public int NodeCount;
        public int PortCount;

        public RouteLineRenderer Line;
        public readonly PathNode[] Nodes = new PathNode[MaxNodes];
        public readonly Port[] Ports = new Port[MaxPorts];
    }

    static public partial class LiveRouteUtility {
        #region Nodes

        static public bool IsMostRecentNode(LiveRouteData liveRoute, PathNode node) {
            return liveRoute.NodeCount > 0 && liveRoute.Nodes[liveRoute.NodeCount - 1] == node;
        }

        static public bool IsNodeInPath(LiveRouteData liveRoute, PathNode node) {
            for(int i = 0; i < liveRoute.NodeCount; i++) {
                if (ReferenceEquals(liveRoute.Nodes[i], node)) {
                    return true;
                }
            }
            return false;
        }

        static public bool TryAddNode(LiveRouteData liveRoute, PathNode node) {
            if (liveRoute.NodeCount >= LiveRouteData.MaxNodes) {
                return false;
            }

            for (int i = 0; i < liveRoute.NodeCount; i++) {
                if (ReferenceEquals(liveRoute.Nodes[i], node)) {
                    Assert.True((node.Flags & PathNodeFlags.IsTemporary) == 0, "Attempting to add temp node multiple times!");
                    return false;
                }
            }

            liveRoute.Nodes[liveRoute.NodeCount++] = node;
            SetNodeOwner(node, liveRoute);
            LiveRouteLineUtility.AddSolid(liveRoute.Line, node.transform.position);
            LiveRouteLineUtility.UpdateTail(liveRoute.Line);
            LiveRouteLineUtility.RegenerateColliders(liveRoute.Line);
            return true;
        }

        static public PathNode PopNode(LiveRouteData liveRoute) {
            Assert.True(liveRoute.NodeCount > 0);

            int idx = liveRoute.NodeCount - 1;
            PathNode node = liveRoute.Nodes[idx];
            liveRoute.Nodes[idx] = null;
            liveRoute.NodeCount--;
            SetNodeOwner(node, null);
            LiveRouteLineUtility.PopSolid(liveRoute.Line);
            LiveRouteLineUtility.UpdateTail(liveRoute.Line);
            LiveRouteLineUtility.RegenerateColliders(liveRoute.Line);
            return node;
        }

        static public void SetNodeOwner(PathNode node, LiveRouteData route) {
            if (node.Highlight != null) {
                if (route != null) {
                    node.Highlight.PathHighlight.enabled = true;
                    node.Highlight.PathHighlight.color = route.LineColor;
                } else {
                    node.Highlight.PathHighlight.enabled = false;
                }
            }
        }

        #endregion // Nodes

        #region Ports

        static public bool IsPortSelected(LiveRouteData liveRoute, Port port) {
            for (int i = 0; i < liveRoute.PortCount; i++) {
                if (ReferenceEquals(liveRoute.Ports[i], port)) {
                    return true;
                }
            }
            return false;
        }

        static public bool TryAddPort(LiveRouteData liveRoute, Port port) {
            if (liveRoute.PortCount >= LiveRouteData.MaxPorts) {
                return false;
            }

            LiveRoutesState state = Find.State<LiveRoutesState>();
            Assert.True(!state.UsedPorts.Contains(port));
            state.UsedPorts.Add(port);

            liveRoute.Ports[liveRoute.PortCount++] = port;
            return true;
        }

        static public void RemovePort(LiveRouteData liveRoute, Port port) {
            LiveRoutesState state = Find.State<LiveRoutesState>();
            Assert.True(!state.UsedPorts.Contains(port));
            state.UsedPorts.Remove(port);

            int portIndex = -1;
            for (int i = 0; i < liveRoute.PortCount; i++) {
                if (ReferenceEquals(liveRoute.Ports[i], port)) {
                    portIndex = i;
                    break;
                }
            }

            Assert.True(portIndex >= 0, "Port not in route");
            ArrayUtils.FastRemoveAt(liveRoute.Ports, ref liveRoute.PortCount, portIndex);
        }

        static public int RemovePortsForNode(LiveRouteData liveRoute, PathNode node, ICollection<Port> removed) {
            LiveRoutesState state = Find.State<LiveRoutesState>();
            int count = 0;
            for (int i = liveRoute.PortCount; i-- > 0;) {
                Port port = liveRoute.Ports[i];
                if (ReferenceEquals(port.ParentNode, node)) {
                    state.UsedPorts.Remove(port);
                    ArrayUtils.FastRemoveAt(liveRoute.Ports, ref liveRoute.PortCount, i);
                    removed.Add(port);
                    count++;
                }
            }
            return count;
        }

        #endregion // Ports
    }
}