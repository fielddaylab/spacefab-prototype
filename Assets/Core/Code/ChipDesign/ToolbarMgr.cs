using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public enum ToolType
    {
        None,
        DrawLinks,
        Erase
    }

    public class ToolbarMgr : MonoBehaviour
    {
        public static ToolbarMgr Instance;

        [SerializeField] private Button LayerButton;
        [SerializeField] private TMP_Text LayerText;

        public ToolType ActiveTool = ToolType.None;

        private void Awake()
        {
            if (Instance == null) { Instance = this; }

            Game.Events.Register(GameEvents.OnLayerChanged, HandleLayerChanged);
            LayerButton.onClick.AddListener(HandleLayerClicked);
        }

        private void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }

            Game.Events.Deregister(GameEvents.OnLayerChanged, HandleLayerChanged);
            LayerButton.onClick.RemoveListener(HandleLayerClicked);
        }

        #region Handlers

        private void HandleLayerClicked()
        {
            InteractionMgr.Instance.SetActiveLayer(InteractionMgr.Instance.ActiveLayer == GridInteractionLayer.Nodes ? GridInteractionLayer.Links : GridInteractionLayer.Nodes);
        }

        private void HandleLayerChanged()
        {
            switch (InteractionMgr.Instance.ActiveLayer)
            {
                case GridInteractionLayer.Nodes:
                    LayerText.SetText("+");
                    break;
                case GridInteractionLayer.Links:
                    LayerText.SetText("-");
                    break;
                default:
                    break;
            }
        }

        #endregion // Handlers
    }
}
