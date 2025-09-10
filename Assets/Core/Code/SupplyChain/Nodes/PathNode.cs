using BeauUtil;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class PathNode : BatchedComponent {
        public PathNodeFlags Flags;
        
        [Required] public Collider2D Collider;
        public PathNodeHighlight Highlight;

        [HideInInspector] public RouteNode[] RouteNodes;

        private void Awake() {
            RouteNodes = GetComponentsInChildren<RouteNode>(true);
        }
    }

    [Flags]
    public enum PathNodeFlags : uint {
        IsTemporary = 0x01,
        IsSupply = 0x02,
        IsDestination = 0x04,
        IsConversion = 0x10,
    }
}