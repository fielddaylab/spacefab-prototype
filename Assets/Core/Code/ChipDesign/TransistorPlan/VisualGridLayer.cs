using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualGridLayer
    {
        public Dimensions Dimensions;
        public int LayerIndex;
        private VisualGridCell[,] m_Cells; // accessed in row, col order

        #region Constructor & Cleanup

        public VisualGridLayer(int xDim, int yDim, int layerIndex, GameObject cellVisualsPrefab, Transform container)
        {
            Dimensions = new Dimensions(xDim, yDim);
            LayerIndex = layerIndex;
            m_Cells = new VisualGridCell[yDim, xDim];
            float cellOffset = 0.5f;
            for (int row = 0; row < yDim; row++)
            {
                for (int col = 0; col < xDim; col++)
                {
                    var cell = GameObject.Instantiate(cellVisualsPrefab, container).GetComponent<VisualGridCell>();
                    cell.transform.position = new Vector3(col + cellOffset, row + cellOffset, 0);
                    cell.gameObject.name = "Cell Visual (" + col + ", " + row + ")";
                    SetCell(col, row, cell);
                }
            }
        }

        public void Destroy()
        {
            for (int row = 0; row < Dimensions.Y; row++)
            {
                for (int col = 0; col < Dimensions.X; col++)
                {
                    GameObject.Destroy(m_Cells[row, col]);
                }
            }
        }

        #endregion // Constructor & Cleanup

        #region Gets & Sets

        // Access x, y in row, col order
        public VisualGridCell GetCell(int x, int y)
        {
            return m_Cells[y, x];
        }

        // Access x, y in row, col order
        public VisualGridCell GetCell(Vector2Int coord)
        {
            return m_Cells[coord.y, coord.x];
        }

        // Set cell at x, y in row, col order
        public void SetCell(int x, int y, VisualGridCell cell)
        {
            m_Cells[y, x] = cell;
        }

        // Set cell at x, y in row, col order
        public void SetCell(Vector2Int coord, VisualGridCell cell)
        {
            m_Cells[coord.y, coord.x] = cell;
        }

        #endregion // Gets & Sets

        #region Refresh

        public void RefreshAll()
        {
            // Cell by cell, update renderer with data
            for (int row = 0; row < Dimensions.Y; row++)
            {
                for (int col = 0; col < Dimensions.X; col++)
                {
                    var cell = GridStack.Instance.GridLayers[LayerIndex].GetCell(col, row);
                    m_Cells[row, col].RefreshVisual(cell, LayerIndex, col, row);
                }
            }
        }

        #endregion // Refresh
    }
}
