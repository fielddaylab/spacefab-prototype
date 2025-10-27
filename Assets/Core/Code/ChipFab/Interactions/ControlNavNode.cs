using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class ControlNavNode : MonoBehaviour
    {
        public Hoverable Hoverable;

        public ControlNavNode LeftNode;
        public ControlNavNode RightNode;
        public ControlNavNode UpNode;
        public ControlNavNode DownNode;
        public ControlNavNode EnterNestNode;
        public ControlNavNode ExitNestNode;

        public NavInteractable Interactable;
    }
}