using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum GameMode
    {
        Cozy,
        Timed
    }

    public class ModeMgr : MonoBehaviour
    {
        public static ModeMgr Instance;

        public GameMode Mode;

        private void Awake()
        {
            Instance = this;
        }
    }
}