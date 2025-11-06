using BeauUtil;
using FieldDay;
using FieldDay.UI;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchGuessDisplay : MonoBehaviour {
        public ResearchGuessGroup ElectricalGroup;
        public ResearchGuessGroup ThermalGroup;
        public ResearchGuessGroup SpecialGroup;
        public CursorHint CloseButton;

        [NonSerialized] public StringHash32 MaterialId;

        public ActionEvent OnClose = new ActionEvent();

        private void Awake() {
            ElectricalGroup.OnSelectionUpdated.Register(UpdateElectricGuess);
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
            MaterialId = default;
        }

        public void PopupElectrical(StringHash32 materialId) {
            MaterialId = materialId;
            gameObject.SetActive(true);
            ElectricalGroup.gameObject.SetActive(true);

            var guess = ResearchMaterialUtility.GetGuess(MaterialId);
            ElectricalGroup.PopulateInitialSelection(ResearchGuessGroup.GetElectricalGuessList(guess));
        }

        public void PopupThermal(StringHash32 materialId) {
            MaterialId = materialId;
            gameObject.SetActive(true);
            ThermalGroup.gameObject.SetActive(true);

            var guess = ResearchMaterialUtility.GetGuess(MaterialId);
            ThermalGroup.PopulateInitialSelection(ResearchGuessGroup.GetThermalGuessList(guess));
        }

        public void PopupSpecial(StringHash32 materialId) {
            MaterialId = materialId;
            gameObject.SetActive(true);
            SpecialGroup.gameObject.SetActive(true);

            var guess = ResearchMaterialUtility.GetGuess(MaterialId);
            SpecialGroup.PopulateInitialSelection(ResearchGuessGroup.GetSpecialGuessList(guess));
        }

        private void UpdateElectricGuess(ResearchSelectionList list) {
            var guess = ResearchMaterialUtility.GetGuess(MaterialId);
            ResearchGuessGroup.PopulateElectricalGuess(ref guess, list);
            ResearchMaterialUtility.SetGuess(MaterialId, guess);
        }

        private void UpdateThermalGuess(ResearchSelectionList list) {
            var guess = ResearchMaterialUtility.GetGuess(MaterialId);
            ResearchGuessGroup.PopulateThermalGuess(ref guess, list);
            ResearchMaterialUtility.SetGuess(MaterialId, guess);
        }

        private void UpdateSpecialGuess(ResearchSelectionList list) {
            var guess = ResearchMaterialUtility.GetGuess(MaterialId);
            ResearchGuessGroup.PopulateSpecialGuess(ref guess, list);
            ResearchMaterialUtility.SetGuess(MaterialId, guess);
        }
    }
}