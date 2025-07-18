using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class InputNode : NodeBase
    {
        [Header("Input")]
        public float InputVal; // for Input Nodes

        private void Awake()
        {
            DefaultVal = InputVal;
        }

        public override float Evaluate(NodeBase prevNode, out bool unstable)
        {
            unstable = false;
            return InputVal;
        }
    }
}