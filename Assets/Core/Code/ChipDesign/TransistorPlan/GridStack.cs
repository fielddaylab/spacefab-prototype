using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    /// <summary>
    /// Underlying data representation of the whole grid stack, containing both metal and transistor layers
    /// Inputs in grid start at 0, 0 in the bottom left. Row indices increase from bottom to top.
    /// </summary>
    public class GridStack : MonoBehaviour
    {
        public static GridStack Instance;

        public Dimensions LayerDims; // x and y dims of each layer
        [HideInInspector] public GridLayer[] GridLayers; // layers ordered from highest to lowest

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (LevelMgr.Instance.CurrLevelData.GetGridConfig() != null)
            {
                LoadConfig(LevelMgr.Instance.CurrLevelData.GetGridConfig());
            }
            else
            { 
                GridLayers = new GridLayer[]
                {
                    new GridLayer(LayerDims.X, LayerDims.Y),  // metal layer (highest)
                    new GridLayer(LayerDims.X, LayerDims.Y)   // transistor layer (lowest)
                };
            }

        }

        #endregion // Unity Callbacks

        #region Loading

        private void LoadConfig(GridStackConfig config)
        {
            LayerDims = config.LayerDims;
            GridLayers = new GridLayer[]
            {
                new GridLayer(LayerDims.X, LayerDims.Y),  // metal layer (highest)
                new GridLayer(LayerDims.X, LayerDims.Y)   // transistor layer (lowest)
            };
            for (int i = 0; i < config.Cells.Length; i++)
            {
                LoadCellConfig(config.Cells[i]);
            }
        }

        private void LoadCellConfig(GridCellConfig config)
        {
            var cell = GridLayers[config.LayerIndex].GetCell(config.ColumnIndex, config.RowIndex);
            cell.LoadCellConfig(config);
            GridLayers[config.LayerIndex].SetCell(config.ColumnIndex, config.RowIndex, cell);
        }

        #endregion // Loading

        #region Queries

        public bool InBounds(int x, int y)
        {
            if (x < 0 || y < 0) { return false; }
            if (x >= LayerDims.X || y >= LayerDims.Y) { return false; }

            return true;
        }

        #endregion // Queries
    }

    public static class GridUtility
    {
        public static EdgeDir DirFromToCell(Vector2Int fromPos, Vector2Int toPos)
        {
            var dif = toPos - fromPos;

            if (dif.x == 1)
            {
                return EdgeDir.EAST;
            }
            else if (dif.x == -1)
            {
                return EdgeDir.WEST;
            }
            else if (dif.y == 1)
            {
                return EdgeDir.NORTH;
            }
            else // (dif.y == 0)
            {
                return EdgeDir.SOUTH;
            }
        }
    }
}