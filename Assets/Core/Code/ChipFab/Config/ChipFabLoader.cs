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

        public Button NoAutoBtn;
        public Button AutoBtn;

        public Button StartBtn;
        [SerializeField] private string m_ChipFabScene;

        [SerializeField] private LevelSetupData NoAutoData;
        [SerializeField] private LevelSetupData AutoData;

        private void Awake()
        {
            CozyModeBtn.onClick.AddListener(HandleCozyClicked);
            TimedModeBtn.onClick.AddListener(HandleTimeClicked);

            NoAutoBtn.onClick.AddListener(HandleNoAutoClicked);
            AutoBtn.onClick.AddListener(HandleAutoClicked);

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

        private void HandleNoAutoClicked()
        {
            ChipFabConfig.Instance.CurrLevel = NoAutoData;
        }

        private void HandleAutoClicked()
        {
            ChipFabConfig.Instance.CurrLevel = AutoData;
        }
    }
}