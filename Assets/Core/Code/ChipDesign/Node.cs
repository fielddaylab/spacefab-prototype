using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public enum NodeType
    {
        Input,
        Output,
        N,
        P
    }

    public class Node : MonoBehaviour
    {
        public NodeType NodeType;
        [NonSerialized] public List<Link> Links = new List<Link>();

        [Header("Input")]
        public float InputVal; // for Input Nodes

        [Header("Output")]
        public float OutputTarget; // for Output Nodes

        public void RemoveLink(Link link)
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
