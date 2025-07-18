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
        DrawNNodes,
        DrawPNodes,
        DrawLinks,
        Erase
    }

    public class ToolbarMgr : MonoBehaviour
    {
        public static ToolbarMgr Instance;


        [Header("Layer")]
        [SerializeField] private Button LayerButton;
        [SerializeField] private TMP_Text LayerText;
        [SerializeField] private TMP_Text LayerLabelText;

        [Header("Common")]
        [SerializeField] private Button EraseButton;
        [SerializeField] private TMP_Text ActiveToolText;

        [Header("Nodes")]
        [SerializeField] private GameObject DrawNodesGroup;
        [SerializeField] private Button DrawNNodesButton;
        [SerializeField] private Button DrawPNodesButton;

        [Header("Links")]
        [SerializeField] private GameObject DrawLinksGroup;
        [SerializeField] private Button DrawLinksButton;

        private void Awake()
        {
            if (Instance == null) { Instance = this; }

            Game.Events.Register(GameEvents.OnLayerChanged, HandleLayerChanged);
            Game.Events.Register(GameEvents.OnToolChanged, HandleToolChanged);
            LayerButton.onClick.AddListener(HandleLayerClicked);
            DrawNNodesButton.onClick.AddListener(HandleDrawNNodesClicked);
            DrawPNodesButton.onClick.AddListener(HandleDrawPNodesClicked);
            EraseButton.onClick.AddListener(HandleEraseClicked);
            DrawLinksButton.onClick.AddListener(HandleDrawLinksClicked);
        }

        private void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }

            Game.Events.Deregister(GameEvents.OnLayerChanged, HandleLayerChanged);
            Game.Events.Deregister(GameEvents.OnToolChanged, HandleToolChanged);
            LayerButton.onClick.RemoveListener(HandleLayerClicked);
            DrawNNodesButton.onClick.RemoveListener(HandleDrawNNodesClicked);
            DrawPNodesButton.onClick.RemoveListener(HandleDrawPNodesClicked);
            EraseButton.onClick.RemoveListener(HandleEraseClicked);
            DrawLinksButton.onClick.RemoveListener(HandleDrawLinksClicked);
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
                    LayerLabelText.SetText("Nodes");
                    DrawNodesGroup.SetActive(true);
                    DrawLinksGroup.SetActive(false);
                    break;
                case GridInteractionLayer.Links:
                    LayerText.SetText("-");
                    LayerLabelText.SetText("Links");
                    DrawNodesGroup.SetActive(false);
                    DrawLinksGroup.SetActive(true);
                    break;
                default:
                    break;
            }
            InteractionMgr.Instance.SetActiveTool(ToolType.None);
        }

        private void HandleToolChanged()
        {
            ActiveToolText.SetText(InteractionMgr.Instance.ActiveTool.ToString());
        }

        private void HandleDrawNNodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawNNodes);
        }

        private void HandleDrawPNodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawPNodes);
        }

        private void HandleEraseClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.Erase);
        }

        private void HandleDrawLinksClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawLinks);
        }

        #endregion // Handlers
    }
}
