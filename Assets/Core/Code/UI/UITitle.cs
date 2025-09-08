using System.Collections;
using System.Collections.Generic;
using BeauUtil;
using FieldDay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab
{
    public class UITitle : MonoBehaviour
    {
        [SerializeField] private Button m_ChipDesignButton;
        [SerializeField] private SceneReference m_ChipDesignScene;

        [SerializeField] private Button m_FloorPlanButton;
        [SerializeField] private SceneReference m_FloorPlanScene;

        [SerializeField] private Button m_SupplyChainButton;
        [SerializeField] private SceneReference m_SupplyChainScene;

        private void Awake()
        {
            m_ChipDesignButton.onClick.AddListener(OnStartClicked);
            m_FloorPlanButton.onClick.AddListener(OnFloorPlanClicked);
            m_SupplyChainButton.onClick.AddListener(OnSupplyChainClicked);
        }


        #region Handlers

        private void OnStartClicked()
        {
            Game.Scenes.LoadMainScene(m_ChipDesignScene);
        }

        private void OnFloorPlanClicked()
        {
            Game.Scenes.LoadMainScene(m_FloorPlanScene);
        }

        private void OnSupplyChainClicked() {
            Game.Scenes.LoadMainScene(m_SupplyChainScene);
        }

        #endregion // Handlers
    }
}