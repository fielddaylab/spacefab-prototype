using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class ChipDesignConfig : MonoBehaviour
    {
        public static ChipDesignConfig Instance;

        public LevelData ConfigLevel;

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