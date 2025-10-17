using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualsMgr : MonoBehaviour
    {
        public SpriteRenderer GridRenderer;
        public GridStack GridData;

        private void Start()
        {
            UpdateGrid();
            UpdateCamera();
        }

        private void UpdateGrid()
        {
            GridRenderer.size = new Vector2(GridData.LayerDims.X, GridData.LayerDims.Y);
        }

        private void UpdateCamera()
        {
            // Move Camera to Grid
            Camera.main.transform.position = new Vector3(
                GridData.LayerDims.X / 2f,
                GridData.LayerDims.Y / 2f,
                Camera.main.transform.position.z
                );
        }
    }
}