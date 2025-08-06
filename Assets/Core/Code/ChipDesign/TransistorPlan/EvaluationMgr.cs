using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public class EvaluationMgr : MonoBehaviour
    {
        [SerializeField] private Button EvaluateButton;

        [SerializeField] private GameObject ResultGroup;
        [SerializeField] private TMP_Text EvaluateText;
        [SerializeField] private TMP_Text ExpectedResultText;
        [SerializeField] private TMP_Text ActualResultText;

        [SerializeField] private GameObject EvaluatePanel;
        [SerializeField] private TMP_Text EvaluatePanelTitleText;
        [SerializeField] private Button ReviseButton;
        [SerializeField] private Button ContinueButton;
        [SerializeField] private TMP_Text UsedSpaceText;
        [SerializeField] private TMP_Text SpaceCostText;
        [SerializeField] private TMP_Text NodeCostText;
        [SerializeField] private TMP_Text LinkCostText;
        [SerializeField] private TMP_Text ValueText;
        [SerializeField] private TMP_Text ProfitText;

        [Header("Costs")]
        public float CostPerLinkUnit;
        public float CostPerNode;
        public float CostPerSpace;
        public float ProjectValue;

        private OutputNode m_OutNode;

        private void Awake()
        {
            EvaluateButton.onClick.AddListener(HandleEvaluateClicked);
            ResultGroup.SetActive(false);
            EvaluateText.SetText(string.Empty);

            // Find output node
            var outNode = GameObject.Find("Out");
            if (outNode != null)
            {
                m_OutNode = outNode.GetComponent<OutputNode>();
                ExpectedResultText.SetText(m_OutNode.OutputTarget.ToString());
            }

            EvaluatePanel.SetActive(false);
            ReviseButton.onClick.AddListener(HandleReviseClicked);
            ContinueButton.onClick.AddListener(HandleContinueClicked);
        }

        private void OnDestroy()
        {
            EvaluateButton?.onClick.RemoveListener(HandleEvaluateClicked);
        }

        #region Evaluation

        private void Evaluate()
        {
            // Find output node
            var outNode = GameObject.Find("Out");
            if (outNode == null)
            {
                Debug.LogError("[EvaluationMgr] No output node named \"Out\" found in level!");
            }
            else
            {
                m_OutNode = outNode.GetComponent<OutputNode>();
                ExpectedResultText.SetText(m_OutNode.OutputTarget.ToString());

                // Start at Output node and work backward
                float actual = m_OutNode.Evaluate(null, out bool unstable);
                if (actual == GameConsts.DEFFERED_CODE) { actual = 0; }

                bool result = (actual == m_OutNode.OutputTarget) && !unstable;
                UpdateEvaluationText(result, actual, unstable);
            }
        }

        /// <summary>
        /// Semi-recursive function to evaluate the charge at the given node
        /// </summary>
        /// <param name="currNode"></param>
        /// <returns></returns>
        public static float EvaluateNode(NodeBase currNode, NodeBase prevNode, NodeBase dependency, float defaultVal, out bool unstable)
        {
            unstable = false;
            currNode.Visited = true;

            // Gather all links at current node
            List<Link> links = currNode.Links;
            float checkVal = defaultVal;
            float otherVal = defaultVal;
            NodeBase otherNode = null;
            bool firstValid = false;
            for (int i = 0; i < links.Count; i++)
            {
                // Get other side
                if (links[i].SideA == currNode) { otherNode = links[i].SideB; }
                else { otherNode = links[i].SideA; }

                // do not handle empty points
                if (otherNode == null) { continue; }

                // do not go back to parent node
                if (otherNode == prevNode) { continue; }

                // do not check links through dependency
                if (otherNode == dependency) { continue; }

                // if node is already visited, mark unstable
                if (!otherNode.Visited) {
                    // Evaluate. Ensure they all have the same value.
                    otherVal = otherNode.Evaluate(currNode, out unstable);
                }
                else
                {
                    otherVal = otherNode.VisitedVal;
                }

                if (!firstValid && otherVal != GameConsts.DEFFERED_CODE)
                {
                    checkVal = otherVal;
                    firstValid = true;
                }
                else if (checkVal != otherVal && otherVal != GameConsts.DEFFERED_CODE)
                {
                    // unstable
                    unstable = true;
                    return GameConsts.UNSTABLE_CODE;
                }

            }

            currNode.VisitedVal = checkVal;
            return checkVal;
        }

        private void UpdateEvaluationText(bool success, float actual, bool unstable)
        {
            ResultGroup.SetActive(true);
            if (unstable) { ActualResultText.SetText("Unstable"); }
            else { ActualResultText.SetText(actual.ToString()); }

            if (success) { EvaluateText.SetText("Correct!"); }
            else { EvaluateText.SetText("Incorrect"); }

            bool profitable = UpdateCostBreakdown();

            EvaluatePanel.SetActive(true);
            if (profitable && success) { EvaluatePanelTitleText.SetText("Design Validated!"); }
            else { EvaluatePanelTitleText.SetText("Back to the\nDrawing Board..."); }
            ContinueButton.interactable = success && profitable;
        }

        private bool UpdateCostBreakdown()
        {
            // space cost
            int numUnits = CalcNumSpaceUnits();
            float spaceCost = CostPerSpace * numUnits;

            // transistor cost
            int numTransistorNodes = 0;
            var allNodes = InteractionMgr.Instance.GetAllNodes();
            foreach (var node in allNodes)
            {
                if (node.NodeType == NodeType.N || node.NodeType == NodeType.P)
                {
                    numTransistorNodes++;
                }
            }
            float transistorCost = numTransistorNodes * CostPerNode;

            // link cost
            float linkCost = 0;
            var allLinks = InteractionMgr.Instance.GetAllLinks();
            foreach (var links in allLinks)
            {
                var dist = Vector3.Distance(links.EndAnchor.transform.position, links.StartAnchor.transform.position);
                linkCost += dist * CostPerLinkUnit;
            }

            float profit = ProjectValue - linkCost - transistorCost - spaceCost;

            // set text
            UsedSpaceText.SetText(numUnits + " nm^2");
            SpaceCostText.SetText("$" + spaceCost.ToString("0.00"));
            SpaceCostText.color = Color.red;
            NodeCostText.SetText("$" + transistorCost.ToString("0.00"));
            NodeCostText.color = Color.red;
            LinkCostText.SetText("$" + linkCost.ToString("0.00"));
            LinkCostText.color = Color.red;
            ValueText.SetText("$" + ProjectValue.ToString("0.00"));
            ValueText.color = Color.green;
            ProfitText.SetText("$" + profit.ToString("0.00"));
            ProfitText.color = profit > 0 ? Color.green : Color.red;

            return profit > 0;
        }

        private int CalcNumSpaceUnits()
        {
            bool anyNodes = false;

            int yBounds = 6;
            int xBounds = 8;

            int topY = yBounds;
            bool hitAny = false;

            var yBox = new Vector2(xBounds * 2, 0.5f);
            var xBox = new Vector2(0.5f, yBounds * 2);

            while (!hitAny && topY >= -yBounds)
            {
                Vector2 currPos = new Vector2(0, topY);
                Collider2D hit = Physics2D.OverlapBox(currPos, yBox, 0, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    hitAny = true;
                    anyNodes = true;
                    break;
                }
                
                topY--;
            }

            hitAny = false;
            int bottomY = -yBounds;
            while (!hitAny && bottomY <= topY)
            {
                Vector2 currPos = new Vector2(0, bottomY);
                Collider2D hit = Physics2D.OverlapBox(currPos, yBox, 0, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    hitAny = true;
                    anyNodes = true;
                    break;
                }

                bottomY++;
            }

            int leftX = -xBounds;
            hitAny = false;
            while (!hitAny && leftX <= xBounds)
            {
                Vector2 currPos = new Vector2(leftX, 0);
                Collider2D hit = Physics2D.OverlapBox(currPos, xBox, 0, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    hitAny = true;
                    anyNodes = true;
                    break;
                }

                leftX++;
            }

            int rightX = xBounds;
            hitAny = false;
            while (!hitAny && rightX >= leftX)
            {
                Vector2 currPos = new Vector2(rightX, 0);
                Collider2D hit = Physics2D.OverlapBox(currPos, xBox, 0, 1 << LayerMask.NameToLayer("Nodes"));
                if (hit != null)
                {
                    hitAny = true;
                    anyNodes = true;
                    break;
                }

                rightX--;
            }

            int height = topY - bottomY;
            int width = rightX - leftX;
            if (anyNodes)
            {
                height++;
                width++;
            }

            return width * height;
        }

        #endregion // Evaluation

        #region Handlers

        private void HandleEvaluateClicked()
        {
            Game.Events.Dispatch(GameEvents.EvaluationStarted);
            Evaluate();
        }

        private void HandleReviseClicked()
        {
            EvaluatePanel.SetActive(false);
        }

        private void HandleContinueClicked()
        {
            // EvaluatePanel.SetActive(false);
        }

        #endregion // Handlers
    }
}