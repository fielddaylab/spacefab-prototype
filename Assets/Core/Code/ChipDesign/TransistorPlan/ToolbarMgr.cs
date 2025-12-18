using FieldDay;
using FieldDay.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public enum ToolType
    {
        None,
        DrawNNodes,
        DrawPNodes,
        DrawLinks, // DrawMetal
        Erase,
        DrawInNodes,
        DrawOutNodes,
        DrawVPlusNodes,
        DrawVMinusNodes,
        DrawANodes,
        DrawBNodes,
        DrawVia,
        DrawGate,
        DrawOutXNodes,
        DrawOutYNodes,
    }

    public enum GridInteractionLayer
    {
        Metal,
        Transistor
    }

    public class ToolbarMgr : MonoBehaviour
    {
        public static ToolbarMgr Instance;

        [Header("Layer")]
        [SerializeField] private Button LayerButton;
        [SerializeField] private Sprite LayerBtnTopImg;
        [SerializeField] private Sprite LayerBtnBottomImg;
        [SerializeField] private TMP_Text LayerText;
        [SerializeField] private TMP_Text LayerLabelText;

        [Header("Common")]
        [SerializeField] private Button EraseButton;
        [SerializeField] private Button DrawViaButton;
        [SerializeField] private Button DrawGateButton;
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
        [SerializeField] private Button DrawOutXNodesButton;
        [SerializeField] private Button DrawOutYNodesButton;


        [Header("Links")]
        [SerializeField] private GameObject DrawLinksGroup;
        [SerializeField] private Button DrawLinksButton;

        [Header("Nav")]
        [SerializeField] private Button ReturnMenuButton;
        [SerializeField] private string m_menuScene;
        [SerializeField] private TMP_Text m_titleText;

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
            DrawOutXNodesButton.onClick.AddListener(HandleDrawOutXNodesClicked);
            DrawOutYNodesButton.onClick.AddListener(HandleDrawOutYNodesClicked);
            DrawVPlusNodesButton.onClick.AddListener(HandleDrawVPlusNodesClicked);
            DrawVMinusNodesButton.onClick.AddListener(HandleDrawVMinusNodesClicked);
            DrawANodesButton.onClick.AddListener(HandleDrawANodesClicked);
            DrawBNodesButton.onClick.AddListener(HandleDrawBNodesClicked);
            EraseButton.onClick.AddListener(HandleEraseClicked);
            DrawLinksButton.onClick.AddListener(HandleDrawLinksClicked);
            DrawViaButton.onClick.AddListener(HandleDrawViaClicked);
            DrawGateButton.onClick.AddListener(HandleDrawGateClicked);

            ReturnMenuButton.onClick.AddListener(HandleReturnMenuClicked);

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
            DrawOutXNodesButton.onClick.RemoveListener(HandleDrawOutXNodesClicked);
            DrawOutYNodesButton.onClick.RemoveListener(HandleDrawOutYNodesClicked);
            DrawVPlusNodesButton.onClick.RemoveListener(HandleDrawVPlusNodesClicked);
            DrawVMinusNodesButton.onClick.RemoveListener(HandleDrawVMinusNodesClicked);
            DrawANodesButton.onClick.RemoveListener(HandleDrawANodesClicked);
            DrawBNodesButton.onClick.RemoveListener(HandleDrawBNodesClicked);
            EraseButton.onClick.RemoveListener(HandleEraseClicked);
            DrawLinksButton.onClick.RemoveListener(HandleDrawLinksClicked);
            DrawViaButton.onClick.RemoveListener(HandleDrawViaClicked);
            DrawGateButton.onClick.RemoveListener(HandleDrawGateClicked);

            ReturnMenuButton.onClick.RemoveListener(HandleReturnMenuClicked);
        }

        private void Start()
        {
            if (InteractMgr.Instance != null)
            {
                DrawInNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.IN));
                DrawOutNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.OUT));
                DrawOutXNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.OUTX));
                DrawOutYNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.OUTY));
                DrawVPlusNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.VPLUS));
                DrawVMinusNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.VMINUS));
                DrawANodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.A));
                DrawBNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.B));
                DrawNNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.NNODE));
                DrawPNodesButton.gameObject.SetActive(LevelMgr.Instance.CurrLevelData.GetPlaceables().Contains(Placeable.PNODE));
            }

            m_titleText.SetText(LevelMgr.Instance.CurrLevelData.GetTitle());
        }

        #region Handlers

        private void HandleReturnMenuClicked()
        {
            Game.Scenes.LoadMainScene(SceneUtils.GetSceneByName(m_menuScene));
        }

        private void HandleLayerClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveLayer(InteractMgr.Instance.ActiveLayer == GridInteractionLayer.Transistor ? GridInteractionLayer.Metal : GridInteractionLayer.Transistor);
            }
        }

        private void HandleLayerChanged()
        {
            var activeLayer = InteractMgr.Instance.ActiveLayer;
            switch (activeLayer)
            {
                case GridInteractionLayer.Transistor:
                    //LayerText.SetText("+");
                    LayerButton.image.sprite = LayerBtnBottomImg;
                    //LayerLabelText.SetText("Nodes");
                    //DrawNodesGroup.SetActive(true);
                    //if (InteractMgr.Instance != null) { DrawLinksGroup.SetActive(false); }
                    break;
                case GridInteractionLayer.Metal:
                    //LayerText.SetText("-");
                    LayerButton.image.sprite = LayerBtnTopImg;
                    //LayerLabelText.SetText("Links");
                    //DrawNodesGroup.SetActive(false);
                    //if (InteractMgr.Instance != null) { DrawLinksGroup.SetActive(true); }
                    break;
                default:
                    break;
            }

            // InteractMgr.Instance?.SetActiveTool(ToolType.None);
        }

        private void HandleToolChanged()
        {
            if (InteractMgr.Instance != null)
            {
                ActiveToolText.SetText(InteractMgr.Instance.ActiveTool.ToString());
            }
        }

        private void HandleDrawNNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawNNodes);
                InteractMgr.Instance.SetActiveLayer(GridInteractionLayer.Transistor);
            }
        }

        private void HandleDrawPNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawPNodes);
                InteractMgr.Instance.SetActiveLayer(GridInteractionLayer.Transistor);
            }
        }

        private void HandleDrawInNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawInNodes);
            }
        }

        private void HandleDrawOutNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawOutNodes);
            }
        }

        private void HandleDrawOutXNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawOutXNodes);
            }
        }

        private void HandleDrawOutYNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawOutYNodes);
            }
        }

        private void HandleDrawVPlusNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawVPlusNodes);
            }
        }

        private void HandleDrawVMinusNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawVMinusNodes);
            }
        }

        private void HandleDrawANodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawANodes);
            }
        }

        private void HandleDrawBNodesClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawBNodes);
            }
        }

        private void HandleEraseClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.Erase);
            }
        }

        private void HandleDrawLinksClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawLinks);
                InteractMgr.Instance.SetActiveLayer(GridInteractionLayer.Metal);
            }
        }

        private void HandleDrawViaClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawVia);
            }
        }

        private void HandleDrawGateClicked()
        {
            if (InteractMgr.Instance != null)
            {
                InteractMgr.Instance.SetActiveTool(ToolType.DrawGate);
            }
        }

        #endregion // Handlers
    }
}