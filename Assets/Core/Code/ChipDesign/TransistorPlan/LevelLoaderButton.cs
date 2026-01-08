using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public class LevelLoaderButton : MonoBehaviour
    {
        public Button Button;
        public LevelData LevelData;
        public TMP_Text LevelText;

        private void Start()
        {
            LevelText.SetText(LevelData.GetTitle());
        }
    }
}