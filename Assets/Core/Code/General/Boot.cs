using System.Collections;
using System.Collections.Generic;
using BeauUtil;
using FieldDay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceFab
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private SceneReference m_FirstScene;

        private void Awake() {
            Game.Scenes.LoadMainScene(m_FirstScene);
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}