using BeauUtil;
using FieldDay;
using FieldDay.Scenes;
using SpaceFab.ChipDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab.ChipFab
{
    public class LevelLoadButton : MonoBehaviour
    {
        public LevelSetupData Data;
        public Button Button;

        private void Awake()
        {
            Button.onClick.AddListener(HandleButtonClicked);
        }

        private void HandleButtonClicked()
        {
            ChipFabConfig.Instance.CurrLevel = Data;
            Game.Scenes.LoadMainScene(SceneReference.FromName(ChipFabConfig.Instance.ChipFabScene));
        }
    }
}