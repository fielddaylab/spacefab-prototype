using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab {
    public interface IStationMicrogame
    {
        public void Activate(WaferState waferState);

        public void Deactivate();
    }
}