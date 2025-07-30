using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class PNode : NodeBase
    {
        [HideInInspector] public int JunctionCount = 1;
        public NodeBase Dependency; // In an PNP, the N. In an PN, the N.
        public NodeBase ConnectedSource; // In an PNP, the other P. In an PN, the N.
        public List<NodeBase> WingNodes = new List<NodeBase>(); // In an NPN, the N's.

        [SerializeField] private TMP_Text JunctionText;
        // public List<NodeBase> ConnectedLikeNodes; // In PP, the other P.

        private void Awake()
        {
            DefaultVal = GameConsts.DEFFERED_CODE;
            JunctionCount = 1;
        }

        public override void UpdateNodeText()
        {
            JunctionText.SetText(JunctionCount.ToString());
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
                        return EvaluationMgr.EvaluateNode(this, prevNode, null, DefaultVal, out unstable);
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
                    // find links val and dependency val
                    var linksVal = Links.Count == 0 ? DefaultVal : EvaluationMgr.EvaluateNode(this, prevNode, Dependency, DefaultVal, out unstable);
                    var dependencyVal = EvaluationMgr.EvaluateNode(Dependency, null, null, GameConsts.DEFFERED_CODE, out unstable);

                    // If dependency evaluates to <0 (and src val agrees with links), use source value. Else use default value from links.
                    if (!unstable)
                    {
                        if (dependencyVal < 0 && dependencyVal != GameConsts.DEFFERED_CODE)
                        {
                            var srcVal = EvaluationMgr.EvaluateNode(ConnectedSource, null, null, GameConsts.DEFFERED_CODE, out unstable);

                            // ensure links agree with srcVal
                            if (linksVal != srcVal && (linksVal != GameConsts.DEFFERED_CODE && srcVal != GameConsts.DEFFERED_CODE))
                            {
                                unstable = true;
                                return GameConsts.UNSTABLE_CODE;
                            }

                            if (!unstable)
                            { 
                                return srcVal;
                            }
                        }
                        else
                        {
                            return linksVal;
                        }
                    }
                }
            }


            return 0;
        }
    }
}