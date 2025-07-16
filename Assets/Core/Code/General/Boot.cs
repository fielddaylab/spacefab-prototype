using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceFab
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private string m_FirstScene;

        private void Start()
        {
            DontDestroyOnLoad(this);
            SceneManager.LoadScene(m_FirstScene);
        }
    }
}