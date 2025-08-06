using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class InputNode : NodeBase
    {
        [Header("Input")]
        public float InputVal; // for Input Nodes
        public TMP_Text LabelText;

        private void Awake()
        {
            DefaultVal = InputVal;

            Game.Events.Register(GameEvents.OnConfigChanged, HandleConfigChanged);

        }

        public override void UpdateNodeText() { }

        public override float Evaluate(NodeBase prevNode, out bool unstable)
        {
            unstable = false;
            return InputVal;
        }

        private void HandleConfigChanged()
        {
            if (this.LabelText.text.Equals("A"))
            {
                InputVal = DefaultVal = InteractionMgr.Instance.CurrLevelData.GetAVal();
            }
            else if (this.LabelText.text.Equals("B"))
            {
                InputVal = DefaultVal = InteractionMgr.Instance.CurrLevelData.GetBVal();
            }
            else if (this.LabelText.text.ToUpper().Equals("IN"))
            {
                InputVal = DefaultVal = InteractionMgr.Instance.CurrLevelData.GetInVal();
            }
        }
    }
}