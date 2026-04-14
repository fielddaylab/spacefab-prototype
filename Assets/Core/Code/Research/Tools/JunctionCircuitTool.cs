using BeauUtil;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class JunctionCircuitTool : MonoBehaviour {
        public VoltageControl Voltage;
        [Range(0, 1)] public float Temperature;

        [NonSerialized] private ResearchTool m_Tool;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);
            Voltage.OnVoltageModified.Register(OnSlotFillUpdated);
        }

        private void OnSlotFillUpdated() {
            if (m_Tool.AllSlotsFilled && Voltage.InputVoltage != 0) {
                
            } else {
                CircuitUtility.SetLightStrength(m_Tool.Circuit, 0);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, 0);
                ResearchToolUtility.SetLightEmissionStrength(null, 0);
                ResearchToolUtility.SetHighMobilityStrength(null, 0);
            }
        }
    }
}