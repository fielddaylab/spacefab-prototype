using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class Link : MonoBehaviour
    {
        public NodeBase SideA;
        public NodeBase SideB;

        public Transform StartAnchor;
        public Transform EndAnchor;

        public BoxCollider2D Collider;

        public LineRenderer LineRenderer;
    }
}
