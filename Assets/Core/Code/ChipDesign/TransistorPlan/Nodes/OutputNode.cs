using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class OutputNode : NodeBase
    {
        [Header("Output")]
        public float OutputTarget; // for Output Nodes

        private void Awake()
        {
            DefaultVal = GameConsts.DEFFERED_CODE;

            Game.Events.Register(GameEvents.OnConfigChanged, HandleConfigChanged);
        }

        public override void UpdateNodeText() { }

        public override float Evaluate(NodeBase prevNode, out bool unstable)
        {
            return EvaluationMgr.EvaluateNode(this, prevNode, null, DefaultVal, out unstable);
        }

        private void HandleConfigChanged()
        {
            OutputTarget = InteractionMgr.Instance.CurrLevelData.GetOutVal();
        }
    }
}