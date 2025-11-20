using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.Scenes;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    [PreloadOrder(10)]
    public sealed class BatteryCircuitTool : MonoBehaviour, IScenePreload {
        public VoltageControl Voltage;
        [Range(0, 1)] public float Temperature;

        [NonSerialized] private ResearchTool m_Tool;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);
            Voltage.OnVoltageModified.Register(OnSlotFillUpdated);
        }

        private void OnUnlockedToolsChanged(ResearchToolsMask toolsMask) {
            Voltage.SetAdjustable((toolsMask & ResearchToolsMask.AdjustableBattery) != 0);
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

        public IEnumerator<WorkSlicer.Result?> Preload() {
            Find.State<ResearchToolState>().OnUnlockedToolsChanged.Register(OnUnlockedToolsChanged);
            Voltage.SetAdjustable(false);
            return null;
        }
    }
}