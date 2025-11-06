using BeauUtil;
using BeauUtil.UI;
using FieldDay.Components;
using FieldDay.UI.Widgets;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchGuessButtonWidget : GuiWidget {
        public Transform Position;
        public Collider2D Collider;
        public PointerListener Listener;
        public GameObject SelectionHighlight;
        public ResearchGuessButtonType ButtonType;

        [NonSerialized] public int Index;
    }

    public enum ResearchGuessButtonType {
        Default,
        Exclusive,
        DeselectOther
    }
}