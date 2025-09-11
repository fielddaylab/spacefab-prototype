using FieldDay.Components;
using FieldDay.HID;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchMaterialItem : BatchedComponent {
        public ResearchMaterialRig Renderer;
        public Collider2D Clickable;
        public CursorHint Hint;

        [NonSerialized] public ResearchMaterial Material;
        [NonSerialized] public ResearchSlot CurrentSlot;
    }
}