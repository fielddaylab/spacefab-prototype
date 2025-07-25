using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class VisualOverlayMgr : MonoBehaviour
    {
        [SerializeField] private GameObject ArrowPrefab;

        private List<GameObject> ArrowVisuals = new List<GameObject>();

        private void Awake()
        {
            Game.Events.Register(GameEvents.OnLayoutChanged, HandleLayoutChanged);
        }

        private void HandleLayoutChanged()
        {
            RefreshArrowVisuals();
            RefreshLinkVisuals();
        }

        private void RefreshArrowVisuals()
        {
            // clear old visuals
            foreach (var arrow in ArrowVisuals)
            {
                Destroy(arrow.gameObject);
            }

            ArrowVisuals.Clear();

            // generate new visuals

            var allNodes = InteractionMgr.Instance.GetAllNodes();
            foreach (var node in allNodes)
            {
                if (node.NodeType == NodeType.P)
                {
                    var nodeP = node.GetComponent<PNode>();

                    /*
                     * For all P nodes
                     *  If there is an adjacent N Node, and both this node and adj node have JunctionCount > 1 and in same junction:
	                 *      Spawn an Arrow pointing away from P
                     */
                    var dirVector = Vector3.zero;
                    var magnitude = 0.5f;
                    var rotation = 0;
                    for (int dir = 0; dir < 4; dir++)
                    {
                        switch (dir)
                        {
                            case 0:
                                // up
                                dirVector = new Vector3(0, 1, 0);
                                rotation = 90;
                                break;
                            case 1:
                                // right
                                dirVector = new Vector3(1, 0, 0);
                                rotation = 0;
                                break;
                            case 2:
                                // down
                                dirVector = new Vector3(0, -1, 0);
                                rotation = -90;
                                break;
                            case 3:
                                // left
                                dirVector = new Vector3(-1, 0, 0);
                                rotation = 180;
                                break;
                            default:
                                break;
                        }

                        var hit = Physics2D.OverlapPoint(node.transform.position + dirVector, 1 << LayerMask.NameToLayer("Nodes"));
                        if (hit != null)
                        {
                            var hitNode = hit.GetComponent<NNode>();

                            if (hitNode == null || hitNode.JunctionCount <= 1 || hitNode.JunctionCount != nodeP.JunctionCount) { continue; }
                            if (nodeP.JunctionCount == 3 && !nodeP.WingNodes.Contains(hitNode)) { continue; }

                            // Spawn an Arrow pointing away from P
                            var arrow = Instantiate(ArrowPrefab);
                            arrow.transform.position = nodeP.transform.position + dirVector * magnitude;
                            arrow.transform.Rotate(Vector3.forward, rotation);

                            ArrowVisuals.Add(arrow);
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        private void RefreshLinkVisuals()
        {
            var allLinks = InteractionMgr.Instance.GetAllLinks();

            foreach (var link in allLinks)
            {
                if (link.SideA && link.SideB)
                {
                    link.LineRenderer.startColor = Color.gray;
                    link.LineRenderer.endColor = Color.gray;
                }
                else
                {
                    link.LineRenderer.startColor = Color.red;
                    link.LineRenderer.endColor = Color.red;
                }
            }
        }
    }
}
