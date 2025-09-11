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

        private EtchMicrogameState m_state;

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
                case EtchMicrogameState.Activated:
                    // TransitionToReady();
                    break;
                case EtchMicrogameState.Ready:
                    // TransitionToSpinning();
                    break;
                case EtchMicrogameState.Etching:
                    // ProcessMicrogame();
                    break;
                case EtchMicrogameState.Finished:
                    // Deactivate();
                    break;
                default:
                    break;
            }
        }

        private void TransitionToActivated()
        {
            m_state = EtchMicrogameState.Activated;
            TransitionCommon();
        }

        private void TransitionCommon()
        {

        }
    }
}