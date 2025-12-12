using BeauRoutine;
using BeauUtil;
using FieldDay;
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
        Unstable,
    }

    public enum EvalResult
    {
        Failure,
        Success,
        CycleDetected,
        MissingNode
    }

    public struct EvalResultIndexer
    {
        public int RowIndex;
        public int ColIndex;
        public bool Success;
        public FlowState FlowResult;
    }

    public class EvaluationMgr : MonoBehaviour
    {
        public static EvaluationMgr Instance;

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
            public bool AwaitingDependency;
            public bool EvaluatedForDependency;
            public List<GraphCoord> NoReturnList; // Prevent directed edges toward these nodes

            public FlowState CurrFlowState;

            public CellType TempTransformedType;

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
                /*
                foreach (var existingEdge in Edges)
                {
                    if (existingEdge.Other.Name == toCheck.Other.Name)
                    {
                        return true;
                    }
                }
                */
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

        private struct CNodeDependency
        {
            public GraphCoord NodeCoord;
            public GraphCoord DependencyCoord;

            public CNodeDependency(GraphCoord node, GraphCoord dep)
            {
                NodeCoord = node;
                DependencyCoord = dep;
            }
        }

        #endregion // Structs

        #region Inspector

        public Button EvaluateButton;

        public TMP_Text NodeReadout;

        [Header("Results")]
        public GameObject ResultPanel;
        public TMP_Text ResultHeaderText;
        public TMP_Text ResultSubText;
        public Button ResultCloseButton;
        public TMP_Text UnstableText;
        public RectTransform ResultRect;

        [Header("Prefabs")]
        public GameObject RowPrefab;
        public GameObject HeaderPrefab;
        public GameObject ContentsPrefab;
        public GameObject CellEvalPrefab;

        [Header("Spacing")]
        public float DefaultCellWidth;
        public float OutputCellWidth;
        public Transform RowContainer;
        public float HeaderHeight;
        public float RowHeight;
        public VerticalLayoutGroup VertLayout;

        #endregion // Inspector

        [HideInInspector] public bool IsUnstable;

        private Dictionary<Tuple<int, int>, SuiteCellEval> m_evalMap = new Dictionary<Tuple<int, int>, SuiteCellEval>();
        private List<SuiteCellEval> m_allEvals = new List<SuiteCellEval>();

        private Dictionary<Tuple<int, int>, SuiteContents> m_contentsMap = new Dictionary<Tuple<int, int>, SuiteContents>();
        private List<SuiteContents> m_allSuiteContents = new List<SuiteContents>();

        private Routine m_EvaluationRoutine;

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;

            EvaluateButton.onClick.AddListener(HandleEvaluateClicked);
            ResultCloseButton.onClick.AddListener(HandleResultCloseClicked);

            ResultPanel.SetActive(false);
        }

        private void Start()
        {
            // Construct Test Suite Table
            var suite = LevelMgr.Instance.CurrLevelData.GetTestSuite();
            ConstructSuiteTable(suite);
            ClearSuiteEvals();

            UnstableText.gameObject.SetActive(false);
        }

        #endregion // Unity Callbacks

        #region UI Construction

        private void ConstructSuiteTable(TestSuiteData suite)
        {
            ClearSuiteEvals();
            m_evalMap.Clear();
            m_allEvals.Clear();
            m_allSuiteContents.Clear();

            var numCols = suite.Headers.Length;
            float tableWidth = 0;

            // headers
            Transform currCellContainer = Instantiate(RowPrefab, RowContainer).transform;
            SuiteRow currRow = currCellContainer.GetComponent<SuiteRow>();
            for (int i = 0; i < numCols; i++)
            {
                SuiteHeader currHeader = Instantiate(HeaderPrefab, currCellContainer).GetComponent<SuiteHeader>();
                currHeader.Label.text = suite.Headers[i].ToString();
                var size = currHeader.Rect.sizeDelta;
                if (suite.Headers[i] == Placeable.OUT || suite.Headers[i] == Placeable.OUTX || suite.Headers[i] == Placeable.OUTY)
                {
                    size.x = OutputCellWidth;
                    currHeader.Rect.sizeDelta = size;
                }
                else
                {
                    size.x = DefaultCellWidth;
                    currHeader.Rect.sizeDelta = size;
                }

                size.y = HeaderHeight;
                currHeader.Rect.sizeDelta = size;
                tableWidth += currHeader.Rect.sizeDelta.x + currRow.Layout.spacing;
            }

            var numRows = suite.Tests.Length;

            var margin = 10;
            var tableSize = ResultRect.sizeDelta;
            tableSize.x = tableWidth + margin * 2;
            tableSize.y = HeaderHeight + RowHeight * numRows + (VertLayout.spacing * numRows) + margin * 2;
            ResultRect.sizeDelta = tableSize;

            // contents
            for (int t = 0; t < numRows; t++)
            {
                currCellContainer = Instantiate(RowPrefab, RowContainer).transform;
                currRow = currCellContainer.GetComponent<SuiteRow>();
                for (int i = 0; i < numCols; i++)
                {
                    SuiteContents currContents = Instantiate(ContentsPrefab, currCellContainer).GetComponent<SuiteContents>();
                    string subtype = EvalUtility.GetSubtypeByPlacableID(suite.Headers[i]);
                    currContents.Label.text = EvalUtility.GetTestValBySubType(subtype, suite.Tests[t]).ToString();
                    var size = currContents.Rect.sizeDelta;
                    if (suite.Headers[i] == Placeable.OUT || suite.Headers[i] == Placeable.OUTX || suite.Headers[i] == Placeable.OUTY)
                    {
                        size.x = OutputCellWidth;
                        currContents.Rect.sizeDelta = size;

                        // Instantiate Cell Eval
                        SuiteCellEval eval = Instantiate(CellEvalPrefab, currContents.transform).GetComponent<SuiteCellEval>();
                        m_evalMap.Add(new Tuple<int, int>(t, i), eval);
                        m_allEvals.Add(eval);
                    }
                    else
                    {
                        size.x = DefaultCellWidth;
                        currContents.Rect.sizeDelta = size;
                    }

                    m_contentsMap.Add(new Tuple<int, int>(t, i), currContents);
                    m_allSuiteContents.Add(currContents);

                    size.y = RowHeight;
                    currContents.Rect.sizeDelta = size;
                }
            }
        }

        private void ClearSuiteEvals()
        {
            foreach (var eval in m_allEvals)
            {
                eval.Img.enabled = false;
            }

            foreach (var contents in m_allSuiteContents)
            {
                contents.FlowImg.enabled = false;
            }
        }

        private void UpdateSuiteEvalsAtPos(int rowIndex, int colIndex, bool success, FlowState flowResult)
        {
            var key = new Tuple<int, int>(rowIndex, colIndex);
            if (!m_evalMap.ContainsKey(key)) { return; }

            SuiteCellEval eval = m_evalMap[key];
            eval.Img.enabled = true;

            if (success)
            {
                eval.SetCorrect();
            }
            else
            {
                eval.SetIncorrect();
            }

            if (m_contentsMap.ContainsKey(key))
            {
                UpdateSuiteContentsAtPos(rowIndex, colIndex, flowResult);
            }
        }

        private void UpdateSuiteContentsAtPos(int rowIndex, int colIndex, FlowState flow)
        {
            var key = new Tuple<int, int>(rowIndex, colIndex);
            if (!m_contentsMap.ContainsKey(key)) { return; }

            SuiteContents contents = m_contentsMap[key];
            contents.FlowImg.enabled = true;

            switch (flow)
            {
                case FlowState.Hi:
                    contents.FlowImg.sprite = SpriteDB.Instance.FlowHi;
                    break;
                case FlowState.Lo:
                    contents.FlowImg.sprite = SpriteDB.Instance.FlowLo;
                    break;
                case FlowState.Unstable:
                    contents.FlowImg.sprite = SpriteDB.Instance.FlowUnstable;
                    break;
                default:
                    contents.FlowImg.enabled = false;
                    break;
            }
        }

        #endregion // UI

        #region Helpers

        private unsafe void Evaluate()
        { 
            if (m_EvaluationRoutine.Exists()) { return; }

            // Gather nodes and edges
            var crucialGraph = new List<CrucialGraphNode>();
            var completeGraph = new List<GraphNode>();
            var orderedEdges = new List<CrucialGraphEdge>();
            Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap = new Dictionary<GraphCoord, CrucialGraphNode>();
            int numCrucialNodes = 0;
            int numCrucialEdges = 0;
            ConstructGraph(out crucialGraph, out completeGraph, out numCrucialNodes, out numCrucialEdges, out orderedEdges, ref crucialCoordNodeMap);

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

            // Evaluation Visuals
            ResetTypeTransformations(crucialGraph, ref crucialCoordNodeMap);

            orderedEdges = SortOrderedEdges(orderedEdges);

            m_EvaluationRoutine.Replace(VisualFeedbackRoutine(evalResult, crucialGraph, crucialCoordNodeMap, orderedEdges, completeGraph));
        }

        private IEnumerator VisualFeedbackRoutine(EvalResult evalResult, List<CrucialGraphNode> crucialGraph, Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap, List<CrucialGraphEdge> orderedEdges, List<GraphNode> completeGraph)
        {
            Debug.Log("[EvaluationMgr] Eval Visuals Started...");

            float timeBetweenSteps = 0.5f;
            float timeBetweenTests = 2;

            // EVALUATE FLOW
            // For each test in suite:
            //      Reset type transformations
            //      For each edge:
            //          determine the starting flow value
            //          for each path node, check if visited
            //              if visited, ensure past flow matches present flow
            //          update visuals along path chunk
            int numTests = LevelMgr.Instance.CurrLevelData.GetTestSuite() != null ? LevelMgr.Instance.CurrLevelData.GetTestSuite().Tests.Length : 0;
            List<EvalResultIndexer> evalIndexers = new List<EvalResultIndexer>();
            bool allTestsCorrect = true;
            IsUnstable = false;
            UnstableText.gameObject.SetActive(IsUnstable);
            ClearSuiteEvals();
            for (int test = 0; test < numTests; test++)
            {
                IsUnstable = false;
                UnstableText.gameObject.SetActive(IsUnstable);
                evalIndexers.Clear();
                bool currTestCorrect = true;
                var currSuite = LevelMgr.Instance.CurrLevelData.GetTestSuite();
                var currTest = currSuite.Tests[test];
                var numCols = currSuite.Headers.Length;
                Debug.Log("[EvaluationMgr] Test " + test);

                // update input visuals
                for (int c = 0; c < numCols; c++)
                {
                    // do outputs later
                    if (currSuite.Headers[c] == Placeable.OUT || currSuite.Headers[c] == Placeable.OUTX || currSuite.Headers[c] == Placeable.OUTY) { continue; }
                    
                    string subtype = EvalUtility.GetSubtypeByPlacableID(currSuite.Headers[c]);
                    UpdateSuiteContentsAtPos(test, c, EvalUtility.GetTestValBySubType(subtype, currTest));
                }

                ResetTypeTransformations(crucialGraph, ref crucialCoordNodeMap);
                ResetFlowStates();
                VisualsMgr.Instance.RefreshVisuals();

                yield return 0.5f;

                /*
                var sb = new StringBuilder();
                sb.Clear();
                foreach (var o in orderedEdges)
                {
                    sb.AppendLine("Node " + o.Origin.Name + " -> Node " + o.Other.Name + " (" + o.EvalDepth + ")");
                }
                NodeReadout.SetText(sb.ToString());
                */

                // reset edge state
                foreach (var edge in orderedEdges)
                {
                    var origin = crucialCoordNodeMap[edge.Origin.Coord];
                    origin.CurrFlowState = FlowState.Empty;
                    origin.TempTransformedType = CellType.NONE;
                    crucialCoordNodeMap[edge.Origin.Coord] = origin;

                    var other = crucialCoordNodeMap[edge.Other.Coord];
                    other.CurrFlowState = FlowState.Empty;
                    other.TempTransformedType = CellType.NONE;
                    crucialCoordNodeMap[edge.Other.Coord] = other;
                }

                int currDepth = 0;
                for (int e = 0; e < orderedEdges.Count; e++)
                {
                    Debug.Log("[EvaluationMgr] edge " + e);

                    var currEdge = orderedEdges[e];
                    currEdge.Origin = crucialCoordNodeMap[currEdge.Origin.Coord];
                    currEdge.Other = crucialCoordNodeMap[currEdge.Other.Coord];
                    var originCell = GridStack.Instance.GetCellDirect(currEdge.Origin.Coord);
                    var destCell = GridStack.Instance.GetCellDirect(currEdge.Other.Coord);

                    FlowState flowState = FlowState.Empty;

                    switch (originCell.CellType)
                    {
                        case CellType.Input:
                            // lookup FlowState by cell's subtype
                            flowState = EvalUtility.GetTestValBySubType(originCell.SubtypeLabel, currTest);
                            break;
                        default:
                            // Use origin's flow type
                            flowState = currEdge.Origin.CurrFlowState;
                            break;
                    }

                    // Try pass flow onto connection (passes by default)
                    bool flowThrough = true;
                    bool stable = flowState != FlowState.Unstable;
                    CellType tempTransformation = CellType.NONE;

                    // Special case: Diodes
                    if (IsTransistorType(originCell.CellType) && IsTransistorType(destCell.CellType))
                    {
                        CellType originType = originCell.CellType;
                        if (currEdge.Origin.TempTransformedType != CellType.NONE)
                        {
                            originType = currEdge.Origin.TempTransformedType;
                        }

                        CellType destType = destCell.CellType;
                        if (currEdge.Other.TempTransformedType != CellType.NONE)
                        {
                            destType = currEdge.Other.TempTransformedType;
                        }

                        if (originType != destType)
                        {
                            flowThrough = EvaluateFlowThroughDiode(flowState, originType, destType);
                        }
                    }

                    // Special case: GateAbove
                    if (destCell.TransferType == TransferType.GateAbove)
                    {
                        var belowCoord = currEdge.Other.Coord;
                        belowCoord.Layer = GridStack.TRANSISTOR_LAYER;
                        var belowCell = GridStack.Instance.GetCellDirect(belowCoord);

                        // if signal is HI, try invert P-type below to N-type
                        if (flowState == FlowState.Hi)
                        {
                            if (belowCell.CellType == CellType.PTransistor)
                            {
                                var belowCNode = crucialCoordNodeMap[belowCoord];
                                belowCNode.TempTransformedType = CellType.NTransistor;
                                tempTransformation = belowCNode.TempTransformedType;
                                crucialCoordNodeMap[belowCoord] = belowCNode;
                            }
                        }
                        // if signal is LO, try invert N-type below to P-type
                        else if (flowState == FlowState.Lo)
                        {
                            if (belowCell.CellType == CellType.NTransistor)
                            {
                                var belowCNode = crucialCoordNodeMap[belowCoord];
                                belowCNode.TempTransformedType = CellType.PTransistor;
                                tempTransformation = belowCNode.TempTransformedType;
                                crucialCoordNodeMap[belowCoord] = belowCNode;
                            }
                        }

                        if (tempTransformation != CellType.NONE)
                        {
                            var cell = GridStack.Instance.GetCellDirect(belowCoord);
                            cell.TempTransformation = tempTransformation;
                            GridStack.Instance.SetCellDirect(belowCoord, cell);
                        }

                        // if signal is unstable, no inversion
                    }

                    // If flow state already exists, ensure they match. Otherwise unstable.
                    if (currEdge.Other.CurrFlowState != FlowState.Empty)
                    {
                        if (currEdge.Origin.CurrFlowState != FlowState.Empty)
                        {
                            stable = currEdge.Origin.CurrFlowState == currEdge.Other.CurrFlowState;
                        }
                    }

                    if (flowThrough)
                    {
                        if (!stable)
                        {
                            // flag all nodes along path as unstable
                            currEdge.Origin.CurrFlowState = FlowState.Unstable;
                            currEdge.Other.CurrFlowState = FlowState.Unstable;
                            flowState = FlowState.Unstable;

                            // flag simulation as unstable
                            currTestCorrect = false;
                            IsUnstable = true;
                            UnstableText.gameObject.SetActive(IsUnstable);
                        }

                        if (flowState != FlowState.Empty)
                        {
                            var updateNode = crucialCoordNodeMap[currEdge.Other.Coord];
                            updateNode.CurrFlowState = flowState;
                            crucialCoordNodeMap[currEdge.Other.Coord] = updateNode;

                            updateNode = crucialCoordNodeMap[currEdge.Origin.Coord];
                            updateNode.CurrFlowState = currEdge.Origin.CurrFlowState;
                            crucialCoordNodeMap[currEdge.Origin.Coord] = updateNode;
                        }

                        if (flowState == FlowState.Unstable)
                        {
                            /*
                            // turn all visited flows into unstable
                            foreach (var graphNode in completeGraph)
                            {
                                var coord = graphNode.Coord;
                                var cell = GridStack.Instance.GetCellDirect(coord);
                                cell.FlowState = flowState;
                                GridStack.Instance.SetCellDirect(coord, cell);
                            }
                            */

                            foreach (var graphNode in currEdge.Path)
                            {
                                var coord = graphNode.Coord;
                                var cell = GridStack.Instance.GetCellDirect(coord);
                                cell.FlowState = flowState;
                                GridStack.Instance.SetCellDirect(coord, cell);
                            }

                            VisualsMgr.Instance.RefreshVisuals();
                        }
                        else if (flowState != FlowState.Empty)
                        {
                            foreach (var graphNode in currEdge.Path)
                            {
                                var coord = graphNode.Coord;
                                var cell = GridStack.Instance.GetCellDirect(coord);
                                cell.FlowState = flowState;
                                GridStack.Instance.SetCellDirect(coord, cell);
                            }
                        }
                    }

                    // Save changes to temporary transformations
                    currEdge.Origin = crucialCoordNodeMap[currEdge.Origin.Coord];
                    currEdge.Other = crucialCoordNodeMap[currEdge.Other.Coord];
                    orderedEdges[e] = currEdge;
                    
                    if (currEdge.EvalDepth > currDepth)
                    {
                        VisualsMgr.Instance.RefreshVisuals();
                        currDepth = currEdge.EvalDepth;

                        yield return timeBetweenSteps;
                    }
                }

                // Check if all relevant outputs have the correct flow state
                if (currTestCorrect)
                {
                    currTestCorrect = OutputsCorrect(test, currTest, ref crucialCoordNodeMap, crucialGraph, ref evalIndexers);
                }
                else
                {
                    // flag as unstable
                    OutputsCorrect(test, currTest, ref crucialCoordNodeMap, crucialGraph, ref evalIndexers, true);
                }

                if (!currTestCorrect) { allTestsCorrect = false; }

                foreach (var result in evalIndexers)
                {
                    UpdateSuiteEvalsAtPos(result.RowIndex, result.ColIndex, result.Success, result.FlowResult);
                }

                yield return timeBetweenTests;
            }

            Debug.Log("[EvaluationMgr] Eval Visuals Ended");

            // Handle result

            if (allTestsCorrect)
            {
                EvaluationSuccess();
            }
            else
            {
                EvaluationFailure();
            }

            IsUnstable = false;
            UnstableText.gameObject.SetActive(IsUnstable);

            /*
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
            */

            yield return null;
        }

        private List<CrucialGraphEdge> SortOrderedEdges(List<CrucialGraphEdge> orderedEdges)
        {
            var newOrder = new List<CrucialGraphEdge>();

            int currDepth = 0;
            while (newOrder.Count < orderedEdges.Count)
            {
                for (int i = 0; i < orderedEdges.Count; i++)
                {
                    if (orderedEdges[i].EvalDepth == currDepth)
                    {
                        newOrder.Add(orderedEdges[i]);
                    }
                }

                currDepth++;
            }

            return newOrder; 
        }

        private bool OutputsCorrect(int testIndex, TestData currTest, ref Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap, List<CrucialGraphNode> crucialGraph, ref List<EvalResultIndexer> evalIndexers, bool isUnstable = false)
        {
            bool allCorrect = true;
            foreach (var cNode in crucialGraph)
            {
                var cell = GridStack.Instance.GetCellDirect(cNode.Coord);
                if (cell.CellType == CellType.Output)
                {
                    bool thisCorrect = true;
                    var outputCNode = crucialCoordNodeMap[cNode.Coord];
                    var evalIndexer = new EvalResultIndexer();
                    evalIndexer.RowIndex = testIndex;
                    evalIndexer.ColIndex = EvalUtility.GetColIndexInHeaders(LevelMgr.Instance.CurrLevelData.GetTestSuite().Headers, cell.SubtypeLabel);

                    evalIndexer.FlowResult = outputCNode.CurrFlowState;
                    if (isUnstable)
                    {
                        thisCorrect = false;
                        allCorrect = false;
                        evalIndexer.FlowResult = FlowState.Unstable;
                    }
                    else if (EvalUtility.GetTestValBySubType(cell.SubtypeLabel, currTest) != outputCNode.CurrFlowState)
                    {
                        thisCorrect = false;
                        allCorrect = false;
                    }

                    evalIndexer.Success = thisCorrect;
                    evalIndexers.Add(evalIndexer);
                }
            }

            return allCorrect;
        }

        private void ResetTypeTransformations(List<CrucialGraphNode> crucialGraph, ref Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap)
        {
            for (int i = 0; i < crucialGraph.Count; i++)
            {
                var coord = crucialGraph[i].Coord;
                var cNode = crucialCoordNodeMap[coord];
                cNode.TempTransformedType = CellType.NONE;
                crucialCoordNodeMap[coord] = cNode;
            }

            var dims = GridStack.Instance.LayerDims;
            var numLayers = GridStack.Instance.GridLayers.Length;
            for (int layer = 0; layer < numLayers; layer++)
            {
                for (int row = 0; row < dims.Y; row++)
                {
                    for (int col = 0; col < dims.X; col++)
                    {
                        var cell = GridStack.Instance.GetCellDirect(layer, col, row);
                        cell.TempTransformation = CellType.NONE;
                        GridStack.Instance.SetCellDirect(layer, col, row, cell);
                    }
                }
            }
        }

        private void ResetFlowStates()
        {
            var dims = GridStack.Instance.LayerDims;
            var numLayers = GridStack.Instance.GridLayers.Length;
            for (int layer = 0; layer < numLayers; layer++)
            {
                for (int row = 0; row < dims.Y; row++)
                {
                    for (int col = 0; col < dims.X; col++)
                    {
                        var cell = GridStack.Instance.GetCellDirect(layer, col, row);
                        cell.FlowState = FlowState.Empty;
                        GridStack.Instance.SetCellDirect(layer, col, row, cell);
                    }
                }
            }

            VisualsMgr.Instance.RefreshVisuals();
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

        private void ConstructGraph(out List<CrucialGraphNode> crucialNodes, out List<GraphNode> allNodes, out int numCrucialNodes, out int numCrucialEdges, out List<CrucialGraphEdge> orderedEdgeProcessList, ref Dictionary<GraphCoord, CrucialGraphNode> crucialCoordNodeMap)
        {
            // SETUP
            crucialNodes = new List<CrucialGraphNode>();
            allNodes = new List<GraphNode>();
            Dictionary<GraphCoord, GraphNode> coordNodeMap = new Dictionary<GraphCoord, GraphNode>();
            crucialCoordNodeMap = new Dictionary<GraphCoord, CrucialGraphNode>();

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

                        // Inputs, Outputs, Gates, and P-N transitions are crucial nodes -- gather them
                        bool isCrucial = false;
                        isCrucial |= (cell.CellType == CellType.Input || cell.CellType == CellType.Output)
                            || (cell.TransferType == TransferType.GateAbove)
                            || (cell.TransferType == TransferType.GateBelow)
                            || IsTransistorTransition(layer, col, row, ref coordNodeMap);

                        // Path ends are also crucial nodes
                        var condensedEdges = EdgeUtility.CondenseEdges(cell.Edges);
                        isCrucial |= EdgeUtility.NumConnections(condensedEdges) == 1;

                        if (isCrucial)
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

        private bool IsTransistorTransition(int layer, int col, int row, ref Dictionary<GraphCoord, GraphNode> coordNodeMap)
        {
            if (layer == GridStack.METAL_LAYER) { return false; }

            var cell = GridStack.Instance.GridLayers[layer].GetCell(col, row);
            if (cell.CellType != CellType.PTransistor && cell.CellType != CellType.NTransistor) { return false; }

            var lookupCoord = new GraphCoord(layer, col, row);

            for (int dir = 0; dir < 6; dir++)
            {
                if (cell.Edges[dir] == EdgeState.Connected)
                {
                    GridUtility.GetOffsetOfDir((EdgeDir)dir, out Vector2Int gridOffset, out int layerOffset);
                    var adjLookupCoord = new GraphCoord(layer + layerOffset, col + gridOffset.x, row + gridOffset.y);

                    var adjCell = GridStack.Instance.GetCellDirect(adjLookupCoord);
                    if (cell.CellType == CellType.NTransistor && adjCell.CellType == CellType.PTransistor)
                    {
                        return true;
                    }
                    else if (cell.CellType == CellType.PTransistor && adjCell.CellType == CellType.NTransistor)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsTransistorType(CellType type)
        {
            return type == CellType.NTransistor || type == CellType.PTransistor;
        }

        private bool EvaluateFlowThroughDiode(FlowState flow, CellType originType, CellType destType)
        {
            // signals only from from P to N, and only when HI
            if (flow != FlowState.Hi && flow != FlowState.Unstable)
            {
                return false;
            }

            if (originType == CellType.PTransistor && destType == CellType.NTransistor)
            {
                return true;
            }

            return false;
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

            List<CNodeDependency> postponedNodes = new List<CNodeDependency>();

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
                        bool addEdge = true;
                        var newCrucialEdge = new CrucialGraphEdge();
                        newCrucialEdge.Init(cNode, accumulatedCrucialNodes[i], accumulatedPath, currDepth);

                        if (!cNode.ContainsCycle(newCrucialEdge))
                        {
                            var transferType = GridStack.Instance.GetCellDirect(accumulatedCrucialNodes[i].Coord).TransferType;
                            if (transferType == TransferType.GateBelow)
                            {
                                // check if this underlying gate is ready to be evaluated
                                var aboveCoord = accumulatedCrucialNodes[i].Coord;
                                aboveCoord.Layer = GridStack.METAL_LAYER;
                                if (crucialCoordNodeMap.ContainsKey(aboveCoord))
                                {
                                    var aboveNode = crucialCoordNodeMap[aboveCoord];
                                    if (aboveNode.EvaluatedForDependency)
                                    {
                                        nodeWorkList.Add(accumulatedCrucialNodes[i]);
                                    }
                                    else
                                    {
                                        addEdge = false;
                                        // do not evaluate until gate dependency is evaluated.
                                        var otherNode = crucialCoordNodeMap[accumulatedCrucialNodes[i].Coord];
                                        otherNode.AwaitingDependency = true;
                                        crucialCoordNodeMap[accumulatedCrucialNodes[i].Coord] = otherNode;
                                        postponedNodes.Add(new CNodeDependency(cNode.Coord, otherNode.Coord));
                                    }
                                }
                            }
                            else if (transferType == TransferType.GateAbove)
                            {
                                var otherNode = crucialCoordNodeMap[accumulatedCrucialNodes[i].Coord];
                                otherNode.EvaluatedForDependency = true;
                                crucialCoordNodeMap[accumulatedCrucialNodes[i].Coord] = otherNode;
                                nodeWorkList.Add(crucialCoordNodeMap[accumulatedCrucialNodes[i].Coord]);

                                // underlying gate is ready to be evaluated
                                var belowCoord = accumulatedCrucialNodes[i].Coord;
                                belowCoord.Layer = GridStack.TRANSISTOR_LAYER;
                                if (crucialCoordNodeMap.ContainsKey(belowCoord))
                                {
                                    var belowNode = crucialCoordNodeMap[belowCoord];
                                    if (belowNode.AwaitingDependency)
                                    {
                                        for (int p = 0; p < postponedNodes.Count; p++)
                                        {
                                            if (postponedNodes[p].DependencyCoord == belowCoord)
                                            {
                                                var postponedNode = crucialCoordNodeMap[postponedNodes[i].NodeCoord];
                                                var updateNode = crucialCoordNodeMap[postponedNodes[i].NodeCoord];
                                                updateNode.EvalDepth = currDepth + 1;
                                                crucialCoordNodeMap[postponedNodes[i].NodeCoord] = updateNode;
                                                nodeWorkList.Add(crucialCoordNodeMap[postponedNodes[i].NodeCoord]);
                                                var belowGraphNode = coordNodeMap[belowCoord];
                                                belowGraphNode.Visited = false;
                                                coordNodeMap[belowCoord] = belowGraphNode;
                                                postponedNodes.RemoveAt(p);
                                                p--;
                                            }
                                        }
                                        // nodeWorkList.Add(crucialCoordNodeMap[belowCoord]);
                                        belowNode.AwaitingDependency = false;
                                    }
                                }
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

                        if (addEdge)
                        {
                            cNode.Edges.Add(newCrucialEdge);
                            orderedEdgeProcessList.Add(newCrucialEdge);
                        }
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

            ResetFlowStates();

            Game.Events.Dispatch(GameEvents.OnResultsHidden);
        }

        #endregion // Handlers
    }
}