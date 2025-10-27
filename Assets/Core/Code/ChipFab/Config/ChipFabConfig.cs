using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class ChipFabConfig : MonoBehaviour
    {
        public static ChipFabConfig Instance;

        public GameMode Mode;
        public LevelSetupData CurrLevel;

        private void Awake()
        {
           if (Instance == null)
           {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
           }
           else
           {
                Destroy(this.gameObject);
           }
        }
    }
}