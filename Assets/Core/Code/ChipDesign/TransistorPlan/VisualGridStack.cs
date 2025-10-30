using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign {
    public class VisualGridStack
    {
        public Dimensions LayerDims; // x and y dims of each layer
        [HideInInspector] public VisualGridLayer[] GridLayers; // layers ordered from highest to lowest

        public void Init(int xDim, int yDim, GameObject cellVisualsPrefab, Transform container)
        {
            LayerDims.X = xDim;
            LayerDims.Y = yDim;

            GridLayers = new VisualGridLayer[]
            {
                new VisualGridLayer(LayerDims.X, LayerDims.Y, GridStack.METAL_LAYER, cellVisualsPrefab, container),  // metal layer (highest)
                new VisualGridLayer(LayerDims.X, LayerDims.Y, GridStack.TRANSISTOR_LAYER, cellVisualsPrefab, container)   // transistor layer (lowest)
            };
        }

        public void Destroy()
        {
            GridLayers[GridStack.METAL_LAYER].Destroy();
            GridLayers[GridStack.TRANSISTOR_LAYER].Destroy();
        }
    }
}
