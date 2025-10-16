using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    /// <summary>
    /// Underlying data representation of the whole grid stack, containing both metal and transistor layers
    /// </summary>
    public class GridStack : MonoBehaviour
    {
        public Dimensions LayerDims; // x and y dims of each layer
        [HideInInspector] public GridLayer[] GridLayers; // layers ordered from highest to lowest

        #region Unity Callbacks

        private void Awake()
        {
            GridLayers = new GridLayer[]
            {
                new GridLayer(LayerDims.X, LayerDims.Y),  // metal layer (highest)
                new GridLayer(LayerDims.X, LayerDims.Y)   // transistor layer (lowest)
            };
        }

        #endregion // Unity Callbacks

        #region Queries

        public bool InBounds(int x, int y)
        {
            if (x < 0 || y < 0) { return false; }
            if (x >= LayerDims.X || y >= LayerDims.Y) { return false; }

            return true;
        }

        #endregion // Queries
    }
}