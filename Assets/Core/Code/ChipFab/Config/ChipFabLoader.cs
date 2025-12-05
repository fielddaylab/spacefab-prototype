using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFab.ChipFab
{
    public class ChipFabLoader : MonoBehaviour
    {
        private void Awake()
        {
            ChipFabConfig.Instance.Mode = GameMode.Timed;
        }
    }
}