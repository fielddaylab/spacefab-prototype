using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class InputNode : NodeBase
    {
        [Header("Input")]
        public float InputVal; // for Input Nodes

        public override float Evaluate(out bool unstable)
        {
            unstable = false;
            return InputVal;
        }
    }
}