using BeauUtil;
using System;
using UnityEngine;

namespace FieldDay.UI.Widgets {
    public sealed class GuiCounter : GuiWidget {
        public abstract class Style : GuiWidgetStyle<int> {
        }

        [SerializeField] private int m_StartingValue;
        [SerializeField] private int m_MaxValue;
        [SerializeField, Required] private Style m_Style;

        [NonSerialized] private int m_CurrentValue = -1;

        private void Awake() {
            if (m_CurrentValue < 0) {
                SetValue(m_StartingValue, GuiWidgetUpdateFlags.Force | GuiWidgetUpdateFlags.NoAnimation);
            }
        }

        public int MaxValue {
            get { return m_MaxValue; }
        }

        public int Value {
            get { return m_CurrentValue; }
            set { SetValue(value, 0); }
        }

        public void SetValue(int value, GuiWidgetUpdateFlags flags = 0) {
            if ((flags & GuiWidgetUpdateFlags.Force) == 0 && value == m_CurrentValue) {
                return;
            }

            value = Math.Clamp(value, 0, m_MaxValue);
            m_CurrentValue = value;

            m_Style.Populate(value, flags);
        }

        public void ResetValue() {
            SetValue(m_StartingValue, 0);
        }
    }
}