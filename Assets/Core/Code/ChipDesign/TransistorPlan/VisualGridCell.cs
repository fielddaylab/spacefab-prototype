using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualGridCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_pathRenderer;

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
                default:
                    m_pathRenderer.sprite = null;
                    m_pathRenderer.color = Color.white;
                    break;
            }

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
