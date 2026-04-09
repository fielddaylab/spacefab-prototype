using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scenes;
using FieldDay.SharedState;
using SpaceFab.ChipFab;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchToolState : SharedStateComponent {
        [Required] public ResearchStationSet Stations;

        [Header("Defaults")]
        public ResearchToolStation DefaultStation;

        [NonSerialized] public ResearchToolStation CurrentStation;
        [NonSerialized] public ResearchTool CurrentTool;
        [NonSerialized] public ResearchToolsMask CurrentUnlocks;

        public CastableEvent<ResearchToolsMask> OnUnlockedToolsChanged = new CastableEvent<ResearchToolsMask>(8);

        private void Awake() {
            Game.Scenes.QueueOnLoad(this, () => ResearchToolUtility.SetCurrentStation(DefaultStation));
        }
    }

    static public partial class ResearchToolUtility {
        static public void ResetTool(ResearchTool tool) {
            Assert.NotNullOrDestroyed(tool);

            if (!tool.isActiveAndEnabled) {
                return;
            }

            foreach (var slot in tool.Slots) {
                ResearchSlotUtility.FillInSlot(slot, null);
            }
            tool.OnReset.Invoke(tool);
        }

        static public void SetUnlocks(ResearchToolsMask unlocks) {
            ResearchToolState toolState = Find.State<ResearchToolState>();
            if (toolState.CurrentUnlocks != unlocks) {
                toolState.CurrentUnlocks = unlocks;
                toolState.OnUnlockedToolsChanged.Invoke(unlocks);
            }
        }
        
        static public void SetCurrentStation(ResearchToolStation station) {
            Assert.NotNullOrDestroyed(station);

            ResearchToolState toolState = Find.State<ResearchToolState>();
            if (toolState.CurrentStation != station) {
                ResearchSlotUtility.CancelCurrentDrag();
                if (toolState.CurrentTool) {
                    ResetTool(toolState.CurrentTool);
                }

                toolState.CurrentStation = station;
                toolState.CurrentTool = station.Tool;
                EnableStationTransitionZones(station.StationIndex);
            }
        }
    }
}