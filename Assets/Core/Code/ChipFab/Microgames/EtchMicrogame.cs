using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum EtchMicrogameState
    {
        Activated,
        Ready,
        Etching,
        Finished,
        Deactivated
    }

    public class EtchMicrogame : StationMicrogame, IStationMicrogame
    {
        private static KeyCode FIRE_KEY = KeyCode.Space;

        public Blaster Blaster;

        public ClickBox FinishButton;

        public SideWaferDisplay WaferDisplay;

        private EtchMicrogameState m_state;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            FinishButton.transform.parent.gameObject.SetActive(false);
            FinishButton.OnMouseDown.AddListener(HandleFinishClicked);

            DragMgr.Instance.DragWaferEnabled = false;

            WaferDisplay.UpdateDisplay(DragMgr.WaferInstance.Data);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            m_state = EtchMicrogameState.Deactivated;

            FinishButton.OnMouseDown.RemoveListener(HandleFinishClicked);
        }

        #endregion // IStationMicrogame


        private void Update()
        {
            if (!Container.activeInHierarchy) { return; }
            
            switch (m_state)
            {
                case EtchMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case EtchMicrogameState.Ready:
                    TransitionToEtching();
                    break;
                case EtchMicrogameState.Etching:
                    ProcessMicrogame();
                    break;
                case EtchMicrogameState.Finished:
                    break;
                default:
                    break;
            }
        }

        private void ProcessMicrogame()
        {
            if (Input.GetKey(FIRE_KEY))
            {
                Blaster.Blast();
            }
        }

        private void TransitionToActivated()
        {
            m_state = EtchMicrogameState.Activated;
            TransitionCommon();
        }

        private void TransitionToReady()
        {
            m_state = EtchMicrogameState.Ready;
            TransitionCommon();
        }

        private void TransitionToEtching()
        {
            m_state = EtchMicrogameState.Etching;
            FinishButton.transform.parent.gameObject.SetActive(true);
            TransitionCommon();
        }

        private void TransitionCommon()
        {

        }

        private void HandleFinishClicked()
        {
            DragMgr.Instance.DragWaferEnabled = true;
            Deactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }
    }
}