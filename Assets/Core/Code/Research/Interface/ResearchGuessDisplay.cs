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
            ElectricalGroup.OnSelectionUpdated.Register(UpdateElectricGuess);
            DopantGroup.OnSelectionUpdated.Register(UpdateDopantGuess);
            ThermalGroup.OnSelectionUpdated.Register(UpdateThermalGuess);
            SpecialGroup.OnSelectionUpdated.Register(UpdateSpecialGuess);
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

        public void PopupElectrical(StringHash32 materialId) {
            MaterialId = materialId;
            gameObject.SetActive(true);
            ThermalGroup.gameObject.SetActive(false);
            SpecialGroup.gameObject.SetActive(false);
            DopantGroup.gameObject.SetActive(false);
            ElectricalGroup.gameObject.SetActive(true);

            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            ElectricalGroup.PopulateInitialSelection(ResearchGuessGroup.GetElectricalGuessList(guess));
        }

        public void PopupThermal(StringHash32 materialId) {
            MaterialId = materialId;
            gameObject.SetActive(true);
            ElectricalGroup.gameObject.SetActive(false);
            SpecialGroup.gameObject.SetActive(false);
            DopantGroup.gameObject.SetActive(false);
            ThermalGroup.gameObject.SetActive(true);

            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            ThermalGroup.PopulateInitialSelection(ResearchGuessGroup.GetThermalGuessList(guess));
        }

        public void PopupDopant(StringHash32 materialId) {
            MaterialId = materialId;
            gameObject.SetActive(true);
            ElectricalGroup.gameObject.SetActive(false);
            SpecialGroup.gameObject.SetActive(false);
            ThermalGroup.gameObject.SetActive(false);
            DopantGroup.gameObject.SetActive(true);

            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            DopantGroup.PopulateInitialSelection(ResearchGuessGroup.GetDopantGuessList(guess));
        }

        public void PopupSpecial(StringHash32 materialId) {
            MaterialId = materialId;
            gameObject.SetActive(true);
            ThermalGroup.gameObject.SetActive(false);
            ElectricalGroup.gameObject.SetActive(false);
            DopantGroup.gameObject.SetActive(false);
            SpecialGroup.gameObject.SetActive(true);

            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            SpecialGroup.PopulateInitialSelection(ResearchGuessGroup.GetSpecialGuessList(guess));
        }

        private void UpdateElectricGuess(ResearchSelectionList list) {
            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            ResearchGuessGroup.PopulateElectricalGuess(ref guess, list);
            ResearchMaterialUtility.SetObservations(MaterialId, guess);
        }

        private void UpdateDopantGuess(ResearchSelectionList list) {
            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            ResearchGuessGroup.PopulateDopantGuess(ref guess, list);
            ResearchMaterialUtility.SetObservations(MaterialId, guess);
        }

        private void UpdateThermalGuess(ResearchSelectionList list) {
            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            ResearchGuessGroup.PopulateThermalGuess(ref guess, list);
            ResearchMaterialUtility.SetObservations(MaterialId, guess);
        }

        private void UpdateSpecialGuess(ResearchSelectionList list) {
            var guess = ResearchMaterialUtility.GetObservations(MaterialId);
            ResearchGuessGroup.PopulateSpecialGuess(ref guess, list);
            ResearchMaterialUtility.SetObservations(MaterialId, guess);
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            ResearchToolsMask unlocks = Find.State<ResearchToolState>().CurrentUnlocks;
            if ((unlocks & ResearchToolsMask.Thermal) == 0) {
                ResearchGuessButtonWidget semiButton = ElectricalGroup.Buttons[1];
                CanvasGroup group = semiButton.EnsureComponent<CanvasGroup>();
                group.alpha = 0.25f;
                group.blocksRaycasts = false;
                semiButton.GetComponentInChildren<TMP_Text>().SetText("???");

                ResearchGuessButtonWidget condButton = ElectricalGroup.Buttons[0];
                condButton.Collider.GetComponent<CursorHint>().Tooltip = "The material conducts electricity.";
            }
            return null;
        }
    }
}