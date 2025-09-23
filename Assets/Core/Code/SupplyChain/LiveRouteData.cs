using System;
using System.Collections.Generic;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Physics;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class LiveRouteData {
        public const int MaxNodes = 32;
        public const int MaxPorts = 16;
        public const int MaxHazards = 16;

        public StringHash32 ShipId;
        public SupplyRouteStats Stats;
        public Color32 LineColor;

        public int NodeCount;
        public int PortCount;
        public int HazardCount;

        public RouteLineRenderer Line;
        public readonly PathNode[] Nodes = new PathNode[MaxNodes];
        public readonly Port[] Ports = new Port[MaxPorts];
        public readonly HazardRegion[] IntersectingHazards = new HazardRegion[MaxHazards];
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
            UpdateStats(liveRoute);
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
            RemovePortsForNodeImpl(liveRoute, node, null);
            UpdateStats(liveRoute);
            return node;
        }

        static public void SetNodeOwner(PathNode node, LiveRouteData route) {
            if (node.Highlight != null) {
                if (route != null) {
                    node.Highlight.PathHighlight.enabled = true;
                    node.Highlight.PathHighlight.color = route.LineColor;
                    node.Highlight.HasPath = true;
                } else {
                    node.Highlight.PathHighlight.enabled = false;
                    node.Highlight.HasPath = false;
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
            SetPortOwner(port, liveRoute);
            UpdateStats(liveRoute);
            return true;
        }

        static public int CountPortsOfType(LiveRouteData liveRoute, PortType type) {
            int count = 0;
            for(int i = 0; i < liveRoute.PortCount; i++) {
                if (liveRoute.Ports[i].Type == type) {
                    count++;
                }
            }
            return count;
        }

        static public void RemovePort(LiveRouteData liveRoute, Port port) {
            LiveRoutesState state = Find.State<LiveRoutesState>();
            Assert.True(state.UsedPorts.Contains(port));
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
            SetPortOwner(port, null);
            UpdateStats(liveRoute);
        }

        static public int RemovePortsForNode(LiveRouteData liveRoute, PathNode node, ICollection<Port> removed) {
            int count = RemovePortsForNodeImpl(liveRoute, node, removed);
            if (count > 0) {
                UpdateStats(liveRoute);
            }
            return count;
        }

        static private int RemovePortsForNodeImpl(LiveRouteData liveRoute, PathNode node, ICollection<Port> removed) {
            LiveRoutesState state = Find.State<LiveRoutesState>();
            int count = 0;
            for (int i = liveRoute.PortCount; i-- > 0;) {
                Port port = liveRoute.Ports[i];
                if (ReferenceEquals(port.ParentNode, node)) {
                    state.UsedPorts.Remove(port);
                    SetPortOwner(port, null);
                    ArrayUtils.FastRemoveAt(liveRoute.Ports, ref liveRoute.PortCount, i);
                    removed?.Add(port);
                    count++;
                }
            }
            return count;
        }

        static public void SetPortOwner(Port port, LiveRouteData route) {
            port.Owner = route;
            if (port.Visuals != null) {
                if (route != null) {
                    port.Visuals.Outline.enabled = true;
                    port.Visuals.Outline.color = route.LineColor;
                    if (port.Visuals.CurrentDetails) {
                        port.Visuals.CurrentDetails.Outline.enabled = true;
                        port.Visuals.CurrentDetails.Outline.color = route.LineColor;
                    }
                } else {
                    port.Visuals.Outline.enabled = false;
                    if (port.Visuals.CurrentDetails) {
                        port.Visuals.CurrentDetails.Outline.enabled = false;
                    }
                }
            }
        }

        #endregion // Ports

        #region Hazards

        static public void SetHazardOwner(HazardRegion hazard, LiveRouteData route) {
            if (hazard.Highlight != null) {
                if (route != null) {
                    hazard.Highlight.PathHighlight.enabled = true;
                    hazard.Highlight.PathHighlight.color = route.LineColor;
                    hazard.Highlight.HasPath = true;
                } else {
                    hazard.Highlight.PathHighlight.enabled = false;
                    hazard.Highlight.HasPath = false;
                }
            }
        }

        #endregion // Hazards

        static private readonly RaycastHit2D[] RaycastBufferA = new RaycastHit2D[8];
        static private readonly RaycastHit2D[] RaycastBufferB = new RaycastHit2D[8];

        static public unsafe void UpdateStats(LiveRouteData route) {
            RouteShip ship = Find.NamedAsset<RouteShip>(route.ShipId);
            SupplyChainMath mathSettings = Find.GlobalAsset<SupplyChainMath>();
            bool isCurrentRoute = Find.State<RouteShipSelectionState>().SelectedRoute == route;

            for (int i = route.HazardCount; i-- > 0;) {
                if (isCurrentRoute) {
                    SetHazardOwner(route.IntersectingHazards[i], null);
                }
                route.IntersectingHazards[i] = null;
            }
            route.HazardCount = 0;

            float distance = 0;
            float* hazardDistances = stackalloc float[3];
            for(int i = 1; i <= route.NodeCount; i++) {
                PathNode nodeA = route.Nodes[i - 1];
                PathNode nodeB = route.Nodes[i % route.NodeCount];

                Vector2 nodeAPos = nodeA.transform.position;
                Vector2 nodeBPos = nodeB.transform.position;

                float segDist = Vector2.Distance(nodeAPos, nodeBPos);
                distance += segDist;

                // TODO: handle concave regions?
                int enterIntersections = Physics2D.LinecastNonAlloc(nodeAPos, nodeBPos, RaycastBufferA, LayerMasks.SupplyRegion_Mask);
                if (enterIntersections > 0) {
                    int exitIntersections = Physics2D.LinecastNonAlloc(nodeBPos, nodeAPos, RaycastBufferB, LayerMasks.SupplyRegion_Mask);
                    for(int hitIdx = 0; hitIdx < enterIntersections; hitIdx++) {
                        RaycastHit2D hitA = RaycastBufferA[hitIdx];
                        HazardRegion hazard = hitA.ResolveComponent<HazardRegion>();
                        if (!hazard) {
                            continue;
                        }

                        float hazardInSeg = segDist - hitA.distance;
                        int exitIdx = TryFindMatchingExit(hitA, RaycastBufferB, exitIntersections);
                        if (exitIdx >= 0) {
                            RaycastHit2D hitB = RaycastBufferB[exitIdx];
                            hazardInSeg = hazardInSeg - hitB.distance;
                        }

                        hazardDistances[(int)hazard.Type] += hazardInSeg;

                        bool hazardInRoute = false;
                        for(int hazardIndex = 0; hazardIndex < route.HazardCount; hazardIndex++) {
                            if (route.IntersectingHazards[hazardIndex] == hazard) {
                                hazardInRoute = true;
                                break;
                            }
                        }

                        if (!hazardInRoute) {
                            Assert.True(route.HazardCount < LiveRouteData.MaxHazards);
                            route.IntersectingHazards[route.HazardCount++] = hazard;
                        }
                    }
                }
            }

            float speed = mathSettings.Speeds[ship.Speed - 1];
            float dialationFactor = mathSettings.TimeDialationSpeedFactor;

            float timeDialatedDistance = hazardDistances[(int)HazardType.TimeDialation];
            float totalTime = (distance - timeDialatedDistance) / speed
                + (timeDialatedDistance) / (speed * dialationFactor);

            float cost = mathSettings.Costs[ship.Cost - 1] * (int) Math.Ceiling(distance / speed);
            int cycles = (int) Math.Ceiling(totalTime);

            int* materials = stackalloc int[SupplyUtility.MaterialTypeCount];

            BitSet32 conversionNodes = default;

            double reliability = 1;
            for (int i = 0; i < route.PortCount; i++) {
                Port port = route.Ports[i];
                RouteNode node = port.GetComponent<RouteNode>();

                if (port.Type == PortType.Supply) {
                    SupplyNode supplyNode = port.GetComponent<SupplyNode>();
                    materials[(int)supplyNode.Material - 1]++;
                    cost += (int)node.Cost;
                    cycles = Math.Max((int)node.ProductionTime, cycles);
                    reliability *= mathSettings.Reliabilities[node.Reliability];
                } else if (port.Type == PortType.Conversion) {
                    conversionNodes.Set(i);
                }
            }

            bool converted = false;
            do {
                converted = false;
                foreach (var bit in conversionNodes) {
                    Port port = route.Ports[bit];
                    RouteNode node = port.GetComponent<RouteNode>();
                    ConversionNode conversion = port.GetComponent<ConversionNode>();
                    if (materials[(int)conversion.Input - 1] > 0) {
                        materials[(int)conversion.Input - 1]--;
                        materials[(int)conversion.Output - 1]++;
                        cost += (int)node.Cost;
                        cycles = Math.Max((int)node.ProductionTime, cycles);
                        reliability *= mathSettings.Reliabilities[node.Reliability];
                        converted = true;
                        conversionNodes.Unset(bit);
                    }
                }
            } while (converted && !conversionNodes.IsEmpty);

            float riskyDistance = hazardDistances[(int)HazardType.Risky];
            if (riskyDistance > 0) {
                reliability *= (float)(Math.Exp(-mathSettings.RiskyMultiplierPerUnit * riskyDistance / mathSettings.RiskyMultiplierUnitDist));
            }

            if (hazardDistances[(int)HazardType.Tariff] > 0) {
                for (int i = 0; i < route.HazardCount; i++) {
                    HazardRegion hazard = route.IntersectingHazards[i];
                    if (hazard.Type == HazardType.Tariff) {
                        cost += hazard.TariffCost;
                    }
                }
            }

            reliability *= mathSettings.ShipReliabilities[ship.Defense - 1];

            if (isCurrentRoute) {
                for (int i = 0; i < route.HazardCount; i++) {
                    SetHazardOwner(route.IntersectingHazards[i], route);
                }
            }

            SupplyRouteStats stats;
            stats.Time = (byte) cycles;
            stats.Cost = (uint) cost;
            stats.Reliability = (byte)(SupplyUtility.MaxReliability * reliability);
            for(int i = 0; i < SupplyUtility.MaterialTypeCount; i++) {
                stats.Materials[i] = (byte) materials[i];
            }

            route.Stats = stats;

            SpaceFabGame.Events.Queue(SupplyChainGame.Events.RouteStatsUpdated, route.ShipId);

            using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                psb.Builder.Append("[LiveRouteUtility] Updated stats for route ").Append(route.ShipId.ToDebugString())
                    .Append("\n   Distance: ").AppendNoAlloc(distance, 2)
                    .Append("\n   Distance (Risky Hazard): ").AppendNoAlloc(hazardDistances[(int)HazardType.Risky], 2)
                    .Append("\n   Distance (Tariff): ").AppendNoAlloc(hazardDistances[(int)HazardType.Tariff], 2)
                    .Append("\n   Distance (Time Dialation): ").AppendNoAlloc(hazardDistances[(int)HazardType.TimeDialation], 2)
                    .Append("\n   Cost: $").AppendNoAlloc(stats.Cost)
                    .Append("\n   Time: ").AppendNoAlloc(stats.Time).Append(" cycles")
                    .Append("\n   Reliability: ").AppendNoAlloc((int) (reliability * 100)).Append("%");
                Log.Msg(psb.Builder.Flush());
            }
        }

        static private int TryFindMatchingExit(RaycastHit2D hitA, RaycastHit2D[] buffer, int count) {
            Collider2D check = hitA.collider;
            for(int i = count; i-- > 0;) {
                RaycastHit2D hit = buffer[i];
                if (hit.collider == check) {
                    return i;
                }
            }
            return -1;
        }
    }
}