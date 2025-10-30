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

            public void SetName(int layerIndex, int col, int row)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("L");
                sb.Append(layerIndex.ToStringLookup());
                sb.Append("C");
                sb.Append(col.ToStringLookup());
                sb.Append("R");
                sb.Append(row.ToStringLookup());

                Name = sb.ToString();
            }
        }

        private struct GraphEdge
        {
            public GraphNode Other;
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
            var graph = new List<GraphNode>();
            int numNodes = 0;
            int numEdges = 0;
            ConstructGraph(out graph, out numNodes, out numEdges);

            #region CONVERT TOPOLOGICAL 

            // Convert to topological map
            DependencySolver.Node<StringHash32>* nodes = stackalloc DependencySolver.Node<StringHash32>[numNodes];
            DependencySolver.Edge<StringHash32>* edges = stackalloc DependencySolver.Edge<StringHash32>[numEdges];

            for (int i = 0; i < numNodes; i++)
            {
                nodes[i].Id = graph[i].Name;

                if (graph[i].Edges.Count != 0)
                {
                    for (int e = 0; e < graph[i].Edges.Count; e++)
                    {
                        edges[i + e].Endpoint = graph[i].Edges[e].Other.Name;
                    }
                    nodes[i].Edges = new OffsetLengthU16((ushort)i, (ushort)graph[i].Edges.Count);
                }
                else
                {
                    nodes[i].Edges = default;
                }
            }

            #endregion // CONVERT TOPOLOGICAL 

            EvalResult evalResult = EvalResult.Failure;

            #region SOLVE TOPOLOGICAL

            if (numNodes > 0)
            {
                DependencySolver.OutputNode<StringHash32>* outputNodes = stackalloc DependencySolver.OutputNode<StringHash32>[numNodes];
                DependencySolver.Result result = DependencySolver.Solve<StringHash32>(new UnsafeSpan<DependencySolver.Node<StringHash32>>(nodes, numNodes), new UnsafeSpan<DependencySolver.Edge<StringHash32>>(edges, numEdges), new UnsafeSpan<DependencySolver.OutputNode<StringHash32>>(outputNodes, numNodes));

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

        private void ConstructGraph(out List<GraphNode> nodes, out int numNodes, out int numEdges)
        {
            nodes = new List<GraphNode>();


            numNodes = nodes.Count;
            numEdges = 0;
            foreach (var node in nodes)
            {
                if (node.Edges == null) { continue; }
                numEdges += node.Edges.Count;
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