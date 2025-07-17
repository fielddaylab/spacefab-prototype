using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class Link : MonoBehaviour
    {
        public Node SideA;
        public Node SideB;

        public Transform StartAnchor;
        public Transform EndAnchor;

        public BoxCollider2D Collider;

        public LineRenderer LineRenderer;
    }
}
