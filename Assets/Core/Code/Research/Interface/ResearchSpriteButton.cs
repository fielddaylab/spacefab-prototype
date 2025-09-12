using FieldDay.Components;
using FieldDay.HID;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchSpriteButton : BatchedComponent {
        public SpriteRenderer Sprite;
        public Collider2D Collider;
        public CursorHint Cursor;
    }
}