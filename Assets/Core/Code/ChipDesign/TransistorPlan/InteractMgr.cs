using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class InteractMgr : MonoBehaviour
    {
        public static InteractMgr Instance;

        #region Toolbar

        public GridInteractionLayer ActiveLayer { get; private set; }
        public ToolType ActiveTool = ToolType.None;

        #endregion // Toolbar

        #region Members

        private Vector2Int m_LastKnownDragCoord;

        #endregion Members

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            ProcessInputs();
        }

        #endregion // Unity Callbacks

        #region Coordinate Inputs

        private void ProcessInputs()
        {
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
                    ClickEmptyMLayerCell();
                }
                else
                {
                    ClickEmptyTLayerCell();
                }
            }
            // if grid cell is full:
            else
            {
                if (ActiveLayer == GridInteractionLayer.Metal)
                {
                    ClickOccupiedMLayerCell();
                }
                else
                {
                    ClickOccupiedTLayerCell();
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
                    DragEmptyMLayerCell();
                }
                else
                {
                    DragEmptyTLayerCell();
                }
            }
            // if grid cell is full:
            else
            {
                if (ActiveLayer == GridInteractionLayer.Metal)
                {
                    DragOccupiedMLayerCell();
                }
                else
                {
                    DragOccupiedTLayerCell();
                }
            }

            // continue dragging
            m_LastKnownDragCoord = gridPos;
        }

        private void HandleLeftMouseUp()
        {
            TerminateDrag();
        }

        #endregion // Coordinate Inputs

        #region Clicks

        private void ClickEmptyMLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.DrawLinks:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        private void ClickEmptyTLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.DrawNNodes:
                    break;
                case ToolType.DrawPNodes:
                    break;
                case ToolType.DrawInNodes:
                    break;
                case ToolType.DrawOutNodes:
                    break;
                case ToolType.DrawVPlusNodes:
                    break;
                case ToolType.DrawVMinusNodes:
                    break;
                case ToolType.DrawANodes:
                    break;
                case ToolType.DrawBNodes:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        private void ClickOccupiedMLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    break;
                case ToolType.DrawLinks:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        private void ClickOccupiedTLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    break;
                case ToolType.DrawNNodes:
                    break;
                case ToolType.DrawPNodes:
                    break;
                case ToolType.DrawInNodes:
                    break;
                case ToolType.DrawOutNodes:
                    break;
                case ToolType.DrawVPlusNodes:
                    break;
                case ToolType.DrawVMinusNodes:
                    break;
                case ToolType.DrawANodes:
                    break;
                case ToolType.DrawBNodes:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        #endregion // Clicks

        #region Dragging

        private void DragEmptyMLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.DrawLinks:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        private void DragEmptyTLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.DrawNNodes:
                    break;
                case ToolType.DrawPNodes:
                    break;
                case ToolType.DrawInNodes:
                    break;
                case ToolType.DrawOutNodes:
                    break;
                case ToolType.DrawVPlusNodes:
                    break;
                case ToolType.DrawVMinusNodes:
                    break;
                case ToolType.DrawANodes:
                    break;
                case ToolType.DrawBNodes:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        private void DragOccupiedMLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    break;
                case ToolType.DrawLinks:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        private void DragOccupiedTLayerCell()
        {
            // check tool
            switch (ActiveTool)
            {
                case ToolType.Erase:
                    break;
                case ToolType.DrawNNodes:
                    break;
                case ToolType.DrawPNodes:
                    break;
                case ToolType.DrawInNodes:
                    break;
                case ToolType.DrawOutNodes:
                    break;
                case ToolType.DrawVPlusNodes:
                    break;
                case ToolType.DrawVMinusNodes:
                    break;
                case ToolType.DrawANodes:
                    break;
                case ToolType.DrawBNodes:
                    break;
                case ToolType.DrawVia:
                    break;
                case ToolType.DrawGate:
                    break;
                default:
                    break;
            }
        }

        #endregion // Dragging

        #region Releases

        private void TerminateDrag()
        {
            // stop tracking dragging
            m_LastKnownDragCoord = Vector2Int.one * -1;
        }

        #endregion // Releases

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
    }
}