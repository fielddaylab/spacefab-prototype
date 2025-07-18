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

        private OutputNode m_OutNode;

        private void Awake()
        {
            EvaluateButton.onClick.AddListener(HandleEvaluateClicked);
            ResultGroup.SetActive(false);
            EvaluateText.SetText(string.Empty);

            // Find output node
            var outNode = GameObject.Find("Out");
            if (outNode == null) {
                Debug.LogError("[EvaluationMgr] No output node named \"Out\" found in level!");
            }
            else {
                m_OutNode = outNode.GetComponent<OutputNode>();
                ExpectedResultText.SetText(m_OutNode.OutputTarget.ToString());
            }
        }

        private void OnDestroy()
        {
            EvaluateButton?.onClick.RemoveListener(HandleEvaluateClicked);
        }

        #region Evaluation

        private void Evaluate()
        {
            // Start at Output node and work backward
            float actual = m_OutNode.Evaluate(null, out bool unstable);

            bool result = (actual == m_OutNode.OutputTarget) && !unstable;
            UpdateEvaluationText(result, actual, unstable);
        }

        /// <summary>
        /// Semi-recursive function to evaluate the charge at the given node
        /// </summary>
        /// <param name="currNode"></param>
        /// <returns></returns>
        public static float EvaluateNode(NodeBase currNode, NodeBase prevNode, float defaultVal, out bool unstable)
        {
            unstable = false;
            currNode.Visited = true;

            // Gather all links at current node
            List<Link> links = currNode.Links;
            float checkVal = defaultVal;
            float otherVal = defaultVal;
            NodeBase otherNode = null;
            bool firstValid = false;
            for (int i = 0; i <  links.Count; i++)
            {
                // Get other side
                if (links[i].SideA == currNode) { otherNode = links[i].SideB; }
                else { otherNode = links[i].SideA; }

                // do not go back to parent node
                if (otherNode == prevNode) { continue; }

                // if node is already visited, mark unstable
                /*
                if (otherNode.Visited) {
                    unstable = true;
                    return -29;
                }
                */

                // Evaluate. Ensure they all have the same value.
                otherVal = otherNode.Evaluate(currNode, out unstable);
                if (!firstValid)
                {
                    checkVal = otherVal;
                    firstValid = true;
                }
                else if (checkVal != otherVal)
                {
                    // unstable
                    unstable = true;
                    return -29;
                }

            }

            return checkVal;
        }

        private void UpdateEvaluationText(bool success, float actual, bool unstable)
        {
            ResultGroup.SetActive(true);
            if (unstable) { ActualResultText.SetText("Unstable"); }
            else { ActualResultText.SetText(actual.ToString()); }

            if (success) { EvaluateText.SetText("Correct!"); }
            else { EvaluateText.SetText("Incorrect"); }
        }

        #endregion // Evaluation

        #region Handlers

        private void HandleEvaluateClicked()
        {
            Game.Events.Dispatch(GameEvents.EvaluationStarted);
            Evaluate();
        }

        #endregion // Handlers
    }
}