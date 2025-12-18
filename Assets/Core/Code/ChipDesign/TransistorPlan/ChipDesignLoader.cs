using FieldDay;
using FieldDay.Scenes;
using SpaceFab.ChipFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public class ChipDesignLoader : MonoBehaviour
    {
        [SerializeField] private LevelLoaderButton[] LevelButtons;
        [SerializeField] private string m_ChipDesignScene;

        private void Awake()
        {
            for (int i = 0; i < LevelButtons.Length; i++)
            {
                int indexCopy = i;
                LevelButtons[i].Button.onClick.AddListener(() => { HandleLevelClicked(indexCopy); });
            }
        }

        private void HandleLevelClicked(int index)
        {
            ChipDesignConfig.Instance.ConfigLevel = LevelButtons[index].LevelData;
            Game.Scenes.LoadMainScene(SceneUtils.GetSceneByName(m_ChipDesignScene));
        }
    }
}