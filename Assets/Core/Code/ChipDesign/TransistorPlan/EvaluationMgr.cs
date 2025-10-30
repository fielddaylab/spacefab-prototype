using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public enum FlowState
    {
        Empty,
        Hi,
        Lo,
        Unstable
    }

    public class EvaluationMgr : MonoBehaviour
    {
        #region Inspector

        public Button EvaluateButton;

        [Header("Results")]
        public GameObject ResultPanel;
        public TMP_Text ResultHeaderText;
        public TMP_Text ResultSubText;
        public Button ResultCloseButton;

        #endregion // Inspector

        #region Unity Callbacks

        private void Awake()
        {
            EvaluateButton.onClick.AddListener(HandleEvaluateClicked);
            ResultCloseButton.onClick.AddListener(HandleResultCloseClicked);

            ResultPanel.SetActive(false);
        }

        #endregion // Unity Callbacks

        #region Helpers

        private void Evaluate()
        {
            // TODO: Create Topological Map

            

            // TODO: Check for cycles
            bool hasCycles = true;
            
            if (hasCycles)
            {
                EvaluationInvalid();
            }
            else
            {
                // TODO: Run Input Suite
            }
        }

        private void EvaluationSuccess()
        {
            ResultHeaderText.SetText("Success");
            ResultPanel.SetActive(true);

            Game.Events.Dispatch(GameEvents.OnResultsDisplayed);
        }

        private void EvaluationInvalid()
        {
            ResultHeaderText.SetText("Invalid");
            ResultPanel.SetActive(true);

            Game.Events.Dispatch(GameEvents.OnResultsDisplayed);
        }

        private void EvaluationFailure()
        {
            ResultHeaderText.SetText("Failure");
            ResultPanel.SetActive(true);

            Game.Events.Dispatch(GameEvents.OnResultsDisplayed);
        }

        #endregion // Helpers

        #region Handlers

        private void HandleEvaluateClicked()
        {
            Evaluate();
        }

        private void HandleResultCloseClicked()
        {
            ResultPanel.SetActive(false);

            Game.Events.Dispatch(GameEvents.OnResultsHidden);
        }

        #endregion // Handlers
    }
}