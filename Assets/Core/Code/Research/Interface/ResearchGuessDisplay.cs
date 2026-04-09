using BeauUtil;
using FieldDay;
using FieldDay.Scenes;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    [PreloadOrder(150)]
    public sealed class ResearchGuessDisplay : MonoBehaviour, IScenePreload {
        public ResearchGuessGroup ElectricalGroup;
        public ResearchGuessGroup ThermalGroup;
        public ResearchGuessGroup DopantGroup;
        public ResearchGuessGroup SpecialGroup;
        public CursorHint CloseButton;

        [NonSerialized] public StringHash32 MaterialId;

        public ActionEvent OnClose = new ActionEvent();

        private void Awake() {
            CloseButton.onClick.Register(() => {
                OnClose.Invoke();
                gameObject.SetActive(false);
            });
        }

        private void OnEnable() {
            Find.State<ResearchSelectionState>().Locked = true;
        }

        private void OnDisable() {
            if (Game.IsShuttingDown) {
                return;
            }

            Find.State<ResearchSelectionState>().Locked = false;
            ElectricalGroup.gameObject.SetActive(false);
            ThermalGroup.gameObject.SetActive(false);
            SpecialGroup.gameObject.SetActive(false);
            DopantGroup.gameObject.SetActive(false);
            MaterialId = default;
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            return null;
        }
    }
}