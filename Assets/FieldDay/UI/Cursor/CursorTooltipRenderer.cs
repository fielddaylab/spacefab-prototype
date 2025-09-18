using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay.UI {
    public sealed class CursorTooltipRenderer : MonoBehaviour {
        #region Inspector

        [SerializeField] private RectTransform m_Bounds;
        [SerializeField] private RectTransform m_Self;
        [SerializeField] private LayoutGroup m_Layout;
        [SerializeField] private TMP_Text m_Text;
        [SerializeField] private float m_CursorOffset = 16;
        [SerializeField] private float m_ScreenEdgeOffset = 16;

        #endregion // Inspector

        [NonSerialized] private Vector2 m_CursorPoint;

        private void SetPivot(float pivotX, float pivotY) {
            m_Self.pivot = new Vector2(pivotX, pivotY);
        }
    }
}