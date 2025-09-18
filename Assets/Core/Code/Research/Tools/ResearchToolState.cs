using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scenes;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchToolState : SharedStateComponent {
        public Transform ToolsRoot;
        public TMP_Text ToolTitle;

        [Header("Defaults")]
        public ResearchTool DefaultTool;

        [NonSerialized] public ResearchTool CurrentTool;

        private void Awake() {
            Game.Scenes.QueueOnLoad(this, () => ResearchToolUtility.SetCurrentTool(DefaultTool));
        }
    }

    static public partial class ResearchToolUtility {
        static public void SetCurrentTool(ResearchTool tool) {
            ResearchToolState toolState = Find.State<ResearchToolState>();
            if (toolState.CurrentTool != tool) {
                ResearchSlotUtility.CancelCurrentDrag();
                if (toolState.CurrentTool) {
                    foreach(var slot in toolState.CurrentTool.Slots) {
                        ResearchSlotUtility.FillInSlot(slot, null);
                    }
                    if (toolState.CurrentTool.OutputSlot) {
                        ResearchSlotUtility.FillInSlot(toolState.CurrentTool.OutputSlot, null);
                    }
                    toolState.CurrentTool.gameObject.SetActive(false);
                }
                toolState.CurrentTool = tool;
                if (toolState.CurrentTool) {
                    toolState.ToolTitle.SetText(toolState.CurrentTool.ToolName);
                    toolState.CurrentTool.gameObject.SetActive(true);
                } else {
                    toolState.ToolTitle.gameObject.SetActive(false);
                }

                foreach (var button in Find.Components<ResearchToolButton>()) {
                    if (button.Locked) {
                        continue;
                    }
                    bool isSelected = button.Tool == toolState.CurrentTool;
                    button.Region.enabled = !isSelected;
                    button.Image.color = isSelected ? button.SelectedColor : button.UnselectedColor;
                }
            }
        }
    }
}