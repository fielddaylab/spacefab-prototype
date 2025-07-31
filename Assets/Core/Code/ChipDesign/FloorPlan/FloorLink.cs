using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class FloorLink : MonoBehaviour
    {
        public FloorNode SideA;
        public FloorNode SideB;

        public Transform StartAnchor;
        public Transform EndAnchor;

        public FloorNode StartAnchorNode;
        public FloorNode EndAnchorNode;

        public BoxCollider2D Collider;

        public LineRenderer LineRenderer;

        public LinkType LinkType;
    }
}
