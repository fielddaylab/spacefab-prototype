using BeauUtil;
using BeauUtil.UI;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchGuessButtonWidget : BatchedComponent {
        public Transform Position;
        public Collider2D Collider;
        public PointerListener Listener;
        public SerializedHash32 Data;

        [NonSerialized] public int Index;
    }
}