using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SpaceFab.ChipDesign
{
    public class FloorInteractionMgr : MonoBehaviour
    {
        public static FloorInteractionMgr Instance;

        public GridInteractionLayer ActiveLayer { get; private set; }
        public ToolType ActiveTool = ToolType.None;
        public LinkType ActiveLinkType = LinkType.Grey;

        [SerializeField] private GameObject LinkPrefab;
        [HideInInspector] public FloorLink CurrLink = null;

        private List<FloorNode> AllNodes = new List<FloorNode>(); // all nodes except floor link nodes
        private List<FloorLink> AllLinks = new List<FloorLink>();

        #region Unity Callbacks

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
        }

        public void AddNode(FloorNode toAdd)
        {
            AllNodes.Add(toAdd);
        }

        private void FixedUpdate()
        {
            ProcessInteractions();
        }

        #endregion // Unity Callbacks

        public List<FloorNode> GetAllNodes()
        {
            return AllNodes;
        }

        public List<FloorLink> GetAllLinks()
        {
            return AllLinks;
        }

        #region Interactions

        private void ProcessInteractions()
        {
            switch (ActiveTool)
            {
                case ToolType.None:
                    break;
                case ToolType.DrawLinks:
                    ProcessDrawLinks();
                    break;
                case ToolType.Erase:
                    ProcessEraseLinks();
                    break;
                default:
                    break;
            }
        }

        private void ProcessDrawLinks()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                BeginNewLink();
            }

            if (Input.GetMouseButton(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (CurrLink != null)
                    {
                        // update endpoint
                        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        var mouseSnapped = SnapToGrid(mousePos);
                        float offsetAmt = 0.25f;
                        if (mouseSnapped.x != CurrLink.transform.position.x || mouseSnapped.y != CurrLink.transform.position.y)
                        {
                            // get dir
                            var dirMousSnapped = mouseSnapped;
                            var dirOffset = Vector3.zero;
                            if (mouseSnapped.x > CurrLink.transform.position.x)
                            {
                                // snapped to the right
                                dirOffset.x = -offsetAmt;
                            }
                            else if (mouseSnapped.x < CurrLink.transform.position.x)
                            {
                                // snapped to the left
                                dirOffset.x = offsetAmt;
                            }
                            else if (mouseSnapped.y > CurrLink.transform.position.y)
                            {
                                // snapped to the up
                                dirOffset.y = -offsetAmt;
                            }
                            else if (mouseSnapped.y < CurrLink.transform.position.y)
                            {
                                // snapped to the down
                                dirOffset.y = offsetAmt;
                            }

                            // terminate link
                            var mousePos2D = new Vector3(mouseSnapped.x, mouseSnapped.y, CurrLink.transform.position.z);
                            CurrLink.LineRenderer.SetPosition(1, mousePos2D);

                            // rotate box collider
                            Vector3 relativePos = mousePos2D - CurrLink.transform.position;
                            float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
                            CurrLink.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                            var endPos = new Vector3(mouseSnapped.x, mouseSnapped.y, CurrLink.transform.position.z);
                            CurrLink.EndAnchor.position = endPos;

                            // move box collider to new position
                            Physics2D.SyncTransforms();

                            // connect to existing nodes and links
                            var startNodeHits = Physics2D.OverlapPointAll(CurrLink.transform.position - dirOffset, 1 << LayerMask.NameToLayer("FloorNodes"));
                            var startLinkHits = Physics2D.OverlapPointAll(CurrLink.transform.position, 1 << LayerMask.NameToLayer("FloorLinkNodes"));

                            var endNodeHits = Physics2D.OverlapPointAll(mouseSnapped + dirOffset, 1 << LayerMask.NameToLayer("FloorNodes"));
                            var endLinkHits = Physics2D.OverlapPointAll(mouseSnapped, 1 << LayerMask.NameToLayer("FloorLinkNodes"));
                            FinalizeLink(CurrLink, startNodeHits, startLinkHits, endNodeHits, endLinkHits);

                            if ((endNodeHits.Length == 0) && (endLinkHits.Length <= 1))
                            {
                                BeginNewLink();
                            }
                        }
                        else
                        {
                            var mousePos2D = new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z);
                            CurrLink.LineRenderer.SetPosition(1, mousePos2D);
                        }
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
                        DeleteLink(CurrLink);
                    }
                }
                else if (CurrLink != null)
                {
                    DeleteLink(CurrLink);
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
                    var toErase = hit.GetComponent<FloorLink>();
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

        private void BeginNewLink()
        {
            if (CurrLink != null)
            {
                // clear last link
                DeleteLink(CurrLink);
            }

            var mousePos = SnapToGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            CurrLink = Instantiate(LinkPrefab).GetComponent<FloorLink>();
            CurrLink.transform.position = new Vector3(mousePos.x, mousePos.y, CurrLink.transform.position.z);
            CurrLink.LineRenderer.SetPosition(0, CurrLink.transform.position);
            CurrLink.LineRenderer.SetPosition(1, CurrLink.transform.position);
            CurrLink.LineRenderer.startColor = ActiveLinkType == LinkType.Grey ? Color.grey : Color.yellow;
            CurrLink.LineRenderer.endColor = ActiveLinkType == LinkType.Grey ? Color.grey : Color.yellow;
            CurrLink.LinkType = ActiveLinkType;
            CurrLink.SideA = null;

            var hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Nodes"));
            if (hit != null)
            {
                var startNode = hit.GetComponent<FloorNode>();
                if (startNode)
                {
                    CurrLink.SideA = startNode;
                }
            }
        }

        private void DeleteLink(FloorLink link)
        {
            link.SideA?.RemoveLink(link);
            link.SideB?.RemoveLink(link);
            if (CurrLink == link) { CurrLink = null; }

            if (AllLinks.Contains(link)) { AllLinks.Remove(link); }
            Destroy(link.gameObject);

            Game.Events.Dispatch(GameEvents.OnLayoutChanged);
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            var gridSize = 1;
            float x = Mathf.Round(position.x / gridSize) * gridSize;
            float y = Mathf.Round(position.y / gridSize) * gridSize;
            return new Vector3(x, y, position.z);
        }

        private void FinalizeLink(FloorLink link, Collider2D[] startNodes, Collider2D[] startLinks, Collider2D[] endNodes, Collider2D[] endLinks)
        {
            FloorNode currNode;
            bool anyStartNodeFound = false;
            bool anyStartLinkFound = false;
            foreach (var startCollider in startNodes)
            {
                currNode = startCollider.GetComponent<FloorNode>();
                if (currNode)
                {
                    link.SideA = currNode;
                    link.SideA?.Links.Add(link);

                    anyStartNodeFound = true;
                }
            }
            if (!anyStartNodeFound)
            {
                foreach (var startCollider in startLinks)
                {
                    currNode = startCollider.GetComponent<FloorNode>();
                    if (currNode && currNode != link.StartAnchorNode && currNode != link.EndAnchorNode)
                    {
                        link.SideA = currNode;
                        link.SideA?.Links.Add(link);

                        // TODO: add back to the newly found link node
                        anyStartLinkFound = true;
                        anyStartNodeFound = true;
                    }
                }

                if (!anyStartLinkFound)
                {
                    link.SideA = link.StartAnchorNode;
                    link.SideA?.Links.Add(link);
                }
            }

            bool anyEndNodeFound = false;
            bool anyEndLinkFound = false;
            foreach (var endCollider in endNodes)
            {
                currNode = endCollider.GetComponent<FloorNode>();
                if (currNode)
                {
                    link.SideB = currNode;
                    link.SideB?.Links.Add(link);

                    anyEndNodeFound = true;
                }
            }
            if (!anyEndNodeFound)
            {
                link.SideB = link.EndAnchorNode;

                foreach (var endCollider in endLinks)
                {
                    currNode = endCollider.GetComponent<FloorNode>();
                    if (currNode && currNode != link.StartAnchorNode && currNode != link.EndAnchorNode)
                    {
                        link.SideB = currNode;
                        link.SideB?.Links.Add(link);

                        // TODO: add back to the newly found link node

                        anyEndLinkFound = true;
                        anyEndNodeFound = true;
                    }
                }

                if (!anyEndLinkFound)
                {
                    link.SideB = link.EndAnchorNode;
                    link.SideB?.Links.Add(link);
                }
            }

            CurrLink = null;

            // adjust collider
            link.Collider.size = new Vector2(Vector3.Distance(link.StartAnchor.position, link.EndAnchor.position), link.LineRenderer.startWidth);
            var offset = link.Collider.offset;
            offset.x = link.Collider.size.x / 2;
            link.Collider.offset = offset;

            if (!AllLinks.Contains(link))
            {
                AllLinks.Add(link);
            }

            Game.Events.Dispatch(GameEvents.OnLayoutChanged);
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
