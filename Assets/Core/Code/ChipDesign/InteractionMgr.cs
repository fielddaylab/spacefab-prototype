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

        private List<NodeBase> AllNodes = new List<NodeBase>();

        #region Unity Callbacks

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
        }

        private void FixedUpdate()
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

                    // move box collider to new position
                    Physics2D.SyncTransforms();

                    AllNodes.Add(newNNode);

                    // Try to attach to Link
                    AddNodeToExistingLink(flooredMousePos, newNNode);

                    // Combine nearby nodes
                    CombineNodePass();
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

                    // move box collider to new position
                    Physics2D.SyncTransforms();

                    AllNodes.Add(newPNode);

                    // Try to attach to Link
                    AddNodeToExistingLink(flooredMousePos, newPNode);

                    // Combine nearby nodes
                    CombineNodePass();
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
            // handle special nodes
            if (node.NodeType == NodeType.N)
            {
                var nodeN = node.GetComponent<NNode>();
                if (nodeN.JunctionCount == 2)
                {
                    // always connected with opposite node (P)
                    PNode pDependencySrc = nodeN.ConnectedSource.GetComponent<PNode>();
                    if (pDependencySrc.ConnectedSource == nodeN) { pDependencySrc.ConnectedSource = null; }
                    if (pDependencySrc.Dependency == nodeN) { pDependencySrc.Dependency = null; }

                    nodeN.Dependency = null;
                    nodeN.ConnectedSource = null;

                    nodeN.JunctionCount = pDependencySrc.JunctionCount = 1;
                }
                else if (nodeN.JunctionCount == 3)
                {
                    // TODO: 
                }
            }
            else if (node.NodeType == NodeType.P)
            {
                var nodeP = node.GetComponent<PNode>();
                if (nodeP.JunctionCount == 2)
                {
                    // always connected with opposite node (N)
                    NNode nDependencySrc = nodeP.ConnectedSource.GetComponent<NNode>();
                    if (nDependencySrc.ConnectedSource == nodeP) { nDependencySrc.ConnectedSource = null; }
                    if (nDependencySrc.Dependency == nodeP) { nDependencySrc.Dependency = null; }

                    nodeP.Dependency = null;
                    nodeP.ConnectedSource = null;

                    nodeP.JunctionCount = nDependencySrc.JunctionCount = 1;
                }
                else if (nodeP.JunctionCount == 3)
                {
                    // TODO: 
                }
            }

            // clear from links
            for (int i = 0; i < node.Links.Count; i++)
            {
                if (node.Links[i].SideA == node)
                {
                    node.Links[i].SideA = null;
                }
                else
                {
                    node.Links[i].SideB = null;
                }
            }

            // remove from list
            AllNodes.Remove(node);

            // delete Node
            Destroy(node.gameObject);
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

        private void CombineNodePass()
        {
            List<NodeBase> checkList = new List<NodeBase>();
            foreach(var node in AllNodes)
            {
                checkList.Add(node);
            }

            bool anyFound = false;

            while (checkList.Count > 0)
            {
                var node = checkList[0];

                // check surroundings for an adjacent node
                var dirVector = Vector3.zero;
                for (int dir = 0; dir < 4; dir++)
                {
                    switch (dir) {
                        case 0:
                            // up
                            dirVector = new Vector3(0, 1, 0);
                            break;
                        case 1:
                            // right
                            dirVector = new Vector3(1, 0, 0);
                            break;
                        case 2:
                            // down
                            dirVector = new Vector3(0, -1, 0);
                            break;
                        case 3:
                            // left
                            dirVector = new Vector3(-1, 0, 0);
                            break;
                        default:
                            break;
                    }

                    var hit = Physics2D.OverlapPoint(node.transform.position + dirVector, 1 << LayerMask.NameToLayer("Nodes"));
                    if (hit != null)
                    {
                        var hitNode = hit.GetComponent<NodeBase>();
                        bool alreadyHandled = true;

                        if (hitNode.NodeType == NodeType.Input || hitNode.NodeType == NodeType.Output)
                        {
                            continue;
                        }

                        if (node.NodeType == NodeType.N)
                        {
                            alreadyHandled = !TryMergeNNode(node.GetComponent<NNode>(), hitNode);
                        }
                        else if (node.NodeType == NodeType.P)
                        {
                            alreadyHandled = !TryMergePNode(node.GetComponent<PNode>(), hitNode);
                        }

                        if (!alreadyHandled)
                        {
                            checkList.Remove(node);
                            checkList.Add(node);
                            if (checkList.Contains(hitNode))
                            {
                                checkList.Remove(hitNode);
                                checkList.Add(hitNode);
                            }
                            else
                            {
                                checkList.Add(hitNode);
                            }

                            anyFound = true;
                        }
                    }

                    if (anyFound)
                    {
                        break;
                    }
                }

                if (anyFound)
                {
                    anyFound = false;
                    continue;
                }
                else
                {
                    checkList.Remove(node);
                }
            }
        }

        private bool TryMergePNode(PNode primaryNode, NodeBase secondaryNode)
        {
            if (secondaryNode.NodeType == NodeType.P)
            {
                // TODO: same node type
                
                Debug.Log("[InteractionMgr] Found new adj node");
            }
            else if (secondaryNode.NodeType == NodeType.N)
            {
                if (secondaryNode == primaryNode.ConnectedSource || secondaryNode == primaryNode.Dependency)
                {
                    // already connected
                    return false;
                }

                Debug.Log("[InteractionMgr] Found new adj node");

                var secondaryNodeN = secondaryNode.GetComponent<NNode>();

                // different node type
                // if 1 node
                if (primaryNode.JunctionCount == 1)
                {
                    // form a 2-node junction
                    // push all involved nodes back for second pass
                    // flag another pass necessary

                    if (secondaryNodeN.JunctionCount == 1)
                    {
                        // connecting to 1 node = 2 total
                        primaryNode.Dependency = secondaryNodeN;
                        primaryNode.ConnectedSource = secondaryNodeN;

                        secondaryNodeN.Dependency = primaryNode;
                        secondaryNodeN.ConnectedSource = primaryNode;

                        primaryNode.JunctionCount = secondaryNodeN.JunctionCount = 2;

                        return true;
                    }
                    else if (secondaryNodeN.JunctionCount == 2)
                    {
                        // TODO: connecting to 2 nodes = 3 total

                    }
                    else if (secondaryNodeN.JunctionCount >= 3)
                    {
                        // connecting to 3+ nodes -- not supported!

                    }
                }
                // if 2 nodes
                else if (primaryNode.JunctionCount == 2)
                {
                    // form a 3-node junction

                    if (secondaryNodeN.JunctionCount == 1)
                    {
                        // TODO: connecting to 1 node = 3 total

                    }
                    else if (secondaryNodeN.JunctionCount >= 2)
                    {
                        // connecting to 2+ nodes = 4+ total -- not supported!

                    }
                }
                // if 3 nodes
                else
                {
                    // additional mergings not supported!
                }
            }

            return false;
        }

        private bool TryMergeNNode(NNode primaryNode, NodeBase secondaryNode)
        {
            if (secondaryNode.NodeType == NodeType.N)
            {
                // TODO: same node type

                Debug.Log("[InteractionMgr] Found new adj node");

            }
            else if (secondaryNode.NodeType == NodeType.P)
            {
                if (secondaryNode == primaryNode.ConnectedSource || secondaryNode == primaryNode.Dependency)
                {
                    // already connected
                    return false;
                }

                Debug.Log("[InteractionMgr] Found new adj node");

                var secondaryNodeP = secondaryNode.GetComponent<PNode>();

                // different node type
                // if 1 node
                if (primaryNode.JunctionCount == 1)
                {
                    // form a 2-node junction
                    // push all involved nodes back for second pass
                    // flag another pass necessary

                    if (secondaryNodeP.JunctionCount == 1)
                    {
                        // connecting to 1 node = 2 total
                        primaryNode.Dependency = secondaryNodeP;
                        primaryNode.ConnectedSource = secondaryNodeP;

                        secondaryNodeP.Dependency = primaryNode;
                        secondaryNodeP.ConnectedSource = primaryNode;

                        primaryNode.JunctionCount = secondaryNodeP.JunctionCount = 2;

                        return true;
                    }
                    else if (secondaryNodeP.JunctionCount == 2)
                    {
                        // TODO: connecting to 2 nodes = 3 total

                    }
                    else if (secondaryNodeP.JunctionCount >= 3)
                    {
                        // connecting to 3+ nodes -- not supported!

                    }
                }
                // if 2 nodes
                else if (primaryNode.JunctionCount == 2)
                {
                    // form a 3-node junction

                    if (secondaryNodeP.JunctionCount == 1)
                    {
                        // TODO: connecting to 1 node = 3 total

                    }
                    else if (secondaryNodeP.JunctionCount >= 2)
                    {
                        // connecting to 2+ nodes = 4+ total -- not supported!

                    }
                }
                // if 3 nodes
                else
                {
                    // additional mergings not supported!
                }
            }

            return false;
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
