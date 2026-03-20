using BeauUtil;
using FieldDay.Components;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FieldDay.UI.Widgets {
    [RequireComponent(typeof(RectTransform))]
    public abstract class GuiWidget : BatchedComponent {
        [NonSerialized] private RectTransform m_RectTransform;
        [NonSerialized] private IGuiPanel m_Panel;

        [SerializeField] private SerializedHash32 m_Id;
        [SerializeField] private SerializedHash32 m_Class;
        [SerializeField] private SerializedHash32 m_Group;

        /// <summary>
        /// The identifier for this widget.
        /// </summary>
        public StringHash32 Id {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Id; }
        }

        /// <summary>
        /// The class identifier for this widget.
        /// </summary>
        public StringHash32 Class {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Class; }
        }

        /// <summary>
        /// The group identifier for this widget.
        /// </summary>
        public StringHash32 Group {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Group; }
        }

        /// <summary>
        /// The RectTransform.
        /// </summary>
        public RectTransform Rect {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ReferenceEquals(m_RectTransform, null) ? (m_RectTransform = (RectTransform)transform) : m_RectTransform; }
        }

        /// <summary>
        /// The closest parent GuiPanel.
        /// </summary>
        public IGuiPanel Panel {
            get { return ReferenceEquals(m_Panel, null) ? (m_Panel = GetComponentInParent<IGuiPanel>()) : m_Panel; }
        }
    }
}