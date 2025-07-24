using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class PNode : NodeBase
    {
        public NodeBase Dependency; // In an PNP, the N. In an PN, the N.
        public NodeBase ConnectedSource; // In an PNP, the other P. In an PN, the N.
        // public List<NodeBase> ConnectedLikeNodes; // In PP, the other P.

        private void Awake()
        {
            DefaultVal = GameConsts.DEFFERED_CODE;
        }

        public override float Evaluate(NodeBase prevNode, out bool unstable)
        {
            unstable = false;

            if (Dependency == null)
            {
                // Has no dependency

                if (ConnectedSource == null)
                {
                    // Use direct value from links. If no links, default to deferred.
                    if (Links.Count == 0) {
                        return DefaultVal;
                    }
                    else {
                        return EvaluationMgr.EvaluateNode(this, prevNode, DefaultVal, out unstable);
                    }
                }
                else
                {
                    // Use direct value from connected source (same type of node)
                }
            }
            else
            {
                // Has a dependency

                if (ConnectedSource == null)
                {
                    // Should not occur!
                    Debug.LogWarning("[PNode] A P Node has a dependancy but no source!");
                }
                else
                {
                    // If dependency evaluates to <0, use source value. Else use default value from links. If no links present, default value of 0.
                }
            }


            return 0;
        }
    }
}