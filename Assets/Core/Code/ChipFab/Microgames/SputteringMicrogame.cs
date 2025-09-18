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

        public LayerMask SputterLayer;

        public Transform LeftBoundPos;
        public Transform RightBoundPos;

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

        public override bool TryCancel()
        {
            return m_state == SputteringMicrogameState.Deactivated;
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

            // disallow oxide state
            // PREREQS Oxide EMPTY
            if (DragMgr.WaferInstance.Data.OxideLayer.State != OxideState.Empty)
            {
                DragMgr.Instance.DragWaferEnabled = true;
                Deactivate();
            }
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

        private float EvaluatePrecision()
        {
            // suite of raycasts
            int numSections = 100;
            int hitCount = 0;
            float xStep = (RightBoundPos.position.x - LeftBoundPos.position.x) / numSections;

            for (int i = 0; i < numSections; i++)
            {
                // raycast at step
                float x = LeftBoundPos.position.x + xStep * i;
                Vector2 pos = new Vector2(x, LeftBoundPos.position.y);
                var collider = Physics2D.OverlapPoint(pos, SputterLayer);
                if (collider)
                {
                    hitCount++;
                }
            }

            float precision = hitCount / (float)numSections;
            return precision;
        }

        private void HandleFinishClicked()
        {
            var precision = EvaluatePrecision();
            DragMgr.Instance.DragWaferEnabled = true;
            DragMgr.WaferInstance.SetMetallizationState(precision);
            Deactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }
    }
}