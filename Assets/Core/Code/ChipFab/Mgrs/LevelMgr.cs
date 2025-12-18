using BeauUtil;
using FieldDay;
using FieldDay.Rendering;
using FieldDay.Scenes;
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
        public static LevelMgr Instance;

        public SideWaferDisplay Current;

        public SideWaferDisplay TargetSide;
        public AngledWaferDisplay TargetAngled;

        public ClickBox SubmitButton;

        public GameObject ResultsGroup;
        public GameObject SuccessGroup;
        public GameObject FailureGroup;
        public TMP_Text PrecisionText;
        public TMP_Text TimeText;
        public TMP_Text CycleText;

        public ClickBox ReturnBtn;

        [HideInInspector] public WaferData TargetData;

        private void Awake()
        {
            Instance = this;
            if (Camera.main.GetComponent<PrimaryWorldCamera>())
            {
                Destroy(Camera.main.gameObject);
            }    
        }

        private void Start()
        {
            Game.Events.Register(GameEvents.WaferStateUpdated, HandleWaferStateUpdated);
            Game.Events.Register(GameEvents.WaferStateUndone, HandleWaferStateUpdated);

            TargetData = ChipFabConfig.Instance.CurrLevel.TargetWafer();
            TargetSide.UpdateDisplay(TargetData);
            // TargetAngled.UpdateDisplay(TargetData);

            SubmitButton.OnMouseDown.AddListener(HandleSubmitClicked);
            ReturnBtn.OnMouseDown.AddListener(HandleReturnClicked);

            ResultsGroup.SetActive(false);
        }

        private void OnDestroy()
        {
            Game.Events?.Deregister(GameEvents.WaferStateUpdated, HandleWaferStateUpdated);
            Game.Events?.Deregister(GameEvents.WaferStateUndone, HandleWaferStateUpdated);
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

            ResultsGroup.SetActive(false);
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
            Game.Scenes.LoadMainScene(SceneReference.FromName("ChipFabLoader"));
        }

        #endregion // Handlers
    
        public void Evaluate()
        {
            var currState = DragMgr.WaferInstance.Data;
            bool success = WaferData.IsEqual(currState, TargetData);

            float secondsPerCycle = 30;

            ResultsGroup.SetActive(true);
            SuccessGroup.SetActive(success);
            FailureGroup.SetActive(!success);
            PrecisionText.gameObject.SetActive(true);
            PrecisionText.SetText("Accuracy: " + (currState.Precision.Avg() * 100).ToString("#.##") + "%");
            TimeText.SetText("Time: " + TimeMgr.Instance.RunningText.text);
            CycleText.SetText("Total Production Time: " + Mathf.Ceil(TimeMgr.Instance.GetElapsedTime() / secondsPerCycle));

            Game.Events.Dispatch(GameEvents.WaferSubmitted);
        }
    }
}