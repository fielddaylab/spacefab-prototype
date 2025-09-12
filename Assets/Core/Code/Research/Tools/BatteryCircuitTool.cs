using BeauRoutine;
using BeauUtil;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class BatteryCircuitTool : MonoBehaviour {
        [Range(-1, 1)] public float InputVoltage;
        [Range(0, 1)] public float Temperature;

        [NonSerialized] private ResearchTool m_Tool;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);
        }

        private void OnSlotFillUpdated() {
            if (m_Tool.AllSlotsFilled) {
                var input = ResearchToolUtility.GetInputMaterial(m_Tool, 0);
                float current = ResearchMaterialUtility.GetCurrent(input, InputVoltage, Temperature);
                CircuitUtility.SetLightStrength(m_Tool.Circuit, current);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, current * 4);
            } else {
                CircuitUtility.SetLightStrength(m_Tool.Circuit, 0);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, 0);
            }
        }
    }
}