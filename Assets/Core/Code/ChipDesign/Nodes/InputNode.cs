using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class InputNode : NodeBase
    {
        [Header("Input")]
        public float InputVal; // for Input Nodes
        public TMP_Text LabelText;

        private void Awake()
        {
            DefaultVal = InputVal;
        }

        public override void UpdateNodeText() { }

        public override float Evaluate(NodeBase prevNode, out bool unstable)
        {
            unstable = false;
            return InputVal;
        }
    }
}