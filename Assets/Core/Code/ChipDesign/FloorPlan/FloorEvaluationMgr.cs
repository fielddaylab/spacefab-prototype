using BeauUtil;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public class FloorEvaluationMgr : MonoBehaviour
    {
        [SerializeField] private Button EvaluateButton;

        [SerializeField] private GameObject ResultGroup;
        [SerializeField] private TMP_Text EvaluateText;
        [SerializeField] private TMP_Text ExpectedResultText;
        [SerializeField] private TMP_Text ActualResultText;

        private void Awake()
        {
            EvaluateButton.onClick.AddListener(HandleEvaluateClicked);
            ResultGroup.SetActive(false);
            EvaluateText.SetText(string.Empty);
        }

        private void OnDestroy()
        {
            EvaluateButton?.onClick.RemoveListener(HandleEvaluateClicked);
        }

        #region Evaluation

        private void Evaluate()
        {
            bool result = true;
            int successfulChecks = 0;

            // for each input, check if connected to target output ID
            foreach (var node in FloorInteractionMgr.Instance.GetAllNodes())
            {
                if (node.NodeType == NodeType.Input)
                {
                    if (!IsInputConnectedToOutput(node, null, node.RequiredID, node.RequiredLinkType, 0))
                    {
                        result = false;
                        // break;
                    }
                    else
                    {
                        successfulChecks++;
                    }
                }
            }

            Debug.Log("Evaluation: " + result + " with " + successfulChecks + " successful connections");
            UpdateEvaluationText(result);
        }

        private bool IsInputConnectedToOutput(FloorNode currNode, FloorNode prevNode, string requiredID, LinkType requiredType, int iter)
        {
            bool anyFound = false;
            // check all connected links (exclude prev node linkage)
            foreach (var link in currNode.Links)
            {
                var nextNode = link.SideA;
                if (link.SideA == currNode)
                {
                    // recurse on side b
                    nextNode = link.SideB;
                }
                else if (link.SideB == currNode)
                {
                    // recurse on side a
                    nextNode = link.SideA;
                }

                if (nextNode == prevNode) { continue; }

                if (nextNode.NodeType == NodeType.Output)
                {
                    if (nextNode.TerminusID == requiredID)
                    {
                        if (iter == 0)
                        {
                            // at least 1 space between inputs/outputs
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else { 
                        // either flat out wrong or unstable
                        return false; 
                    }
                }
                else
                {
                    if (link.LinkType == requiredType)
                    {
                        // recurse
                        if (IsInputConnectedToOutput(nextNode, currNode, requiredID, requiredType, iter + 1))
                        {
                            anyFound = true;
                        }
                    }
                    else {
                        // conflicting materials
                        return false;
                    }
                }
            }

            return anyFound;
        }

        private void UpdateEvaluationText(bool success)
        {
            ResultGroup.SetActive(true);

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