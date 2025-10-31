using BeauRoutine;
using BeauUtil;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.UI;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class BatteryCircuitTool : MonoBehaviour {
        public VoltageControl Voltage;
        [Range(0, 1)] public float Temperature;

        [NonSerialized] private ResearchTool m_Tool;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);
            Voltage.OnVoltageModified.Register(OnSlotFillUpdated);
        }

        private void OnSlotFillUpdated() {
            if (m_Tool.AllSlotsFilled) {
                var input = ResearchToolUtility.GetInputMaterial(m_Tool, 0);
                float current = ResearchMaterialUtility.GetCurrent(input, Voltage.InputVoltage, Temperature);
                CircuitUtility.SetLightStrength(m_Tool.Circuit, current);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, current);
                if (!ResearchMaterialUtility.IsStableAtVoltage(input, Voltage.InputVoltage)) {
                    ResearchMaterialUtility.ExplodeItem(ResearchToolUtility.GetInputMaterialItem(m_Tool, 0), ExplosionStyle.VoltageBreakdown, 1);
                }
                ResearchToolUtility.SetHighMobilityStrength(m_Tool.SlotsEffectPosition, (input.SpecialTags & SpecialTag.HighMobility) != 0 ? current : 0);
            } else {
                CircuitUtility.SetLightStrength(m_Tool.Circuit, 0);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, 0);
                ResearchToolUtility.SetHighMobilityStrength(null, 0);
            }
        }
    }
}