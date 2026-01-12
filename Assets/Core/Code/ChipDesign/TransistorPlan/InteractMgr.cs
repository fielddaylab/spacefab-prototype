using ChipFab.ChipDesign;
using FieldDay;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceFab.ChipDesign
{
    public class InteractMgr : MonoBehaviour
    {
        public static InteractMgr Instance;

        public CursorHint WaitCursor;

        #region Toolbar

        public GridInteractionLayer ActiveLayer { get; private set; }
        public ToolType ActiveTool = ToolType.None;

        #endregion // Toolbar

        #region Members

        private Vector2Int m_LastKnownDragCoord;
        private Vector2Int m_LastTerminatedDragCoord;

        [HideInInspector] public bool InteractInputsEnabled = true;

        #endregion Members

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Game.Events.Register(GameEvents.EvaluationStarted, HandleEvalStarted);
            Game.Events.Register(GameEvents.OnResultsDisplayed, HandleResultsDisplayed);
            Game.Events.Register(GameEvents.OnResultsHidden, HandleResultsHidden);

            m_LastTerminatedDragCoord = -Vector2Int.one;
        }

        private void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }

            Game.Events.Deregister(GameEvents.EvaluationStarted, HandleEvalStarted);
            Game.Events.Deregister(GameEvents.OnResultsDisplayed, HandleResultsDisplayed);
            Game.Events.Deregister(GameEvents.OnResultsHidden, HandleResultsHidden);
        }

        private void Update()
        {
            ProcessInputs();
        }

        #endregion // Unity Callbacks

        #region Coordinate Inputs

        private void ProcessInputs()
        {
            if (EventSystem.current.IsPointerOverGameObject()) { return; }
            if (!InteractInputsEnabled) { return; }

            if (Input.GetMouseButtonDown(0)) {
                HandleLeftMouseDown();
            }
            if (Input.GetMouseButton(0)) {
                HandleLeftMouseDrag();
            }
            if (Input.GetMouseButtonUp(0)) {
                HandleLeftMouseUp();
            }
        }

        private void HandleLeftMouseDown()
        {
            // get mouse position in world space
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var gridPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));

            // if grid cell is out of bounds:
            if (!GridStack.Instance.InBounds(gridPos.x, gridPos.y)) {
                // do nothing
                return;
            }

            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            // if grid cell is empty:
            if (layer.IsCellEmpty(gridPos))
            {
                if (ActiveLayer == GridInteractionLayer.Metal)
                {
                    ClickEmptyMLayerCell(gridPos);
                }
                else
                {
                    ClickEmptyTLayerCell(gridPos);
                }
            }
            // if grid cell is full:
            else
            {
                if (ActiveLayer == GridInteractionLayer.Metal)
                {
                    ClickOccupiedMLayerCell(gridPos);
                }
                else
                {
                    ClickOccupiedTLayerCell(gridPos);
                }
            }
            Debug.Log("[InteractMgr] Click Coords: (x: " + gridPos.x + " , y: " + gridPos.y + ")");

            // begin dragging
            m_LastKnownDragCoord = gridPos;
        }

        private void HandleLeftMouseDrag()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var gridPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));

            if (gridPos == m_LastKnownDragCoord) {
                // no change in drag position
                return;
            }

            if (m_LastTerminatedDragCoord != -Vector2Int.one)
            {
                // no change from last terminated drag position (player needs to release mouse before new drag can begin)
                return;
            }

            var dif = gridPos - m_LastKnownDragCoord;
            if (dif.x != 0 && dif.y != 0)
            {
                // only orthogonal movement allowed; collapse to one dimension (x)
                gridPos.y = m_LastKnownDragCoord.y;
            }

            // if dragging too quickly
            if (Math.Abs(dif.x) > 1 || Math.Abs(dif.y) > 1)
            {
                // terminate drag
                TerminateDrag();
                return;
            }

            // if out of bounds:
            if (!GridStack.Instance.InBounds(gridPos.x, gridPos.y)) {
                // terminate drag
                TerminateDrag();
                return;
            }

            // if within bounds:
            Debug.Log("[InteractMgr] Dragging to (x: " + gridPos.x + " , y: " + gridPos.y + ")");
            
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            // if grid cell is empty:
            if (layer.IsCellEmpty(gridPos))
            {
                if (ActiveLayer == GridInteractionLayer.Metal)
                {
                    DragEmptyMLayerCell(gridPos);
                }
                else
                {
                    DragEmptyTLayerCell(gridPos);
                }
            }
            // if grid cell is full:
            else
            {
                if (ActiveLayer == GridInteractionLayer.Metal)
                {
                    DragOccupiedMLayerCell(gridPos);
                }
                else
                {
                    DragOccupiedTLayerCell(gridPos);
                }
            }

            // continue dragging
            if (gridPos != m_LastTerminatedDragCoord)
            {
                m_LastKnownDragCoord = gridPos;
            }
        }

        private void HandleLeftMouseUp()
        {
            TerminateDrag(true);
        }

        #endregion // Coordinate Inputs

        #region Clicks

        private void ClickEmptyMLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawLinks:
                    DrawMetal(ref cell, gridPos);
                    break;
                case ToolType.DrawVia:
                    DrawVia(ref cell, gridPos);
                    break;
                case ToolType.DrawGate:
                    DrawGate(ref cell, gridPos);
                    break;
                default:
                    break;
            }

            layer.SetCell(gridPos, cell);
        }

        private void ClickEmptyTLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawNNodes:
                    cell.CellType = CellType.NTransistor;
                    break;
                case ToolType.DrawPNodes:
                    cell.CellType = CellType.PTransistor;
                    break;
                case ToolType.DrawInNodes:
                    DrawIONode(true, GameConsts.IN_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawOutNodes:
                    DrawIONode(false, GameConsts.OUT_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawOutXNodes:
                    DrawIONode(false, GameConsts.X_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawOutYNodes:
                    DrawIONode(false, GameConsts.Y_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawVPlusNodes:
                    DrawIONode(true, GameConsts.VPLUS_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawVMinusNodes:
                    DrawIONode(true, GameConsts.VMINUS_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawANodes:
                    DrawIONode(true, GameConsts.A_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawBNodes:
                    DrawIONode(true, GameConsts.B_SUBTYPE, ref cell, gridPos);
                    break;
                case ToolType.DrawVia:
                    DrawVia(ref cell, gridPos);
                    break;
                case ToolType.DrawGate:
                    DrawGate(ref cell, gridPos);
                    break;
                default:
                    break;
            }

            layer.SetCell(gridPos, cell);
        }

        private void ClickOccupiedMLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawLinks:
                    // Do nothing. Click only matters if node is empty.
                    break;
                case ToolType.DrawVia:
                    // place a via if metal
                    if (cell.CellType == CellType.Metal) {
                        DrawVia(ref cell, gridPos);
                    }
                    break;
                case ToolType.DrawGate:
                    // place a gate if metal
                    if (cell.CellType == CellType.Metal) {
                        DrawGate(ref cell, gridPos);
                    }
                    break;
                default:
                    break;
            }

            layer.SetCell(gridPos, cell);
        }

        private void ClickOccupiedTLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawNNodes:
                    // only relevant if the occupied cell is a transistor
                    if (cell.CellType == CellType.NTransistor || cell.CellType == CellType.PTransistor) {
                        cell.CellType = CellType.NTransistor;
                        // note: preserves edge connections
                    }
                    break;
                case ToolType.DrawPNodes:
                    // only relevant if the occupied cell is a transistor
                    if (cell.CellType == CellType.NTransistor || cell.CellType == CellType.PTransistor) {
                        cell.CellType = CellType.PTransistor;
                        // note: preserves edge connections
                    }
                    break;
                // only allow inputs/outputs to be placed on empty spaces
                case ToolType.DrawInNodes:
                case ToolType.DrawOutNodes:
                case ToolType.DrawVPlusNodes:
                case ToolType.DrawVMinusNodes:
                case ToolType.DrawANodes:
                case ToolType.DrawBNodes:
                    break;
                case ToolType.DrawVia:
                    // place a via if transistor
                    if (cell.CellType == CellType.NTransistor || cell.CellType == CellType.PTransistor) {
                        DrawVia(ref cell, gridPos);
                    }
                    break;
                case ToolType.DrawGate:
                    // place a gate if transistor
                    if (cell.CellType == CellType.NTransistor || cell.CellType == CellType.PTransistor) {
                        DrawGate(ref cell, gridPos);
                    }
                    break;
                default:
                    break;
            }

            layer.SetCell(gridPos, cell);
        }

        #endregion // Clicks

        #region Dragging

        private void DragEmptyMLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawLinks:
                    DragDrawNodeOfType(CellType.Metal, gridPos);
                    break;
                default:
                    break;
            }
        }

        private void DragEmptyTLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawNNodes:
                    DragDrawNodeOfType(CellType.NTransistor, gridPos);
                    break;
                case ToolType.DrawPNodes:
                    DragDrawNodeOfType(CellType.PTransistor, gridPos);
                    break;
                default:
                    break;
            }
        }

        private void DragOccupiedMLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawLinks:
                    DragDrawNodeOfType(CellType.Metal, gridPos);
                    break;
                default:
                    break;
            }
        }

        private void DragOccupiedTLayerCell(Vector2Int gridPos)
        {
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var cell = layer.GetCell(gridPos);

            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    EraseCellBothLayers(gridPos);
                    break;
                case ToolType.DrawNNodes:
                    // do not allow dragging onto inputs/outputs
                    if (cell.CellType == CellType.Input || cell.CellType == CellType.Output) 
                    {
                        TerminateDrag();
                        return;
                    }
                    else
                    {
                        if (cell.CellType == CellType.PTransistor)
                        {
                            // draw connection, preserve type
                            DragDrawNodeOfType(CellType.PTransistor, gridPos);
                        }
                        else
                        {
                            // override
                            DragDrawNodeOfType(CellType.NTransistor, gridPos);
                        }
                    }
                    break;
                case ToolType.DrawPNodes:
                    // do not allow dragging onto inputs/outputs
                    if (cell.CellType == CellType.Input || cell.CellType == CellType.Output)
                    {
                        TerminateDrag();
                        return;
                    }
                    else
                    {
                        if (cell.CellType == CellType.NTransistor)
                        {
                            // draw connection, preserve type
                            DragDrawNodeOfType(CellType.NTransistor, gridPos);
                        }
                        else
                        {
                            // override
                            DragDrawNodeOfType(CellType.PTransistor, gridPos);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion // Dragging

        #region Releases

        /// <summary>
        /// Terminates drag tracking.
        /// </summary>
        /// <param name="fullRelease">True if player released mouse button, false if released due to logic rules</param>
        private void TerminateDrag(bool fullRelease = false)
        {
            // stop tracking dragging
            if (!fullRelease) { m_LastTerminatedDragCoord = m_LastKnownDragCoord; }
            else { m_LastTerminatedDragCoord = -Vector2Int.one; }

            m_LastKnownDragCoord = -Vector2Int.one;
        }

        #endregion // Releases

        #region Helpers

        private void DrawMetal(ref GridCell cell, Vector2Int gridPos)
        {
            cell.CellType = CellType.Metal;

            // if an input or output is below, connect edge
            GridInteractionLayer linkedLayerType = GridInteractionLayer.Transistor;
            var linkedLayer = GridStack.Instance.GridLayers[(int)linkedLayerType];
            var linkedCell = linkedLayer.GetCell(gridPos);

            if (linkedCell.CellType == CellType.Input || linkedCell.CellType == CellType.Output)
            {
                cell.TransferType = TransferType.Implicit;
                linkedCell.TransferType = TransferType.Implicit;

                int cellEdgeIndex = (int)EdgeDir.DESCEND;
                int linkedEdgeIndex = (int)EdgeDir.ASCEND;
                cell.Edges[cellEdgeIndex].EdgeState = EdgeState.Connected;
                linkedCell.Edges[linkedEdgeIndex].EdgeState = EdgeState.Connected;
            }
        }

        private void ConnectToMetalLayer(ref GridCell cell, Vector2Int gridPos)
        {
            // in there's metal above, connect edge
            GridInteractionLayer linkedLayerType = GridInteractionLayer.Metal;
            var linkedLayer = GridStack.Instance.GridLayers[(int)linkedLayerType];
            var linkedCell = linkedLayer.GetCell(gridPos);

            if (linkedCell.CellType == CellType.Metal)
            {
                cell.TransferType = TransferType.Implicit;
                linkedCell.TransferType = TransferType.Implicit;

                int cellEdgeIndex = (int)EdgeDir.ASCEND;
                int linkedEdgeIndex = (int)EdgeDir.DESCEND;
                cell.Edges[cellEdgeIndex].EdgeState = EdgeState.Connected;
                linkedCell.Edges[linkedEdgeIndex].EdgeState = EdgeState.Connected;
            }
        }

        private void DrawIONode(bool isInput, string subtype, ref GridCell cell, Vector2Int gridPos)
        {
            if (cell.TransferType == TransferType.GateAbove || cell.TransferType == TransferType.GateBelow  ||  cell.TransferType == TransferType.Via) { return; }
            cell.CellType = isInput ? CellType.Input : CellType.Output;
            cell.SubtypeLabel = subtype;
            ConnectToMetalLayer(ref cell, gridPos);
        }

        private void DrawVia(ref GridCell cell, Vector2Int gridPos)
        {
            if (cell.CellType == CellType.Input || cell.CellType == CellType.Output || !cell.TransferEraseable) { return; }

            GridInteractionLayer linkedLayerType = ActiveLayer == GridInteractionLayer.Metal ? GridInteractionLayer.Transistor : GridInteractionLayer.Metal;
            var linkedLayer = GridStack.Instance.GridLayers[(int)linkedLayerType];
            var linkedCell = linkedLayer.GetCell(gridPos);

            if (linkedCell.CellType == CellType.Input || linkedCell.CellType == CellType.Output) { return; }

            cell.TransferType = TransferType.Via;
            linkedCell.TransferType = TransferType.Via;

            int cellEdgeIndex = ActiveLayer == GridInteractionLayer.Metal ? (int)EdgeDir.DESCEND : (int)EdgeDir.ASCEND;
            int linkedEdgeIndex = ActiveLayer == GridInteractionLayer.Metal ? (int)EdgeDir.ASCEND : (int)EdgeDir.DESCEND;
            cell.Edges[cellEdgeIndex].EdgeState = EdgeState.Connected;
            linkedCell.Edges[linkedEdgeIndex].EdgeState = EdgeState.Connected;
        }

        private void DrawGate(ref GridCell cell, Vector2Int gridPos)
        {
            if (cell.CellType == CellType.Input || cell.CellType == CellType.Output || !cell.TransferEraseable) { return; }

            GridInteractionLayer linkedLayerType = ActiveLayer == GridInteractionLayer.Metal ? GridInteractionLayer.Transistor : GridInteractionLayer.Metal;
            var linkedLayer = GridStack.Instance.GridLayers[(int)linkedLayerType];
            var linkedCell = linkedLayer.GetCell(gridPos);

            if (linkedCell.CellType == CellType.Input || linkedCell.CellType == CellType.Output) { return; }

            cell.TransferType = ActiveLayer == GridInteractionLayer.Metal ? TransferType.GateAbove : TransferType.GateBelow;
            linkedCell.TransferType = ActiveLayer == GridInteractionLayer.Metal ? TransferType.GateBelow : TransferType.GateAbove;

            /*
            int cellEdgeIndex = ActiveLayer == GridInteractionLayer.Metal ? (int)EdgeDir.DESCEND : (int)EdgeDir.ASCEND;
            int linkedEdgeIndex = ActiveLayer == GridInteractionLayer.Metal ? (int)EdgeDir.ASCEND : (int)EdgeDir.DESCEND;
            cell.Edges[cellEdgeIndex] = EdgeState.Connected;
            linkedCell.Edges[linkedEdgeIndex] = EdgeState.Connected;
            */
        }

        private void EraseCellBothLayers(Vector2Int gridPos)
        {
            int currLayer = (int)ActiveLayer;
            var layer = GridStack.Instance.GridLayers[currLayer];
            var cell = layer.GetCell(gridPos);

            EraseCellOneLayer(cell, gridPos, currLayer);

            currLayer = (int)GetOppositeLayer(ActiveLayer);
            var twinLayer = GridStack.Instance.GridLayers[currLayer];
            var twinCell = twinLayer.GetCell(gridPos);
            EraseCellOneLayer(twinCell, gridPos, currLayer);
        }

        private GridInteractionLayer GetOppositeLayer(GridInteractionLayer layer)
        {
            if (layer == GridInteractionLayer.Metal) { return GridInteractionLayer.Transistor; }
            else { return GridInteractionLayer.Metal; }
        }

        private void EraseCellOneLayer(GridCell cell, Vector2Int gridPos, int currLayer)
        {
            // erase cell
            cell.Erase(out List<EdgeDir> danglingEdges);

            // erase dangling edges
            foreach (var dangling in danglingEdges)
            {
                // get adj cell
                var adjCell = GetAdjCell(gridPos, dangling, currLayer);

                // erase opposite edge
                adjCell.EraseEdge(GetOppositeDir(dangling));
            }

            Game.Events.Dispatch(GameEvents.OnLayoutChanged);
        }

        private void DragDrawNodeOfType(CellType type, Vector2Int gridPos)
        {
            // create edge between last known pos and curr pos
            var layer = GridStack.Instance.GridLayers[(int)ActiveLayer];
            var fromCell = layer.GetCell(m_LastKnownDragCoord);
            var toCell = layer.GetCell(gridPos);
            var fromDir = GridUtility.DirFromToCell(m_LastKnownDragCoord, gridPos);
            var reverseDir = GetOppositeDir(fromDir);

            // disallow drag from inputs/outputs on transistor layer
            if (type == CellType.NTransistor || type == CellType.PTransistor)
            {
                if (fromCell.CellType == CellType.Input || fromCell.CellType == CellType.Input)
                {
                    TerminateDrag();
                    return;
                }
            }

            fromCell.Edges[(int)fromDir].EdgeState = EdgeState.Connected;
            toCell.Edges[(int)reverseDir].EdgeState = EdgeState.Connected;

            // set properties
            toCell.CellType = type;

            if (type == CellType.Metal)
            {
                DrawMetal(ref toCell, gridPos);
            }
            else if (type == CellType.Input || type == CellType.Output)
            {
                ConnectToMetalLayer(ref toCell, gridPos);
            }

            // save changes
            layer.SetCell(m_LastKnownDragCoord, fromCell);
            layer.SetCell(gridPos, toCell);
        }

        private GridCell GetAdjCell(Vector2Int gridPos, EdgeDir dir, int currLayer)
        {
            int layerOffset = 0;
            Vector2Int gridOffset = Vector2Int.zero;

            GridUtility.GetOffsetOfDir(dir, out gridOffset, out layerOffset);

            var adjGridPos = gridPos + gridOffset;
            var adjLayerIndex = currLayer + layerOffset;

            var adjLayer = GridStack.Instance.GridLayers[adjLayerIndex];

            GridCell adjCell = adjLayer.GetCell(adjGridPos);

            return adjCell;
        }

        private EdgeDir GetOppositeDir(EdgeDir original)
        {
            EdgeDir opposite = (EdgeDir)(((int)original + Enum.GetValues(typeof(EdgeDir)).Length / 2) % Enum.GetValues(typeof(EdgeDir)).Length);

            return opposite;
        }

        #endregion // Helpers

        #region Tools

        public void SetActiveTool(ToolType tool)
        {
            ActiveTool = tool;
            Game.Events.Dispatch(GameEvents.OnToolChanged);
        }

        #endregion // Tools

        #region Layers

        public void SetActiveLayer(GridInteractionLayer layer)
        {
            ActiveLayer = layer;
            Game.Events.Dispatch(GameEvents.OnLayerChanged);
        }

        #endregion // Layers

        #region Handlers

        private void HandleEvalStarted()
        {
            InteractInputsEnabled = false;
            CursorHint.TryLock(WaitCursor);
        }

        private void HandleResultsDisplayed()
        {
            InteractInputsEnabled = true;
            CursorHint.Unlock(WaitCursor);
        }

        private void HandleResultsHidden()
        {
            InteractInputsEnabled = true;
            CursorHint.Unlock(WaitCursor);
        }

        #endregion // Handlers
    }
}