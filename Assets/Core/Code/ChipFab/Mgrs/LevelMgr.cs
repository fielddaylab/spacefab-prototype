using FieldDay;
using FieldDay.Rendering;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab.ChipFab
{
    public class LevelMgr : MonoBehaviour
    {
        public SideWaferDisplay Current;

        public SideWaferDisplay TargetSide;
        public AngledWaferDisplay TargetAngled;

        public ClickBox SubmitButton;

        public GameObject SuccessGroup;
        public GameObject FailureGroup;
        public TMP_Text PrecisionText;

        public ClickBox ReturnBtn;

        [HideInInspector] public WaferData TargetData;

        private void Awake()
        {
            if (Camera.main.GetComponent<PrimaryWorldCamera>())
            {
                Destroy(Camera.main.gameObject);
            }    
        }

        private void Start()
        {
            Game.Events.Register(GameEvents.WaferStateUpdated, HandleWaferStateUpdated);

            TargetData = ChipFabConfig.Instance.CurrLevel.TargetWafer();
            TargetSide.UpdateDisplay(TargetData);
            // TargetAngled.UpdateDisplay(TargetData);

            SubmitButton.OnMouseDown.AddListener(HandleSubmitClicked);
            ReturnBtn.OnMouseDown.AddListener(HandleReturnClicked);
        }

        #region Handlers

        private void HandleWaferStateUpdated()
        {
            if (DragMgr.WaferInstance) {
                Current.UpdateDisplay(DragMgr.WaferInstance.Data);
            }
            else
            {
                Current.UpdateDisplay(default);
            }

            SuccessGroup.SetActive(false);
            FailureGroup.SetActive(false);
            PrecisionText.gameObject.SetActive(false);
        }

        private void HandleSubmitClicked()
        {
            Evaluate();
        }

        private void HandleReturnClicked()
        {
            SceneManager.LoadScene("ChipFabLoader");
        }

        #endregion // Handlers
    
        private void Evaluate()
        {
            var currState = DragMgr.WaferInstance.Data;
            bool success = WaferData.IsEqual(currState, TargetData);

            SuccessGroup.SetActive(success);
            FailureGroup.SetActive(!success);
            PrecisionText.gameObject.SetActive(true);
            PrecisionText.SetText("Precision: " + (currState.Precision.Avg() * 100).ToString("#.##") + "%");

            Game.Events.Dispatch(GameEvents.WaferSubmitted);
        }
    }
}