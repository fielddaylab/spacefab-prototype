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

        private void Awake()
        {
            m_ChipDesignButton.onClick.AddListener(OnStartClicked);
        }


        #region Handlers

        private void OnStartClicked()
        {
            SceneManager.LoadScene(m_ChipDesignScene);
        }

        #endregion // Handlers
    }
}