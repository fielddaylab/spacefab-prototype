using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class OutputNode : NodeBase
    {
        [Header("Output")]
        public float OutputTarget; // for Output Nodes

        public override float Evaluate(out bool unstable)
        {
            return EvaluationMgr.EvaluateNode(this, out unstable);
        }
    }
}