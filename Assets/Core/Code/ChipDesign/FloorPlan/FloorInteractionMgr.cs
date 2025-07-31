using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        [HideInInspector] public Link CurrLink = null;

        private List<NodeBase> AllNodes = new List<NodeBase>();
        private List<Link> AllLinks = new List<Link>();

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

        public List<NodeBase> GetAllNodes()
        {
            return AllNodes;
        }

        public List<Link> GetAllLinks()
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
                if (CurrLink != null)
                {
                    // clear last link
                    DeleteLink(CurrLink);
                }

                var mousePos = SnapToGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                CurrLink = Instantiate(LinkPrefab).GetComponent<Link>();
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
                        var mouseSnapped = SnapToGrid(mousePos);
                        if (mouseSnapped.x != CurrLink.transform.position.x || mouseSnapped.y != CurrLink.transform.position.y)
                        {
                            // terminate link
                            var mousePos2D = new Vector3(mouseSnapped.x, mouseSnapped.y, CurrLink.transform.position.z);
                            CurrLink.LineRenderer.SetPosition(1, mousePos2D);

                            // rotate box collider
                            Vector3 relativePos = mousePos2D - CurrLink.transform.position;
                            float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
                            CurrLink.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                            FinalizeLink(CurrLink, null, new Vector3(mouseSnapped.x, mouseSnapped.y, CurrLink.transform.position.z));
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
