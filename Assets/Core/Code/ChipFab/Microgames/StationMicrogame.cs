using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum StationId
    {
        Furnace,
        Photolithograph,
        Resist,
        Sputter,
        Etch,
        Wash
    }

    public abstract class StationMicrogame : MonoBehaviour, IStationMicrogame
    {
        public GameObject Container;
        public CamPositioner CamPos;

        protected void Start()
        {
            if (Container)
            {
                Container.SetActive(false);
            }
        }

        public virtual void Activate(WaferState waferState)
        {
            CamMgr.Instance.LoadCamPos(CamPos.Pos);

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
