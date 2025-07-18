using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class NNode : NodeBase
    {
        public NodeBase Dependency; // In an NPN, the P. In an NP, the P.
        public NodeBase ConnectedSource; // In an NPN, the other N. In an NP, the P.

        public override float Evaluate(NodeBase prevNode, out bool unstable)
        {
            unstable = false;

            if (Dependency == null)
            {
                // Has no dependency

                if (ConnectedSource == null)
                {
                    // Use direct value from links. If no links, default to 0.
                    if (Links.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return EvaluationMgr.EvaluateNode(this, prevNode, 0, out unstable);
                    }
                }
                else
                {
                    // Use direct value from connected source
                }
            }
            else
            {
                // Has a dependency

                if (ConnectedSource == null)
                {
                    // Should not occur!
                    Debug.LogWarning("[NNode] An N Node has a dependancy but no source!");
                }
                else
                {
                    // If dependency evaluates to >0, use source value. Else use default value from links. If no links present, default value of 0.
                }
            }


            return 0;
        }
    }
}