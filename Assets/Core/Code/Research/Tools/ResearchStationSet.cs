using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scenes;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchStationSet : MonoBehaviour, IScenePreload {
        public ResearchToolStation[] Stations;

        public IEnumerator<WorkSlicer.Result?> Preload() {
            for(int i = 0; i < Stations.Length; i++) {
                ResearchToolStation station = Stations[i];
                station.StationIndex = i;
                station.OffscreenClickHint.UserData = Stations[i];
                station.OffscreenClickHint.onClick.Register(ResearchToolUtility.HandleStationTransition);
            }
            return null;
        }
    }

    static public partial class ResearchToolUtility {
        static public void HandleStationTransition(CursorHint.EventData evtData) {
            ResearchToolStation station = (ResearchToolStation) evtData.Source.UserData;
            SetCurrentStation(station);
            ResearchUtility.MoveCameraTo(station.transform);
        }

        static public void DisableAllStationTransitionZones() {
            foreach (var station in Find.Components<ResearchToolStation>()) {
                station.OffscreenClickHint.gameObject.SetActive(false);
            }
        }

        static public void EnableStationTransitionZones(int stationIndex) {
            foreach (var station in Find.Components<ResearchToolStation>()) {
                station.OffscreenClickHint.CursorType = station.StationIndex < stationIndex ? "TransitionLeft" : "TransitionRight";
                station.OffscreenClickHint.gameObject.SetActive(Math.Abs(station.StationIndex - stationIndex) == 1);
            }
        }
    }
}