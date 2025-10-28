using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualGridCell : MonoBehaviour
    {
        private const int TRANSFER_SORT_ORDER = 200;
        private const int METAL_SORT_ORDER = 100;
        private const int TRANSISTOR_SORT_ORDER = 0;

        [SerializeField] private SpriteRenderer m_pathRenderer;
        [SerializeField] private SpriteRenderer m_subRenderer;
        [SerializeField] private TMP_Text m_textRenderer;
        [SerializeField] private SpriteRenderer m_transferRenderer;

        public void RefreshVisual(GridCell cellData, int layerIndex)
        {
            PathLibrary.AssembledPathData pathData = default;
            bool lookedUpEdge = false;

            // Render according to cell data
            switch (cellData.CellType)
            {
                case CellType.Metal:
                    SpriteDB.Instance.MetalLibrary.Lookup(EdgeUtility.CondenseEdges(cellData.Edges), out pathData);
                    lookedUpEdge = true;
                    break;
                case CellType.NTransistor:
                    SpriteDB.Instance.TransistorLibrary.Lookup(EdgeUtility.CondenseEdges(cellData.Edges), out pathData);
                    lookedUpEdge = true;
                    m_pathRenderer.color = SpriteDB.Instance.NColor;
                    break;
                case CellType.PTransistor:
                    SpriteDB.Instance.TransistorLibrary.Lookup(EdgeUtility.CondenseEdges(cellData.Edges), out pathData);
                    lookedUpEdge = true;
                    m_pathRenderer.color = SpriteDB.Instance.PColor;
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
                    m_pathRenderer.sprite = null;
                    m_subRenderer.sprite = null;
                    m_textRenderer.SetText("");
                    m_pathRenderer.color = Color.white;
                    break;
            }

            switch (cellData.TransferType)
            {
                case TransferType.Via:
                    m_transferRenderer.sprite = SpriteDB.Instance.Via;
                    break;
                case TransferType.Gate:
                    m_transferRenderer.sprite = SpriteDB.Instance.Gate;
                    break;
                default:
                    m_transferRenderer.sprite = null;
                    break;
            }

            m_pathRenderer.sortingOrder = layerIndex == 0 ? METAL_SORT_ORDER : TRANSISTOR_SORT_ORDER;
            m_subRenderer.sortingOrder = m_pathRenderer.sortingOrder - 10;
            m_textRenderer.GetComponent<Renderer>().sortingOrder = m_pathRenderer.sortingOrder + 10;
            m_transferRenderer.sortingOrder = TRANSFER_SORT_ORDER;

            if (lookedUpEdge)
            {
                m_pathRenderer.sprite = pathData.Sprite;
                var angles = m_pathRenderer.transform.rotation.eulerAngles;
                angles.z = 90 * pathData.Turns;
                m_pathRenderer.transform.rotation = Quaternion.Euler(angles);
            }
        }
    }
}
