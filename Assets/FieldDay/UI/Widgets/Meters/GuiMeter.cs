using BeauUtil;
using System;
using UnityEngine;

namespace FieldDay.UI.Widgets {
    public sealed class GuiMeter : GuiWidget {
        public abstract class Style : GuiWidgetStyle<int> {
        }

        [SerializeField] private int m_MaxValue;
        [SerializeField] private int m_StartingValue;
        [SerializeField, Required] private Style m_Style;

        [NonSerialized] private int m_CurrentValue;

        private void Awake() {
            m_CurrentValue = m_StartingValue;
        }

        public int MaxValue {
            get { return m_MaxValue; }
        }

        public int Value {
            get { return m_CurrentValue; }
            set { SetValue(value, false); }
        }

        public void SetValue(int value, bool force) {
            if (!force && value == m_CurrentValue) {
                return;
            }

            value = Math.Clamp(value, 0, m_MaxValue);
            m_CurrentValue = value;

            m_Style.Populate(value);
        }

        public void ResetValue() {
            SetValue(m_StartingValue, false);
        }
    }
}