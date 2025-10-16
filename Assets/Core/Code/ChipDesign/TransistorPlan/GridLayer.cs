using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    [Serializable]
    public struct Dimensions
    {
        public int X;
        public int Y;

        public Dimensions(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Underlying data representation of a grid layer
    /// </summary>
    public class GridLayer : MonoBehaviour
    {
        public Dimensions Dimensions;
        private GridCell[,] m_Cells; // accessed in row, col order

        #region Constructor

        public GridLayer(int xDim, int yDim)
        {
            Dimensions = new Dimensions(xDim, yDim);
            m_Cells = new GridCell[xDim, yDim];
            for (int row = 0; row < yDim; row++)
            {
                for (int col = 0; col < xDim; col++)
                {
                    SetCell(col, row, new GridCell());
                }
            }
        }

        #endregion // Constructor

        #region Gets & Sets

        // Access x, y in row, col order
        public GridCell GetCell(int x, int y)
        {
            return m_Cells[y, x];
        }

        // Set cell at x, y in row, col order
        public void SetCell(int x, int y, GridCell cell)
        {
            m_Cells[y, x] = cell;
        }

        #endregion // Gets & Sets
    }
}