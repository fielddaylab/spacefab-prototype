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

    public enum LinkType
    {
        Grey,
        Gold
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

        [SerializeField] private GameObject DrawFloorLinksGroup;
        [SerializeField] private Button DrawGreyLinksButton;
        [SerializeField] private Button DrawGoldLinksButton;

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

            DrawGreyLinksButton.onClick.AddListener(HandleDrawGreyLinksClicked);
            DrawGoldLinksButton.onClick.AddListener(HandleDrawGoldLinksClicked);
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
            if (InteractionMgr.Instance != null)
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
            else
            {
                DrawInNodesButton.gameObject.SetActive(false);
                DrawOutNodesButton.gameObject.SetActive(false);
                DrawVPlusNodesButton.gameObject.SetActive(false);
                DrawVMinusNodesButton.gameObject.SetActive(false);
                DrawANodesButton.gameObject.SetActive(false);
                DrawBNodesButton.gameObject.SetActive(false);
                DrawNNodesButton.gameObject.SetActive(false);
                DrawPNodesButton.gameObject.SetActive(false);
                EraseButton.gameObject.SetActive(FloorInteractionMgr.Instance.ActiveLayer == GridInteractionLayer.Links);
            }
        }

        #region Handlers

        private void HandleLayerClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveLayer(InteractionMgr.Instance.ActiveLayer == GridInteractionLayer.Nodes ? GridInteractionLayer.Links : GridInteractionLayer.Nodes);
            }
            else if (FloorInteractionMgr.Instance != null)
            {
                FloorInteractionMgr.Instance.SetActiveLayer(FloorInteractionMgr.Instance.ActiveLayer == GridInteractionLayer.Nodes ? GridInteractionLayer.Links : GridInteractionLayer.Nodes);
            }
        }

        private void HandleLayerChanged()
        {
            var activeLayer = InteractionMgr.Instance != null ? InteractionMgr.Instance.ActiveLayer : FloorInteractionMgr.Instance.ActiveLayer;
            switch (activeLayer)
            {
                case GridInteractionLayer.Nodes:
                    LayerText.SetText("+");
                    LayerLabelText.SetText("Nodes");
                    DrawNodesGroup.SetActive(true);
                    if (InteractionMgr.Instance != null) { DrawLinksGroup.SetActive(false); }
                    if (FloorInteractionMgr.Instance != null) { 
                        EraseButton.gameObject.SetActive(false);
                        DrawFloorLinksGroup.SetActive(false); 
                    }
                    break;
                case GridInteractionLayer.Links:
                    LayerText.SetText("-");
                    LayerLabelText.SetText("Links");
                    DrawNodesGroup.SetActive(false);
                    if (InteractionMgr.Instance != null) { DrawLinksGroup.SetActive(true); }
                    if (FloorInteractionMgr.Instance != null) {
                        EraseButton.gameObject.SetActive(true);
                        DrawFloorLinksGroup.SetActive(true);
                    }
                    break;
                default:
                    break;
            }

            InteractionMgr.Instance?.SetActiveTool(ToolType.None);
            FloorInteractionMgr.Instance?.SetActiveTool(ToolType.None);
        }

        private void HandleToolChanged()
        {
            if (InteractionMgr.Instance != null)
            {
                ActiveToolText.SetText(InteractionMgr.Instance.ActiveTool.ToString());
            }
            else
            {
                ActiveToolText.SetText(FloorInteractionMgr.Instance.ActiveTool.ToString());
            }
        }

        private void HandleDrawNNodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawNNodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawNNodes);
            }
        }

        private void HandleDrawPNodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawPNodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawPNodes);
            }
        }

        private void HandleDrawInNodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawInNodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawInNodes);
            }
        }

        private void HandleDrawOutNodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawOutNodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawOutNodes);
            }
        }

        private void HandleDrawVPlusNodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawVPlusNodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawVPlusNodes);
            }
        }

        private void HandleDrawVMinusNodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawVMinusNodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawVMinusNodes);
            }
        }

        private void HandleDrawANodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawANodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawANodes);
            }
        }

        private void HandleDrawBNodesClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawBNodes);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawBNodes);
            }
        }

        private void HandleEraseClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.Erase);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.Erase);
            }
        }

        private void HandleDrawLinksClicked()
        {
            if (InteractionMgr.Instance != null)
            {
                InteractionMgr.Instance.SetActiveTool(ToolType.DrawLinks);
            }
            else
            {
                FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawLinks);
            }
        }

        private void HandleDrawGreyLinksClicked()
        {
            FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawLinks);
            FloorInteractionMgr.Instance.ActiveLinkType = LinkType.Grey;
        }

        private void HandleDrawGoldLinksClicked()
        {
            FloorInteractionMgr.Instance.SetActiveTool(ToolType.DrawLinks);
            FloorInteractionMgr.Instance.ActiveLinkType = LinkType.Gold;
        }

        #endregion // Handlers
    }
}
