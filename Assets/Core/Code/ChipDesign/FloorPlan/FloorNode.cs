using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class FloorNode : MonoBehaviour
    {
        [NonSerialized] public List<FloorLink> Links = new List<FloorLink>();

        public bool IsLink = false;
        public NodeType NodeType; // input or output
        public string TerminusID;
        public string RequiredID; // target other side to connect

        private void Awake()
        {
            FloorInteractionMgr.Instance.AddNode(this);
        }

        public void RemoveLink(FloorLink link)
        {
            for (int i = 0; i < Links.Count; i++)
            {
                if (Links[i] == link)
                {
                    Links.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
