using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class Station : MonoBehaviour
    {
        public StationId Id;

        private void Awake()
        {
            StationMgr.Instance.RegisterStation(this);
        }
    }
}