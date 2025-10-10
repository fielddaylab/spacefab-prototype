using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class DopantMgr : MonoBehaviour
    {
        public static DopantMgr Instance;
        public Dispenser NDispenser;
        public Dispenser PDispenser;

        private void Start()
        {
            Instance = this;
        }
    }
}