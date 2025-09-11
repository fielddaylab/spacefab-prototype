using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum SputteringMicrogameState
    {
        Activated,
        Ready,
        Sputtering,
        Finished,
        Deactivated
    }

    public class SputteringMicrogame : StationMicrogame, IStationMicrogame
    {
        private static KeyCode FIRE_KEY = KeyCode.Space;

        public Blaster Blaster;

        public ClickBox FinishButton;

        public SideWaferDisplay WaferDisplay;

        private SputteringMicrogameState m_state;

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

            m_state = SputteringMicrogameState.Deactivated;

            FinishButton.OnMouseDown.RemoveListener(HandleFinishClicked);
        }

        #endregion // IStationMicrogame

        private void Update()
        {
            if (!Container.activeInHierarchy) { return; }

            switch (m_state)
            {
                case SputteringMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case SputteringMicrogameState.Ready:
                    TransitionToSputtering();
                    break;
                case SputteringMicrogameState.Sputtering:
                    ProcessMicrogame();
                    break;
                case SputteringMicrogameState.Finished:
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
            m_state = SputteringMicrogameState.Activated;
            TransitionCommon();
        }

        private void TransitionToReady()
        {
            m_state = SputteringMicrogameState.Ready;
            TransitionCommon();
        }

        private void TransitionToSputtering()
        {
            m_state = SputteringMicrogameState.Sputtering;
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