using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class FloorShape : MonoBehaviour
    {
        public Collider2D Collider;

        private void Start()
        {
            FloorEvaluationMgr.Instance.AllShapes.Add(this);
        }
    }
}