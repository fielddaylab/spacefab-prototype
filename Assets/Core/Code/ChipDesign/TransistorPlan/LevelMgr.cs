using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class LevelMgr : MonoBehaviour
    {
        public static LevelMgr Instance;

        [SerializeField] private LevelData CURR_LEVEL_DATA;
        public LevelDataCopy CurrLevelData { get; private set; }

        private void Awake()
        {
            Instance = this;

            CurrLevelData = new LevelDataCopy();
            CurrLevelData.LoadData(CURR_LEVEL_DATA);
        }
    }
}