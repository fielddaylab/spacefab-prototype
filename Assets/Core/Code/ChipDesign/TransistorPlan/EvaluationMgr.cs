using BeauUtil;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public enum FlowState
    {
        Empty,
        Hi,
        Lo,
        Unstable
    }

    public enum EvalResult
    {
        Failure,
        Success,
        CycleDetected,
        MissingNode
    }

    public class EvaluationMgr : MonoBehaviour
    {
        #region Structs

        private struct GraphNode
        {
            public string Name;
            public List<GraphEdge> Edges;
            public GraphCoord Coord;
            public bool Visited;

            public void Init(int layerIndex, int col, int row)
            {
                Edges = new List<GraphEdge>();

                StringBuilder sb = new StringBuilder();
                sb.Append("L");
                sb.Append(layerIndex.ToStringLookup());
                sb.Append("C");
                sb.Append(col.ToStringLookup());
                sb.Append("R");
                sb.Append(row.ToStringLookup());

                Name = sb.ToString();

                Coord = new GraphCoord();
                Coord.Layer = layerIndex;
                Coord.Col = col;
                Coord.Row = row;

                Visited = false;
            }
        }

        private struct GraphEdge
        {
            public GraphNode Other { get; private set; }

            public void Init(GraphNode other)
            {
                Other = other;
            }
        }

        public struct GraphCoord
        {
            public int Layer;
            public int Col;
            public int Row;

            public GraphCoord(int layer, int col, int row)
            {
                Layer = layer;
                Col = col;
                Row = row;
            }

            public static bool operator ==(GraphCoord c1, GraphCoord c2)
            {
                return c1.Equals(c2);
            }

            public static bool operator !=(GraphCoord c1, GraphCoord c2)
            {
                return !c1.Equals(c2);
            }

            public override bool Equals(object obj)
            {
                var other = (GraphCoord)obj;
                return (Layer == other.Layer) && (Col == other.Col) && (Row == other.Row);
            }
        }


        private struct CrucialGraphNode
        {
            public string Name;
            public List<CrucialGraphEdge> Edges; // Edges to other CrucialNodes
            public GraphCoord Coord;
            public int EvalDepth;
            public List<GraphCoord> NoReturnList; // Prevent directed edges toward these nodes

            public void Init(int layerIndex, int col, int row)
            {
                Edges = new List<CrucialGraphEdge>();
                NoReturnList = new List<GraphCoord>();

                StringBuilder sb = new StringBuilder();
                sb.Append("L");
                sb.Append(layerIndex.ToStringLookup());
                sb.Append("C");
                sb.Append(col.ToStringLookup());
                sb.Append("R");
                sb.Append(row.ToStringLookup());

                Name = sb.ToString();

                Coord = new GraphCoord();
                Coord.Layer = layerIndex;
                Coord.Col = col;
                Coord.Row = row;
            }

            public bool ContainsCycle(CrucialGraphEdge toCheck)
            {
                foreach (var existingEdge in Edges)
                {
                    if (existingEdge.Other.Name == toCheck.Other.Name)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private struct CrucialGraphEdge
        {
            public CrucialGraphNode Origin;
            public CrucialGraphNode Other;
            public List<GraphNode> Path;
            public int EvalDepth;
            public bool CycleDetected;

            public void Init(CrucialGraphNode origin, CrucialGraphNode other, List<GraphNode> path, int evalDepth)
            {
                Origin = origin;
                Other = other;
                Path = path;
                EvalDepth = evalDepth;
            }
        }

        #endregion // Structs

        #region Inspector

        public Button EvaluateButton;

        [Header("Results")]
        public GameObject ResultPanel;
        public TMP_Text ResultHeaderText;
        public TMP_Text ResultSubText;
        public Button ResultCloseButton;

        #endregion // Inspector

        #region Unity Callbacks

        private void Awake()
        {
            EvaluateButton.onClick.AddListener(HandleEvaluateClicked);
            ResultCloseButton.onClick.AddListener(HandleResultCloseClicked);

            ResultPanel.SetActive(false);
        }

        #endregion // Unity Callbacks

        #region Helpers

        private unsafe void Evaluate()
        {
            // TODO: Gather nodes and edges
            var crucialGraph = new List<CrucialGraphNode>();
            var completeGraph = new List<GraphNode>();
            var orderedEdges = new List<CrucialGraphEdge>();
            int numCrucialNodes = 0;
            int numCrucialEdges = 0;
            ConstructGraph(out crucialGraph, out completeGraph, out numCrucialNodes, out numCrucialEdges, out orderedEdges);

            #region CONVERT TOPOLOGICAL 

            // Convert to topological map
            DependencySolver.Node<StringHash32>* nodes = stackalloc DependencySolver.Node<StringHash32>[numCrucialNodes];
            DependencySolver.Edge<StringHash32>* edges = stackalloc DependencySolver.Edge<StringHash32>[numCrucialNodes];

            for (int i = 0; i < numCrucialNodes; i++)
            {
                nodes[i].Id = crucialGraph[i].Name;

                if (crucialGraph[i].Edges.Count != 0)
                {
                    for (int e = 0; e < crucialGraph[i].Edges.Count; e++)
                    {
                        edges[i + e].Endpoint = crucialGraph[i].Edges[e].Other.Name;
                    }
                    nodes[i].Edges = new OffsetLengthU16((ushort)i, (ushort)crucialGraph[i].Edges.Count);
                }
                else
                {
                    nodes[i].Edges = default;
                }
            }

            #endregion // CONVERT TOPOLOGICAL 

            EvalResult evalResult = EvalResult.Failure;

            #region SOLVE TOPOLOGICAL

            if (numCrucialNodes > 0)
            {
                DependencySolver.OutputNode<StringHash32>* outputNodes = stackalloc DependencySolver.OutputNode<StringHash32>[numCrucialNodes];
                DependencySolver.Result result = DependencySolver.Solve<StringHash32>(new UnsafeSpan<DependencySolver.Node<StringHash32>>(nodes, numCrucialNodes), new UnsafeSpan<DependencySolver.Edge<StringHash32>>(edges, numCrucialEdges), new UnsafeSpan<DependencySolver.OutputNode<StringHash32>>(outputNodes, numCrucialNodes));

                // Convert result into EvalResult
                switch (result)
                {
                    case DependencySolver.Result.Success:
                        evalResult = EvalResult.Success;
                        break;
                    case DependencySolver.Result.CycleDetected:
                        evalResult = EvalResult.CycleDetected;
                        break;
                    case DependencySolver.Result.MissingNode:
                        evalResult = EvalResult.MissingNode;
                        break;
                    default:
                        break;
                }


                for (int i = 0; i < numCrucialNodes; i++)
                {
                    Debug.Log("[EvaluationMgr] node at " + i + " : " + outputNodes[i].Id.ToDebugString() + " at " + outputNodes[i].OriginalIndex);
                }
            }

            #endregion // SOLVE TOPOLOGICAL


            // Handle result

            switch (evalResult)
            {
                case EvalResult.Failure:
                    EvaluationFailure();
                    break;
                case EvalResult.CycleDetected:
                    EvaluationInvalid();
                    break;
                case EvalResult.MissingNode:
                    EvaluationInvalid();
                    break;
                case EvalResult.Success:
                    EvaluationSuccess();
                    break;
                default:
                    break;
            }
        }

        private void EvaluationSuccess()
        {
            ResultHeaderText.SetText("Success");
            ResultPanel.SetActive(true);

            Game.Events.Dispatch(GameEvents.OnResultsDisplayed);
        }

        private void EvaluationInvalid()
        {
            ResultHeaderText.SetText("Invalid");
            ResultPanel.SetActive(true);

            Game.Events.Dispatch(GameEvents.OnResultsDisplayed);
        }

        private void EvaluationFailure()
        {
            ResultHeaderText.SetText("Failure");
            ResultPanel.SetActive(true);

            Game.Events.Dispatch(GameEvents.OnResultsDisplayed);
        }

        private void ConstructGraph(out List<CrucialGraphNode> crucialNodes, out List<GraphNode> allNodes, out int numCrucialNodes, out int numCrucialEdges, out List<CrucialGraphEdge> orderedEdgeProcessList)
        {
            // SETUP
            crucialNodes = new List<CrucialGraphNode>();
            allNodes = new List<GraphNode>();
            Dictionary<GraphCoord, GraphNode> coordNodeMap = new Dictionary<GraphCoord, GraphNode>();
            Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap = new Dictionary<GraphCoord, CrucialGraphNode>();

            // CORE -- CREATE NODES
            List<CrucialGraphNode> nodeWorkList = new List<CrucialGraphNode>();
            GraphConstructNodes(ref allNodes, ref crucialNodes, ref nodeWorkList, ref coordNodeMap, ref crucialCoordNodeMap);

            // CORE -- CREATE EDGES
            orderedEdgeProcessList = new List<CrucialGraphEdge>();
            GraphConstructEdges(ref coordNodeMap);

            // CORE -- ASSEMBLE CRUCIAL NODES / EDGES
            SetAllNodesAllPaths(ref coordNodeMap, ref crucialCoordNodeMap, ref nodeWorkList, ref orderedEdgeProcessList);

            // SUMMARIZE AND RETURN

            numCrucialNodes = crucialNodes.Count;
            numCrucialEdges = 0;
            foreach (var cNode in crucialNodes)
            {
                if (cNode.Edges == null) { continue; }
                numCrucialEdges += cNode.Edges.Count;
            }
        }

        private void GraphConstructNodes(ref List<GraphNode> allNodes, ref List<CrucialGraphNode> crucialNodes, ref List<CrucialGraphNode> startingCrucialNodes, ref Dictionary<GraphCoord, GraphNode> coordNodeMap, ref Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap)
        {
            var dims = GridStack.Instance.LayerDims;
            for (int layer = 0; layer < GridStack.Instance.GridLayers.Length; layer++)
            {
                for (int row = 0; row < dims.Y; row++)
                {
                    for (int col = 0; col < dims.X; col++)
                    {
                        var cell = GridStack.Instance.GridLayers[layer].GetCell(col, row);
                        if (cell.CellType == CellType.NONE) { continue; }

                        // Inputs, Outputs, and Gates are crucial nodes -- gather them
                        if ((cell.CellType == CellType.Input || cell.CellType == CellType.Output)
                            || (cell.TransferType == TransferType.GateAbove) || (cell.TransferType == TransferType.GateBelow))
                        {
                            var crucialNode = new CrucialGraphNode();
                            crucialNode.Init(layer, col, row);

                            crucialNodes.Add(crucialNode);

                            var crucialCoordKey = crucialNode.Coord;
                            crucialCoordNodeMap.Add(crucialCoordKey, crucialNode);

                            if (cell.CellType == CellType.Input)
                            {
                                startingCrucialNodes.Add(crucialNode);
                            }
                        }

                        var newNode = new GraphNode();
                        newNode.Init(layer, col, row);
                        allNodes.Add(newNode);
                        var coordKey = newNode.Coord;
                        coordNodeMap.Add(coordKey, newNode);
                    }
                }
            }
        }

        private void GraphConstructEdges(ref Dictionary<GraphCoord, GraphNode> coordNodeMap)
        {
            var dims = GridStack.Instance.LayerDims;
            for (int layer = 0; layer < GridStack.Instance.GridLayers.Length; layer++)
            {
                for (int row = 0; row < dims.Y; row++)
                {
                    for (int col = 0; col < dims.X; col++)
                    {
                        var cell = GridStack.Instance.GridLayers[layer].GetCell(col, row);
                        if (cell.CellType == CellType.NONE) { continue; }

                        var lookupCoord = new GraphCoord(layer, col, row);
                        var dictNode = coordNodeMap[lookupCoord];

                        for (int dir = 0; dir < 6; dir++)
                        {
                            if (cell.Edges[dir] == EdgeState.Connected)
                            {
                                GridUtility.GetOffsetOfDir((EdgeDir)dir, out Vector2Int gridOffset, out int layerOffset);
                                var adjLookupCoord = new GraphCoord(layer + layerOffset, col + gridOffset.x, row + gridOffset.y);
                                GraphNode adjNode = coordNodeMap[adjLookupCoord];

                                var newEdge = new GraphEdge();
                                newEdge.Init(adjNode);
                                dictNode.Edges.Add(newEdge);
                            }
                        }

                        coordNodeMap[lookupCoord] = dictNode;
                    }
                }
            }
        }

        private void SetAllNodesAllPaths(ref Dictionary<GraphCoord, GraphNode> coordNodeMap, ref Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap, ref List<CrucialGraphNode> nodeWorkList, ref List<CrucialGraphEdge> orderedEdgeProcessList)
        {
            // reset visited to false for all nodes
            ResetAllVisited(ref coordNodeMap);

            // Follow BFS -- Start with Inputs, then append newly found nodes
            int currDepth = 0;
            while (nodeWorkList.Count > 0)
            {
                var currCrucialNode = nodeWorkList[0];
                if (currCrucialNode.EvalDepth != currDepth)
                {
                    ResetAllVisited(ref coordNodeMap);
                    currDepth = currCrucialNode.EvalDepth;
                }

                // "find all paths to all nodes" from starting node:

                // lookup starting node in dict
                var startingNode = coordNodeMap[currCrucialNode.Coord];
                // perform DFS on each edge
                foreach (var origEdge in startingNode.Edges)
                {
                    // init path from this edge
                    var accumulatedPath = new List<GraphNode>();
                    var accumulatedCrucialNodes = new List<CrucialGraphNode>();
                    accumulatedPath.Add(startingNode);
                    // keep track of the path of nodes
                    SetAllNodesAllPathsRecursive(currCrucialNode.Coord, origEdge.Other.Coord, ref coordNodeMap, ref crucialCoordNodeMap, ref accumulatedPath, ref accumulatedCrucialNodes, currDepth);

                    // For each found node along this edge, set crucial edge path to the accumulated edge path
                    var cNode = crucialCoordNodeMap[currCrucialNode.Coord];

                    for (int i = 0; i < accumulatedCrucialNodes.Count; i++)
                    {
                        var newCrucialEdge = new CrucialGraphEdge();
                        newCrucialEdge.Init(cNode, accumulatedCrucialNodes[i], accumulatedPath, currDepth);

                        if (!cNode.ContainsCycle(newCrucialEdge))
                        {
                            var transferType = GridStack.Instance.GetCellDirect(accumulatedCrucialNodes[i].Coord).TransferType;
                            if (transferType == TransferType.GateBelow)
                            {
                                // do not evaluate until gate dependency is evaluated
                            }
                            else if (transferType == TransferType.GateAbove)
                            {
                                nodeWorkList.Add(accumulatedCrucialNodes[i]);

                                // underlying gate is ready to be evaluated
                                var belowCoord = accumulatedCrucialNodes[i].Coord;
                                belowCoord.Layer = GridStack.TRANSISTOR_LAYER;
                                nodeWorkList.Add(crucialCoordNodeMap[belowCoord]);
                            }
                            else
                            {
                                nodeWorkList.Add(accumulatedCrucialNodes[i]);
                            }
                        }
                        else
                        {
                            newCrucialEdge.CycleDetected = true;
                        }

                        cNode.Edges.Add(newCrucialEdge);
                        orderedEdgeProcessList.Add(newCrucialEdge);
                    }

                    crucialCoordNodeMap[currCrucialNode.Coord] = cNode;
                }

                nodeWorkList.RemoveAt(0);
            }
        }

        private bool ContainsByName(List<CrucialGraphNode> checkInList, CrucialGraphNode lookup)
        {
            for (int i = 0; i < checkInList.Count; i++)
            {
                if (checkInList[i].Name == lookup.Name) { return true; }
            }

            return false;
        }

        private void SetAllNodesAllPathsRecursive(GraphCoord originCoord, GraphCoord currCoord, ref Dictionary<GraphCoord, GraphNode> coordNodeMap, ref Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap, ref List<GraphNode> accumulatedPath, ref List<CrucialGraphNode> accumulatedCrucialNodes, int parentDepth)
        {
            var currNode = coordNodeMap[currCoord];
            if (currNode.Visited) { return; }

            accumulatedPath.Add(currNode);

            // mark visited
            currNode.Visited = true;

            coordNodeMap[currCoord] = currNode;

            if (crucialCoordNodeMap.ContainsKey(currCoord))
            {
                // Whenever reaching a crucial node (that is not this original node):
                if (currNode.Coord != originCoord)
                {
                    // Do not allow edges back to the nodes that added this originNode to the work list
                    if (!crucialCoordNodeMap[originCoord].NoReturnList.Contains(currCoord))
                    {
                        var cNode = crucialCoordNodeMap[currCoord];

                        // track it as a connected node along crucial edge
                        if (!ContainsByName(accumulatedCrucialNodes, crucialCoordNodeMap[currCoord]))
                        {
                            cNode.EvalDepth = parentDepth + 1;
                            cNode.NoReturnList.Add(originCoord);
                            crucialCoordNodeMap[currCoord] = cNode;
                            accumulatedCrucialNodes.Add(crucialCoordNodeMap[currCoord]);
                        }
                    }
                }
            }
            else
            {
                // continue recursion
                foreach (var edge in currNode.Edges)
                {
                    SetAllNodesAllPathsRecursive(originCoord, edge.Other.Coord, ref coordNodeMap, ref crucialCoordNodeMap, ref accumulatedPath, ref accumulatedCrucialNodes, parentDepth);
                }
            }
        }

        private void ResetAllVisited(ref Dictionary<GraphCoord, GraphNode> coordNodeMap)
        {
            var dims = GridStack.Instance.LayerDims;
            for (int layer = 0; layer < GridStack.Instance.GridLayers.Length; layer++)
            {
                for (int row = 0; row < dims.Y; row++)
                {
                    for (int col = 0; col < dims.X; col++)
                    {
                        var lookupCoord = new GraphCoord(layer, col, row);
                        SetVisited(false, lookupCoord, ref coordNodeMap);
                    }
                }
            }
        }

        private void SetVisited(bool visited, GraphCoord lookupCoord, ref Dictionary<GraphCoord, GraphNode> coordNodeMap)
        {
            if (!coordNodeMap.ContainsKey(lookupCoord)) { return; }

            var node = coordNodeMap[lookupCoord];
            node.Visited = visited;
            coordNodeMap[lookupCoord] = node;
        }

        #endregion // Helpers

        #region Handlers

        private void HandleEvaluateClicked()
        {
            Evaluate();
        }

        private void HandleResultCloseClicked()
        {
            ResultPanel.SetActive(false);

            Game.Events.Dispatch(GameEvents.OnResultsHidden);
        }

        #endregion // Handlers
    }
}