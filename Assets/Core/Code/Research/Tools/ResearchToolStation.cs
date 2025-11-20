using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scenes;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    [PreloadOrder(5)]
    public sealed class ResearchToolStation : BatchedComponent, IScenePreload {
        public CursorHint OffscreenClickHint;
        public GameObject EmptyGroup;
        public GameObject AvailableGroup;
        public ResearchToolsMask UnlockMask;
        [Required(ComponentLookupDirection.Children)] public ResearchTool Tool;

        [NonSerialized] public int StationIndex;
        [NonSerialized] public bool Unlocked;

        public IEnumerator<WorkSlicer.Result?> Preload() {
            ResearchToolUtility.SetStationUnlocked(this, UnlockMask == 0, true);
            if (UnlockMask != 0) {
                Find.State<ResearchToolState>().OnUnlockedToolsChanged.Register(UpdateUnlocked);
            }
            return null;
        }

        private void UpdateUnlocked(ResearchToolsMask mask) {
            ResearchToolUtility.SetStationUnlocked(this, UnlockMask == 0 || (mask & UnlockMask) == UnlockMask, false);
        }
    }

    static public partial class ResearchToolUtility {
        static public void SetStationUnlocked(ResearchToolStation station, bool unlocked, bool force) {
            if (!force && station.Unlocked == unlocked) {
                return;
            }

            GuiCommands.SetActive(station.EmptyGroup, !unlocked);
            GuiCommands.SetActive(station.AvailableGroup, unlocked);
            station.Unlocked = unlocked;
        }
    }
}