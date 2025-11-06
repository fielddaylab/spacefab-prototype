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
        private const int METAL_SORT_ORDER = 200;
        private const int VIA_SORT_ORDER = 100;
        private const int TRANSISTOR_SORT_ORDER = 0;

        [SerializeField] private SpriteRenderer m_pathRenderer;
        [SerializeField] private SpriteRenderer m_subRenderer;
        [SerializeField] private TMP_Text m_textRenderer;
        [SerializeField] private SpriteRenderer m_transferRenderer;
        [SerializeField] private SpriteRenderer[] m_dirRenderers;
        [SerializeField] private SpriteMask m_flowMask;

        [SerializeField] private SpriteRenderer m_flowIndicator;

        public void UpdateFlowVisuals(FlowState flow, int layerIndex)
        {
            // m_flowIndicator.sortingOrder = FLOW_SORT_ORDER;
            m_flowIndicator.sortingOrder = layerIndex == 0 ? METAL_SORT_ORDER : TRANSISTOR_SORT_ORDER;
            m_flowIndicator.sortingOrder += 50;

            switch (flow)
            {
                case (FlowState.Hi):
                    m_flowIndicator.sprite = SpriteDB.Instance.FlowHi;
                    break;
                case (FlowState.Lo):
                    m_flowIndicator.sprite = SpriteDB.Instance.FlowLo;
                    break;
                case (FlowState.Unstable):
                    m_flowIndicator.sprite = SpriteDB.Instance.FlowUnstable;
                    break;
                default:
                    m_flowIndicator.sprite = null;
                    break;
            }
        }

        public void RefreshVisual(GridCell cellData, int layerIndex, int col, int row)
        {
            PathLibrary.AssembledPathData pathData = default;
            bool lookedUpEdge = false;

            // Reset
            m_pathRenderer.sprite = null;
            m_subRenderer.sprite = null;
            foreach (var r in m_dirRenderers) { r.sprite = null; }
            m_flowMask.sprite = null;
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

            switch (cellData.TransferType)
            {
                case TransferType.Via:
                    m_transferRenderer.sprite = SpriteDB.Instance.Via;
                    break;
                case TransferType.GateAbove:
                    m_transferRenderer.sprite = SpriteDB.Instance.Gate;
                    break;
                default:
                    break;
            }

            m_pathRenderer.sortingOrder = layerIndex == 0 ? METAL_SORT_ORDER : TRANSISTOR_SORT_ORDER;
            m_subRenderer.sortingOrder = m_pathRenderer.sortingOrder - 10;
            m_textRenderer.GetComponent<Renderer>().sortingOrder = m_pathRenderer.sortingOrder + 10;
            foreach (var r in m_dirRenderers) { r.sortingOrder = m_pathRenderer.sortingOrder + 5; }
            m_transferRenderer.sortingOrder = cellData.TransferType == TransferType.Via ? VIA_SORT_ORDER : GATE_SORT_ORDER;

            if (lookedUpEdge)
            {
                m_pathRenderer.sprite = pathData.Sprite;
                var angles = m_pathRenderer.transform.rotation.eulerAngles;
                angles.z = 90 * pathData.Turns;
                m_pathRenderer.transform.rotation = Quaternion.Euler(angles);

                m_flowMask.transform.rotation = Quaternion.Euler(angles);
                m_flowMask.sprite = pathData.Sprite;
            }

            UpdateFlowVisuals(cellData.FlowState, layerIndex);
        }

        private void RenderNTransistor(ref GridCell cellData, ref PathLibrary.AssembledPathData pathData, ref bool lookedUpEdge, int layerIndex, int col, int row)
        {
            var condensedEdges = EdgeUtility.CondenseEdges(cellData.Edges);
            SpriteDB.Instance.TransistorLibrary.Lookup(condensedEdges, out pathData);
            lookedUpEdge = true;
            m_pathRenderer.color = SpriteDB.Instance.NColor;

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
                            m_dirRenderers[i].sprite = SpriteDB.Instance.NSide;
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
                            m_dirRenderers[i].sprite = SpriteDB.Instance.PSide;
                        }
                    }
                }
            }
        }
    }
}
