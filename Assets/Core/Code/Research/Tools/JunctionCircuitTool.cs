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
                var inputA = ResearchToolUtility.GetInputMaterial(m_Tool, 0);
                var inputB = ResearchToolUtility.GetInputMaterial(m_Tool, 1);
                if (Voltage.InputVoltage < 0) {
                    Ref.Swap(ref inputA, ref inputB);
                }

                float current = 0;
                if (ResearchMaterialUtility.BehavesAsInsulator(inputA) || ResearchMaterialUtility.BehavesAsInsulator(inputB)) {
                    current = 0;
                } else if (inputA.DopantType == DopantType.P && inputB.DopantType == DopantType.N) {
                    current = 0; // p->n is not allowed
                } else {
                    float currentA = ResearchMaterialUtility.GetCurrent(inputA, Voltage.InputVoltage, Temperature);
                    float currentB = ResearchMaterialUtility.GetCurrent(inputB, Voltage.InputVoltage, Temperature);
                    current = Math.Min(Math.Abs(currentA), Math.Abs(currentB)) * Math.Sign(Voltage.InputVoltage);
                }

                CircuitUtility.SetLightStrength(m_Tool.Circuit, current);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, current);
            } else {
                CircuitUtility.SetLightStrength(m_Tool.Circuit, 0);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, 0);
            }
        }
    }
}