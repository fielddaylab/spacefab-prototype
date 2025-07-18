using FieldDay;
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

    public abstract class NodeBase : MonoBehaviour
    {
        public NodeType NodeType;
        [NonSerialized] public List<Link> Links = new List<Link>();
        [HideInInspector] public bool Visited;

        private void Awake()
        {
            Game.Events.Register(GameEvents.EvaluationStarted, HandleEvaluationStarted);
        }

        private void OnDestroy()
        {
            Game.Events?.Deregister(GameEvents.EvaluationStarted, HandleEvaluationStarted);
        }

        public abstract float Evaluate(NodeBase prevNode, out bool unstable);

        private void HandleEvaluationStarted()
        {
            Visited = false;
        }

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
