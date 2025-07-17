using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public enum GridInteractionLayer
    {
        Nodes,
        Links
    }

    public class InteractionMgr : MonoBehaviour
    {
        public static InteractionMgr Instance;

        public GridInteractionLayer ActiveLayer { get; private set; }

        [SerializeField] private GameObject LinkPrefab;
        [HideInInspector] public Link CurrLink = null;

        #region Unity Callbacks

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
        }

        private void Update()
        {
            ProcessInteractions();
        }

        #endregion // Unity Callbacks

        #region Interactions

        private void ProcessInteractions()
        {
            switch (ToolbarMgr.Instance.ActiveTool)
            {
                case ToolType.None:
                    break;
                case ToolType.DrawLinks:
                    ProcessDrawLinks();
                    break;
                case ToolType.Erase:
                    if (ActiveLayer == GridInteractionLayer.Nodes) { ProcessEraseNodes(); }
                    else { ProcessEraseLinks(); }
                    break;
                default:
                    break;
            }
        }

        private void ProcessDrawLinks()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (CurrLink != null)
                {
                    // clear last link
                    DeleteLink(CurrLink);
                }

                // check if valid start
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    Debug.Log("valid start");
                    var startNode = hit.GetComponent<Node>();
                    if (startNode)
                    {
                        CurrLink = Instantiate(LinkPrefab).GetComponent<Link>();
                        CurrLink.transform.position = new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z);
                        CurrLink.SideA = startNode;
                    }
                }
                else
                {
                    Debug.Log("invalid start");
                }
            }

            if (Input.GetMouseButton(0))
            { 
                if (CurrLink != null)
                {
                    // update endpoint
                    var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var offset = CurrLink.transform.InverseTransformPoint(mousePos);
                    CurrLink.LineRenderer.SetPosition(1, new Vector3(offset.x, offset.y, 0));
                }
            }
             
            if (Input.GetMouseButtonUp(0))
            {
                if (CurrLink != null)
                {
                    var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Nodes"));
                    if (hit != null)
                    {
                        var endNode = hit.GetComponent<Node>();
                        if (endNode)
                        {
                            FinalizeLink(CurrLink, endNode);
                        }
                    }
                    else
                    {
                        DeleteLink(CurrLink);
                    }
                }
            }
        }

        private void ProcessEraseNodes()
        {

        }

        private void ProcessEraseLinks()
        {

        }

        #endregion // Interactions

        #region Helpers

        private void DeleteLink(Link link)
        {
            link.SideA?.RemoveLink(link);
            link.SideB?.RemoveLink(link);
            if (CurrLink == link) { CurrLink = null; }
            Destroy(link.gameObject);
        }

        private void FinalizeLink(Link link, Node end)
        {
            link.SideB = end;
            CurrLink = null;
        }

        #endregion // Helpers

        #region Layers

        public void SetActiveLayer(GridInteractionLayer layer)
        {
            ActiveLayer = layer;
            Game.Events.Dispatch(GameEvents.OnLayerChanged);
        }

        #endregion // Layers
    }
}
