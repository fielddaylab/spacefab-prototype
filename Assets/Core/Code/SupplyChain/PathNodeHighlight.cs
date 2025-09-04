using BeauUtil;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class PathNodeHighlight : BatchedComponent {
        [Required] public SpriteRenderer PathHighlight;
        [Required] public SpriteRenderer HoverHighlight;
    }
}