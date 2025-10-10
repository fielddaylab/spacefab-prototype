using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class NavNodesMgr : MonoBehaviour
    {
        public static NavNodesMgr Instance;

        public List<ControlNavNode> Nodes;

        private void Start()
        {
            Instance = this;
        }
    }
}