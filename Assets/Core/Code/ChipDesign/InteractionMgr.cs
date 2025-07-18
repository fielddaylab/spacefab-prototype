using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public ToolType ActiveTool = ToolType.None;

        [SerializeField] private GameObject LinkPrefab;
        [HideInInspector] public Link CurrLink = null;

        [SerializeField] private GameObject NPrefab;
        [SerializeField] private GameObject PPrefab;

        #region Unity Callbacks

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
        }

        private void Update()
        {
            ProcessInteractions();
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("[Mouse] " + mousePos);
        }

        #endregion // Unity Callbacks

        #region Interactions

        private void ProcessInteractions()
        {
            switch (ActiveTool)
            {
                case ToolType.None:
                    break;
                case ToolType.DrawNNodes:
                    ProcessDrawNNodes();
                    break;
                case ToolType.DrawPNodes:
                    ProcessDrawPNodes();
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

        private void ProcessDrawNNodes()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                // check if valid start
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    Debug.Log("invalid start");
                }
                else
                {
                    Debug.Log("valid start");
                    var newNNode = Instantiate(NPrefab).GetComponent<NNode>();
                    newNNode.transform.position = new Vector3(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f), newNNode.transform.position.z);
                }
            }
        }

        private void ProcessDrawPNodes()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                // check if valid start
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    Debug.Log("invalid start");
                }
                else
                {
                    Debug.Log("valid start");
                    var newPNode = Instantiate(PPrefab).GetComponent<PNode>();
                    newPNode.transform.position = new Vector3(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f), newPNode.transform.position.z);
                }
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
                    var startNode = hit.GetComponent<NodeBase>();
                    if (startNode)
                    {
                        CurrLink = Instantiate(LinkPrefab).GetComponent<Link>();
                        CurrLink.transform.position = new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z);
                        CurrLink.SideA = startNode;
                        CurrLink.LineRenderer.SetPosition(0, CurrLink.transform.position);
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
                    var mousePos2D = new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z);
                    CurrLink.LineRenderer.SetPosition(1, mousePos2D);

                    // rotate box collider
                    Vector3 relativePos = mousePos2D - CurrLink.transform.position;
                    float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
                    CurrLink.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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
                        var endNode = hit.GetComponent<NodeBase>();
                        if (endNode)
                        {
                            FinalizeLink(CurrLink, endNode, new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z));
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
            if (Input.GetMouseButtonDown(0))
            {
                // check if valid start
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Links"));
                if (hit != null)
                {
                    Debug.Log("valid link to erase");
                    var toErase = hit.GetComponent<Link>();
                    DeleteLink(toErase);
                }
                else
                {
                    Debug.Log("invalid link to erase");
                }
            }
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

        private void FinalizeLink(Link link, NodeBase end, Vector3 endPos)
        {
            link.SideB = end;
            CurrLink = null;
            link.EndAnchor.position = endPos;

            link.SideA.Links.Add(link);
            link.SideB.Links.Add(link);

            // adjust collider
            link.Collider.size = new Vector2(Vector3.Distance(link.StartAnchor.position, link.EndAnchor.position), link.LineRenderer.startWidth);
            var offset = link.Collider.offset;
            offset.x = link.Collider.size.x / 2;
            link.Collider.offset = offset;
        }

        #endregion // Helpers

        #region Tools

        public void SetActiveTool(ToolType tool)
        {
            ActiveTool = tool;
            Game.Events.Dispatch(GameEvents.OnToolChanged);
        }

        #endregion // Tools

        #region Layers

        public void SetActiveLayer(GridInteractionLayer layer)
        {
            ActiveLayer = layer;
            Game.Events.Dispatch(GameEvents.OnLayerChanged);
        }

        #endregion // Layers
    }
}
