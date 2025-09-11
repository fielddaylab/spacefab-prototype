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
            var currState = DragMgr.WaferInstance.Data;

            // TODO: make more dynamic
            if (TargetData.ResistLayer.State != currState.ResistLayer.State)
            {
                success = false;
            }

            if ((TargetData.MetallizationLayer.State != currState.MetallizationLayer.State)
                || (TargetData.MetallizationLayer.Mask.Id != currState.MetallizationLayer.Mask.Id)
                || (TargetData.MetallizationLayer.Mask.Rotation != currState.MetallizationLayer.Mask.Rotation)
                )
            {
                success = false;
            }

            if (TargetData.OxideLayer.State != currState.OxideLayer.State)
            {
                success = false;
            }

            bool hasPatterns = true;

            foreach (var pattern in TargetData.SemiconductorLayer.DopingPatterns)
            {
                bool anyFound = false;
                foreach (var currPattern in currState.SemiconductorLayer.DopingPatterns)
                {
                    if ((currPattern.Mask.Id == pattern.Mask.Id)
                        && (currPattern.Mask.Rotation == pattern.Mask.Rotation)
                        && (currPattern.DopingType == pattern.DopingType)
                        )
                    {
                        anyFound = true;
                    }
                }

                if (!anyFound) {
                    hasPatterns = false;
                    break;
                }
            }

            if (!hasPatterns)
            {
                success = false;
            }

            SuccessGroup.SetActive(success);
            FailureGroup.SetActive(!success);
        }
    }
}