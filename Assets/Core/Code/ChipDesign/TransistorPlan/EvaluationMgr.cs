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
        }


        private struct CrucialGraphNode
        {
            public string Name;
            public List<CrucialGraphEdge> Edges; // Edges to other CrucialNodes
            public GraphCoord Coord;

            public void Init(int layerIndex, int col, int row)
            {
                Edges = new List<CrucialGraphEdge>();

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
        }

        private struct CrucialGraphEdge
        {
            public GraphNode Other { get; private set; }
            public List<GraphNode> Path { get; private set; }

            public void Init(GraphNode other, List<GraphNode> path)
            {
                Other = other;
                Path = path;
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
            int numCrucialNodes = 0;
            int numCrucialEdges = 0;
            ConstructGraph(out crucialGraph, out completeGraph, out numCrucialNodes, out numCrucialEdges);

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
                    EvaluationFailure();
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

        private void ConstructGraph(out List<CrucialGraphNode> crucialNodes, out List<GraphNode> allNodes, out int numCrucialNodes, out int numCrucialEdges)
        {
            // SETUP

            crucialNodes = new List<CrucialGraphNode>();
            allNodes = new List<GraphNode>();

            Dictionary<GraphCoord, GraphNode> CoordNodeMap = new Dictionary<GraphCoord, GraphNode>();

            // CORE -- CREATE NODES

            var dims = GridStack.Instance.LayerDims;
            for (int layer = 0; layer < GridStack.Instance.GridLayers.Length; layer++)
            {
                for (int row = 0; row < dims.Y; row++)
                {
                    for (int col = 0; col < dims.X; col++)
                    {
                        var cell = GridStack.Instance.GridLayers[layer].GetCell(col, row);
                        if (cell.CellType == CellType.NONE) { continue; }

                        // Inputs, Outputs, and Transistors under Gates are crucial nodes -- gather them on the transistor layer
                        if (layer == GridStack.TRANSISTOR_LAYER)
                        {
                            if ((cell.CellType == CellType.Input || cell.CellType == CellType.Output)
                                || ((cell.CellType == CellType.NTransistor || cell.CellType == CellType.PTransistor) && cell.TransferType == TransferType.Gate))
                            {
                                var crucialNode = new CrucialGraphNode();
                                crucialNode.Init(layer, col, row);

                                crucialNodes.Add(crucialNode);
                            }
                        }

                        var newNode = new GraphNode();
                        newNode.Init(layer, col, row);
                        var coordKey = newNode.Coord;

                        CoordNodeMap.Add(coordKey, newNode);
                    }
                }
            }

            // CORE -- CREATE EDGES

            for (int layer = 0; layer < GridStack.Instance.GridLayers.Length; layer++)
            {
                for (int row = 0; row < dims.Y; row++)
                {
                    for (int col = 0; col < dims.X; col++)
                    {
                        var cell = GridStack.Instance.GridLayers[layer].GetCell(col, row);
                        if (cell.CellType == CellType.NONE) { continue; }

                        var lookupCoord = new GraphCoord(layer, col, row);
                        var dictNode = CoordNodeMap[lookupCoord];

                        for (int dir = 0; dir < 6; dir++)
                        {
                            if (cell.Edges[dir] == EdgeState.Connected)
                            {
                                GridUtility.GetOffsetOfDir((EdgeDir)dir, out Vector2Int gridOffset, out int layerOffset);
                                var adjLookupCoord = new GraphCoord(layer + layerOffset, col + gridOffset.x, row + gridOffset.y);
                                GraphNode adjNode = CoordNodeMap[adjLookupCoord];

                                var newEdge = new GraphEdge();
                                newEdge.Init(adjNode);
                                dictNode.Edges.Add(newEdge);
                            }
                        }

                        CoordNodeMap[lookupCoord] = dictNode;
                    }
                }
            }

            // TODO: CORE -- ASSEMBLE CRUCIAL NODES / EDGES


            // SUMMARY AND RETURN

            numCrucialNodes = crucialNodes.Count;
            numCrucialEdges = 0;
            foreach (var cNode in crucialNodes)
            {
                if (cNode.Edges == null) { continue; }
                numCrucialEdges += cNode.Edges.Count;
            }
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