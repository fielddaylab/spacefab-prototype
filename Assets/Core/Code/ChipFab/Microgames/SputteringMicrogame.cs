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
        private SputteringMicrogameState m_state;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        #endregion // IStationMicrogame

        private void Update()
        {
            switch (m_state)
            {
                case SputteringMicrogameState.Activated:
                    // TransitionToReady();
                    break;
                case SputteringMicrogameState.Ready:
                    // TransitionToSpinning();
                    break;
                case SputteringMicrogameState.Sputtering:
                    // ProcessMicrogame();
                    break;
                case SputteringMicrogameState.Finished:
                    // Deactivate();
                    break;
                default:
                    break;
            }
        }

        private void TransitionToActivated()
        {
            m_state = SputteringMicrogameState.Activated;
            TransitionCommon();
        }

        private void TransitionCommon()
        {

        }
    }
}