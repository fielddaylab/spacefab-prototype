using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab.ChipFab
{
    public class ChipFabLoader : MonoBehaviour
    {
        public Button CozyModeBtn;
        public Button TimedModeBtn;

        public Button StartBtn;
        [SerializeField] private string m_ChipFabScene;


        private void Awake()
        {
            CozyModeBtn.onClick.AddListener(HandleCozyClicked);
            TimedModeBtn.onClick.AddListener(HandleTimeClicked);
            StartBtn.onClick.AddListener(HandleStartClicked);
        }

        private void HandleStartClicked()
        {
            SceneManager.LoadScene(m_ChipFabScene);
        }

        private void HandleCozyClicked()
        {
            ChipFabConfig.Instance.Mode = GameMode.Cozy;
        }

        private void HandleTimeClicked()
        {
            ChipFabConfig.Instance.Mode = GameMode.Timed;
        }
    }
}