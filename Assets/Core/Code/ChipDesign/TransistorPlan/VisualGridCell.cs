using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualGridCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_renderer;

        public void RefreshVisual(GridCell cellData, int layerIndex)
        {
            // Render according to cell data
            switch (cellData.CellType)
            {
                case CellType.Metal:
                    m_renderer.sprite = SpriteDB.Instance.Metal;
                    break;
                case CellType.NTransistor:
                    m_renderer.sprite = SpriteDB.Instance.Transistor;
                    m_renderer.color = SpriteDB.Instance.NColor;
                    break;
                case CellType.PTransistor:
                    m_renderer.sprite = SpriteDB.Instance.Transistor;
                    m_renderer.color = SpriteDB.Instance.PColor;
                    break;
                default:
                    m_renderer.sprite = null;
                    m_renderer.color = Color.white;
                    break;
            }
        }
    }
}
