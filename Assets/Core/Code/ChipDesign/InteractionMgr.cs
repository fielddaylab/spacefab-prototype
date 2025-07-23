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
                    var flooredMousePos = new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f));
                    newNNode.transform.position = new Vector3(flooredMousePos.x, flooredMousePos.y, newNNode.transform.position.z);

                    // Try to attach to Link
                    AddNodeToExistingLink(flooredMousePos, newNNode);
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
                    var flooredMousePos = new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f));
                    newPNode.transform.position = new Vector3(flooredMousePos.x, flooredMousePos.y, newPNode.transform.position.z);

                    // Try to attach to Link
                    AddNodeToExistingLink(flooredMousePos, newPNode);
                }
            }
        }

        private void ProcessDrawLinks()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) 
            {
                if (CurrLink != null)
                {
                    // clear last link
                    DeleteLink(CurrLink);
                }

                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                CurrLink = Instantiate(LinkPrefab).GetComponent<Link>();
                CurrLink.transform.position = new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z);
                CurrLink.LineRenderer.SetPosition(0, CurrLink.transform.position);
                CurrLink.SideA = null;

                var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    var startNode = hit.GetComponent<NodeBase>();
                    if (startNode)
                    {
                        CurrLink.SideA = startNode;
                    }
                }
            }

            if (Input.GetMouseButton(0))
            { 
                if (!EventSystem.current.IsPointerOverGameObject())
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
                else if (CurrLink != null)
                {
                    DeleteLink(CurrLink);
                }
            }
             
            if (Input.GetMouseButtonUp(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
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
                            FinalizeLink(CurrLink, null, new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z));
                        }
                    }
                }
                else if (CurrLink != null)
                {
                    DeleteLink(CurrLink);
                }
            }
        }

        private void ProcessEraseNodes()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // check if valid start
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    Debug.Log("valid node to erase");
                    var toErase = hit.GetComponent<NodeBase>();
                    DeleteNode(toErase);
                }
                else
                {
                    Debug.Log("invalid node to erase");
                }
            }
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

        private void DeleteNode(NodeBase node)
        {
            // TODO: handle special nodes

            // clear from links
            for (int i = 0; i < node.Links.Count; i++)
            {
                if (node.Links[i].SideA == node)
                {
                    node.Links[i].SideA = null;

                    /*
                    if (node.Links[i].SideB != null)
                    {
                        RemoveNodeFromConnectedLink(node, node.Links[i].SideB, i);
                    }
                    */
                }
                else
                {
                    node.Links[i].SideB = null;

                    /*
                    if (node.Links[i].SideA != null)
                    {
                        RemoveNodeFromConnectedLink(node, node.Links[i].SideA, i);
                    }
                    */
                }
            }

            // delete Node
            Destroy(node.gameObject);
        }

        private void RemoveNodeFromConnectedLink(NodeBase origNode, NodeBase connectedNode, int i)
        {
            // find link from other node and remove it
            for (int j = 0; j < connectedNode.Links.Count; j++)
            {
                if (connectedNode.Links[j].SideB == origNode)
                {
                    connectedNode.Links[j].SideB = null;
                }
                else if (connectedNode.Links[j].SideA == origNode)
                {
                    connectedNode.Links[j].SideA = null;
                }
            }
        }

        private void AddNodeToExistingLink(Vector3 mousePos, NodeBase newNode)
        {
            var colliderExtents = newNode.GetComponent<Collider2D>().bounds.extents;
            var size = new Vector2(colliderExtents.x * newNode.transform.lossyScale.x, colliderExtents.y * newNode.transform.lossyScale.x);
            var linkHits = Physics2D.OverlapBoxAll(mousePos, size, 0, 1 << LayerMask.NameToLayer("Links"));
            
            if (linkHits.Length > 0)
            {
                for (int i = 0; i < linkHits.Length; i++)
                {
                    var linkHit = linkHits[i];
                    Link link = linkHit.GetComponent<Link>();

                    if (link.SideA == null)
                    {
                        Debug.Log("[InteractionMgr] Attaching to existing link");
                        link.SideA = newNode;
                        newNode.Links.Add(link);
                    }
                    else if (link.SideB == null)
                    {
                        Debug.Log("[InteractionMgr] Attaching to existing link");
                        link.SideB = newNode;
                        newNode.Links.Add(link);
                    }
                }
            }
        }

        private void FinalizeLink(Link link, NodeBase end, Vector3 endPos)
        {
            link.SideB = end;
            CurrLink = null;
            link.EndAnchor.position = endPos;

            link.SideA?.Links.Add(link);
            link.SideB?.Links.Add(link);

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
