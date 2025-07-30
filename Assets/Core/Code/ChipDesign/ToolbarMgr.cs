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
        Erase,
        DrawInNodes,
        DrawOutNodes,
        DrawVPlusNodes,
        DrawVMinusNodes,
        DrawANodes,
        DrawBNodes
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

        [SerializeField] private Button DrawInNodesButton;
        [SerializeField] private Button DrawVPlusNodesButton;
        [SerializeField] private Button DrawVMinusNodesButton;
        [SerializeField] private Button DrawANodesButton;
        [SerializeField] private Button DrawBNodesButton;
        [SerializeField] private Button DrawOutNodesButton;


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
            DrawInNodesButton.onClick.AddListener(HandleDrawInNodesClicked);
            DrawOutNodesButton.onClick.AddListener(HandleDrawOutNodesClicked);
            DrawVPlusNodesButton.onClick.AddListener(HandleDrawVPlusNodesClicked);
            DrawVMinusNodesButton.onClick.AddListener(HandleDrawVMinusNodesClicked);
            DrawANodesButton.onClick.AddListener(HandleDrawANodesClicked);
            DrawBNodesButton.onClick.AddListener(HandleDrawBNodesClicked);
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
            DrawInNodesButton.onClick.RemoveListener(HandleDrawInNodesClicked);
            DrawOutNodesButton.onClick.RemoveListener(HandleDrawOutNodesClicked);
            DrawVPlusNodesButton.onClick.RemoveListener(HandleDrawVPlusNodesClicked);
            DrawVMinusNodesButton.onClick.RemoveListener(HandleDrawVMinusNodesClicked);
            DrawANodesButton.onClick.RemoveListener(HandleDrawANodesClicked);
            DrawBNodesButton.onClick.RemoveListener(HandleDrawBNodesClicked);
            EraseButton.onClick.RemoveListener(HandleEraseClicked);
            DrawLinksButton.onClick.RemoveListener(HandleDrawLinksClicked);
        }

        private void Start()
        {
            DrawInNodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.IN));
            DrawOutNodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.OUT));
            DrawVPlusNodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.VPLUS));
            DrawVMinusNodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.VMINUS));
            DrawANodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.A));
            DrawBNodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.B));
            DrawNNodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.NNODE));
            DrawPNodesButton.gameObject.SetActive(InteractionMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.PNODE));

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

        private void HandleDrawInNodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawInNodes);
        }

        private void HandleDrawOutNodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawOutNodes);
        }

        private void HandleDrawVPlusNodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawVPlusNodes);
        }

        private void HandleDrawVMinusNodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawVMinusNodes);
        }

        private void HandleDrawANodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawANodes);
        }

        private void HandleDrawBNodesClicked()
        {
            InteractionMgr.Instance.SetActiveTool(ToolType.DrawBNodes);
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
