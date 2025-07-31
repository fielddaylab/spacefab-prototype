using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab
{
    public class UITitle : MonoBehaviour
    {
        [SerializeField] private Button m_ChipDesignButton;
        [SerializeField] private string m_ChipDesignScene;

        [SerializeField] private Button m_FloorPlanButton;
        [SerializeField] private string m_FloorPlanScene;

        private void Awake()
        {
            m_ChipDesignButton.onClick.AddListener(OnStartClicked);
            m_FloorPlanButton.onClick.AddListener(OnFloorPlanClicked);
        }


        #region Handlers

        private void OnStartClicked()
        {
            SceneManager.LoadScene(m_ChipDesignScene);
        }

        private void OnFloorPlanClicked()
        {
            SceneManager.LoadScene(m_FloorPlanScene);
        }

        #endregion // Handlers
    }
}