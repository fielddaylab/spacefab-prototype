using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualsMgr : MonoBehaviour
    {
        public static VisualsMgr Instance;

        public SpriteRenderer GridRenderer;
        public GridStack GridData;
        public VisualGridStack GridVisuals;

        public GameObject CellVisualsPrefab;
        public Transform CellVisualsContainer;

        private void Awake()
        {
            Instance = this;

            Game.Events.Register(GameEvents.OnLayoutChanged, HandleLayoutChanged);
            Game.Events.Register(GameEvents.NewGridStackCreated, HandleNewGridStackCreated);
        }

        private void Start()
        {
            GridVisuals = new VisualGridStack();

            RefreshGrid();
            RefreshCamera();
        }

        private void RefreshGrid()
        {
            GridRenderer.size = new Vector2(GridData.LayerDims.X, GridData.LayerDims.Y);
        }

        private void RefreshCamera()
        {
            // Move Camera to Grid
            Camera.main.transform.position = new Vector3(
                GridData.LayerDims.X / 2f,
                GridData.LayerDims.Y / 2f,
                Camera.main.transform.position.z
                );

            // Adjust zoom
            Camera.main.orthographicSize = GridData.LayerDims.X / 1.81f;
        }

        public void RefreshVisuals()
        {
            if (GridVisuals == null || GridVisuals.GridLayers == null || GridVisuals.GridLayers.Length == 0) { return; }

            // Render Metal Layer
            GridVisuals.GridLayers[0].RefreshAll();
            // Render Transistor Layer
            GridVisuals.GridLayers[1].RefreshAll();
        }

        #region Handlers

        private void HandleLayoutChanged()
        {
            RefreshVisuals();
        }

        private void HandleNewGridStackCreated()
        {
            if (GridVisuals.GridLayers != null && GridVisuals.GridLayers.Length != 0)
            {
                GridVisuals.Destroy();
            }
            GridVisuals.Init(GridData.LayerDims.X, GridData.LayerDims.Y, CellVisualsPrefab, CellVisualsContainer);

            RefreshGrid();
            RefreshCamera();
        }

        #endregion // Handlers
    }
}