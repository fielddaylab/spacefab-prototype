using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualGridCell : MonoBehaviour
    {
        private const int FLOW_SORT_ORDER = 500;
        private const int GATE_SORT_ORDER = 300;
        private const int SECONDARY_SORT_ORDER = 275;
        private const int METAL_SORT_ORDER = 200;
        private const int VIA_SORT_ORDER = 100;
        private const int TRANSISTOR_SORT_ORDER = 0;

        [SerializeField] private SpriteRenderer m_pathRenderer;
        [SerializeField] private SpriteRenderer m_subRenderer;
        [SerializeField] private SpriteRenderer m_pathOverlayRenderer;
        [SerializeField] private SpriteRenderer m_pathOverlayBaseRenderer;
        [SerializeField] private TMP_Text m_textRenderer;
        [SerializeField] private SpriteRenderer m_transferRenderer;
        [SerializeField] private SpriteRenderer m_secondaryTransferRenderer;
        [SerializeField] private SpriteRenderer[] m_dirRenderers;
        [SerializeField] private SpriteMask m_flowMask;

        [SerializeField] private SpriteRenderer m_flowIndicator;

        public void UpdateFlowVisuals(GridCell cell, int layerIndex)
        {
            var flow = cell.FlowState;
            // m_flowIndicator.sortingOrder = FLOW_SORT_ORDER;
            m_flowIndicator.sortingOrder = layerIndex == 0 ? METAL_SORT_ORDER : TRANSISTOR_SORT_ORDER;
            m_flowIndicator.sortingOrder += 50;

            switch (flow)
            {
                case (FlowState.Hi):
                    UpdateHiFlow(cell, layerIndex);
                    break;
                case (FlowState.Lo):
                    UpdateLoFlow(cell, layerIndex);
                    break;
                case (FlowState.Unstable):
                    UpdateUnstableFlow(cell, layerIndex);
                    break;
                default:
                    UpdateDefaultFlow(cell, layerIndex);
                    break;
            }

            // if (EvaluationMgr.Instance.IsUnstable) { m_flowIndicator.sprite = SpriteDB.Instance.FlowUnstable; }
        }

        private void UpdateHiFlow(GridCell cell, int layerIndex)
        {
            if (layerIndex == GridStack.METAL_LAYER)
            {
                m_flowIndicator.sprite = SpriteDB.Instance.FlowHiAbove;
            }
            else if (cell.CellType != CellType.Output && cell.CellType != CellType.Input)
            {
                m_flowIndicator.sprite = SpriteDB.Instance.FlowHiBelow;
            }

            SetTransferWithFlow(cell, FlowState.Hi);
        }

        private void UpdateLoFlow(GridCell cell, int layerIndex)
        {
            if (layerIndex == GridStack.METAL_LAYER)
            {
                m_flowIndicator.sprite = SpriteDB.Instance.FlowLoAbove;
            }
            else if (cell.CellType != CellType.Output && cell.CellType != CellType.Input)
            {
                m_flowIndicator.sprite = SpriteDB.Instance.FlowLoBelow;
            }

            SetTransferWithFlow(cell, FlowState.Lo);
        }

        private void UpdateUnstableFlow(GridCell cell, int layerIndex)
        {
            if (layerIndex == GridStack.METAL_LAYER)
            {
                m_flowIndicator.sprite = SpriteDB.Instance.FlowUnstableAbove;
            }
            else if (cell.CellType != CellType.Output && cell.CellType != CellType.Input)
            {
                m_flowIndicator.sprite = SpriteDB.Instance.FlowUnstableBelow;
            }

            SetTransferWithFlow(cell, FlowState.Unstable);
        }

        private void UpdateDefaultFlow(GridCell cell, int layerIndex)
        {
            m_flowIndicator.sprite = null;

            SetTransferWithFlow(cell, FlowState.Empty);
        }

        private void SetTransferWithFlow(GridCell cell, FlowState flow)
        {
            if (cell.TransferType == TransferType.Via)
            {
                // lookup via for flow state
                var sprite = SpriteDB.Instance.LookupViaSprite(flow);
                m_transferRenderer.sprite = sprite;
                m_secondaryTransferRenderer.sprite = sprite;
            }
            else if (cell.TransferType == TransferType.GateAbove)
            {
                var sprite = SpriteDB.Instance.LookupGateSprite(flow);
                m_transferRenderer.sprite = sprite;
            }
        }

        public void RefreshVisual(GridCell cellData, int layerIndex, int col, int row)
        {
            PathLibrary.AssembledPathData pathData = default;
            bool lookedUpEdge = false;

            // Reset
            m_pathRenderer.sprite = null;
            m_pathOverlayRenderer.sprite = null;
            m_pathOverlayBaseRenderer.sprite = null;
            m_subRenderer.sprite = null;
            foreach (var r in m_dirRenderers) { r.sprite = null; }
            m_flowMask.sprite = null;
            m_flowMask.backSortingOrder = 0;
            m_flowMask.frontSortingOrder = 0;
            m_textRenderer.SetText("");
            m_pathRenderer.color = Color.white;

            // Render according to cell data
            switch (cellData.CellType)
            {
                case CellType.Metal:
                    SpriteDB.Instance.MetalLibrary.Lookup(EdgeUtility.CondenseEdges(cellData.Edges), out pathData);
                    lookedUpEdge = true;
                    break;
                case CellType.NTransistor:
                    RenderNTransistor(ref cellData, ref pathData, ref lookedUpEdge, layerIndex, col, row);
                    break;
                case CellType.PTransistor:
                    RenderPTransistor(ref cellData, ref pathData, ref lookedUpEdge, layerIndex, col, row);
                    break;
                case CellType.Input:
                    m_pathRenderer.sprite = SpriteDB.Instance.IOOuter;
                    m_subRenderer.sprite = SpriteDB.Instance.IOInner;
                    m_textRenderer.SetText(cellData.SubtypeLabel);
                    break;
                case CellType.Output:
                    m_pathRenderer.sprite = SpriteDB.Instance.IOOuter;
                    m_subRenderer.sprite = SpriteDB.Instance.IOInner;
                    m_textRenderer.SetText(cellData.SubtypeLabel);
                    break;
                default:
                    break;
            }

            // Reset
            m_transferRenderer.sprite = null;
            m_secondaryTransferRenderer.sprite = null;

            switch (cellData.TransferType)
            {
                case TransferType.Via:
                    m_transferRenderer.sprite = SpriteDB.Instance.LookupViaSprite(FlowState.Empty);
                    m_secondaryTransferRenderer.sprite = SpriteDB.Instance.LookupViaSprite(FlowState.Empty);
                    break;
                case TransferType.GateAbove:
                    m_transferRenderer.sprite = SpriteDB.Instance.LookupGateSprite(FlowState.Empty);
                    break;
                default:
                    break;
            }

            m_pathRenderer.sortingOrder = layerIndex == 0 ? METAL_SORT_ORDER : TRANSISTOR_SORT_ORDER;
            m_pathOverlayRenderer.sortingOrder = m_pathRenderer.sortingOrder + 3;
            m_pathOverlayBaseRenderer.sortingOrder = m_pathOverlayRenderer.sortingOrder - 1;
            m_subRenderer.sortingOrder = m_pathRenderer.sortingOrder - 10;
            m_textRenderer.GetComponent<Renderer>().sortingOrder = m_pathRenderer.sortingOrder + 10;
            foreach (var r in m_dirRenderers) { r.sortingOrder = m_pathRenderer.sortingOrder + 5; }
            m_transferRenderer.sortingOrder = cellData.TransferType == TransferType.Via ? VIA_SORT_ORDER : GATE_SORT_ORDER;
            m_secondaryTransferRenderer.sortingOrder = SECONDARY_SORT_ORDER;

            m_flowMask.backSortingOrder = m_pathRenderer.sortingOrder - 50;
            m_flowMask.frontSortingOrder = m_pathRenderer.sortingOrder + 50;

            if (lookedUpEdge)
            {
                m_pathRenderer.sprite = pathData.Sprite;
                var angles = m_pathRenderer.transform.rotation.eulerAngles;
                angles.z = 90 * pathData.Turns;
                m_pathRenderer.transform.rotation = Quaternion.Euler(angles);

                m_flowMask.transform.rotation = Quaternion.Euler(angles);
                m_flowMask.sprite = pathData.Sprite;
            }

            UpdateFlowVisuals(cellData, layerIndex);
        }

        private void RenderNTransistor(ref GridCell cellData, ref PathLibrary.AssembledPathData pathData, ref bool lookedUpEdge, int layerIndex, int col, int row)
        {
            var condensedEdges = EdgeUtility.CondenseEdges(cellData.Edges);
            SpriteDB.Instance.TransistorLibrary.Lookup(condensedEdges, out pathData);
            lookedUpEdge = true;
            m_pathRenderer.color = SpriteDB.Instance.NColor;

            if (cellData.TempTransformation != CellType.NONE)
            {
                if (cellData.TempTransformation == CellType.PTransistor)
                {
                    m_pathOverlayRenderer.sprite = SpriteDB.Instance.InvertedOverlay;
                    m_pathOverlayBaseRenderer.sprite = SpriteDB.Instance.InvertedOverlayBase;
                    m_pathOverlayRenderer.color = SpriteDB.Instance.PColor;
                    m_pathOverlayBaseRenderer.color = SpriteDB.Instance.NColor;

                    m_pathRenderer.color = SpriteDB.Instance.PColor;
                }
            }

            // set dir renderers
            for (int i = 0; i < 4; i++)
            {
                if (condensedEdges[i] == EdgeState.Connected)
                {
                    // lookup adjacent
                    int adjCol = col;
                    int adjRow = row;

                    // N
                    if (i == 0) { adjRow++; }
                    // E
                    else if (i == 1) { adjCol++; }
                    // S
                    else if (i == 2) { adjRow--; }
                    // W
                    else if (i == 3) { adjCol--; }

                    if (GridStack.Instance.InBounds(adjCol, adjRow))
                    {
                        var adjCell = GridStack.Instance.GridLayers[layerIndex].GetCell(adjCol, adjRow);
                        // if P, set N to P half of renderer
                        if (adjCell.CellType == CellType.PTransistor)
                        {
                            if (cellData.TempTransformation != CellType.PTransistor && adjCell.TempTransformation != CellType.NTransistor)
                            {
                                m_dirRenderers[i].sprite = SpriteDB.Instance.NSide;
                            }
                        }
                    }
                }
            }
        }

        private void RenderPTransistor(ref GridCell cellData, ref PathLibrary.AssembledPathData pathData, ref bool lookedUpEdge, int layerIndex, int col, int row)
        {
            var condensedEdges = EdgeUtility.CondenseEdges(cellData.Edges);
            SpriteDB.Instance.TransistorLibrary.Lookup(condensedEdges, out pathData);
            lookedUpEdge = true;
            m_pathRenderer.color = SpriteDB.Instance.PColor;

            if (cellData.TempTransformation != CellType.NONE)
            {
                if (cellData.TempTransformation == CellType.NTransistor)
                {
                    m_pathOverlayRenderer.sprite = SpriteDB.Instance.InvertedOverlay;
                    m_pathOverlayBaseRenderer.sprite = SpriteDB.Instance.InvertedOverlayBase;
                    m_pathOverlayRenderer.color = SpriteDB.Instance.NColor;
                    m_pathOverlayBaseRenderer.color = SpriteDB.Instance.PColor;

                    m_pathRenderer.color = SpriteDB.Instance.NColor;
                }
            }

            // set dir renderers
            for (int i = 0; i < 4; i++)
            {
                if (condensedEdges[i] == EdgeState.Connected)
                {
                    // lookup adjacent
                    int adjCol = col;
                    int adjRow = row;

                    // N
                    if (i == 0) { adjRow++; }
                    // E
                    else if (i == 1) { adjCol++; }
                    // S
                    else if (i == 2) { adjRow--; }
                    // W
                    else if (i == 3) { adjCol--; }

                    if (GridStack.Instance.InBounds(adjCol, adjRow))
                    {
                        var adjCell = GridStack.Instance.GridLayers[layerIndex].GetCell(adjCol, adjRow);
                        // if P, set N to P half of renderer
                        if (adjCell.CellType == CellType.NTransistor)
                        {
                            if (cellData.TempTransformation != CellType.NTransistor && adjCell.TempTransformation != CellType.PTransistor)
                            {
                                m_dirRenderers[i].sprite = SpriteDB.Instance.PSide;
                            }
                        }
                    }
                }
            }
        }
    }
}
