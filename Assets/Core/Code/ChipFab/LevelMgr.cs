using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public WaferData TargetData;

        private void Start()
        {
            Game.Events.Register(GameEvents.WaferStateUpdated, HandleWaferStateUpdated);

            TargetSide.UpdateDisplay(TargetData);
            // TargetAngled.UpdateDisplay(TargetData);

            SubmitButton.OnMouseDown.AddListener(HandleSubmitClicked);
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
        }

        private void HandleSubmitClicked()
        {
            Evaluate();
        }

        #endregion // Handlers
    
        private void Evaluate()
        {
            bool success = true;

            SuccessGroup.SetActive(success);
            FailureGroup.SetActive(!success);
        }
    }
}