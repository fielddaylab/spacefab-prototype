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
        public Button StartBtn;
        [SerializeField] private string m_ChipDesignScene;

        public LevelData Level0Data;

        private void Awake()
        {
            StartBtn.onClick.AddListener(HandleStartClicked);
        }

        private void HandleStartClicked()
        {
            ChipDesignConfig.Instance.ConfigLevel = Level0Data;
            SceneManager.LoadScene(m_ChipDesignScene);
        }
    }
}