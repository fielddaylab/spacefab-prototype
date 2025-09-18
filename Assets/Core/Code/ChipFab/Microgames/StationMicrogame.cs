using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public abstract class StationMicrogame : MonoBehaviour, IStationMicrogame
    {
        public GameObject Container;

        protected void Start()
        {
            if (Container)
            {
                Container.SetActive(false);
            }
        }

        public virtual void Activate(WaferState waferState)
        {
            if (Container)
            {
                Container.SetActive(true);
            }
        }

        public virtual void Deactivate()
        {
            if (Container)
            {
                Container.SetActive(false);
            }
        }

        public abstract bool TryCancel();
    }
}
