using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ThermalTool : MonoBehaviour {
        [Range(-1, 1)] public float InputVoltage;
        [Range(0, 1)] public float Temperature;

        public SpriteRenderer CoilRenderer;
        public Color32[] Colors;
        public string[] Labels;
        public ResearchSpriteButton IncreaseButton;
        public ResearchSpriteButton DecreaseButton;
        public TMP_Text TemperatureLabel;

        [NonSerialized] private ResearchTool m_Tool;
        [NonSerialized] public int TemperatureIndex;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);
            CoilRenderer.color = Colors[0];
            TemperatureLabel.SetText(Labels[0]);
            TemperatureIndex = 0;

            IncreaseButton.Cursor.onClick.Register(OnClickIncrease);
            DecreaseButton.Cursor.onClick.Register(OnClickDecrease);

            IncreaseButton.gameObject.SetActive(true);
            DecreaseButton.gameObject.SetActive(false);
        }

        private void OnDisable() {
            TemperatureIndex = 0;
            CoilRenderer.color = Colors[0];
            TemperatureLabel.SetText(Labels[0]);
            Temperature = 0;
            IncreaseButton.gameObject.SetActive(true);
            DecreaseButton.gameObject.SetActive(false);

        }

        private void OnClickIncrease() {
            TemperatureIndex++;
            CoilRenderer.color = Colors[TemperatureIndex];
            TemperatureLabel.SetText(Labels[TemperatureIndex]);

            Sfx.Play("Research.Tool.Button");
            GuiCommands.SetActive(DecreaseButton.gameObject, true);
            GuiCommands.SetActive(IncreaseButton.gameObject, TemperatureIndex < 5);

            Temperature = TemperatureIndex / 5f;
            OnSlotFillUpdated();
        }

        private void OnClickDecrease() {
            TemperatureIndex--;
            CoilRenderer.color = Colors[TemperatureIndex];
            TemperatureLabel.SetText(Labels[TemperatureIndex]);

            Sfx.Play("Research.Tool.Button");
            GuiCommands.SetActive(IncreaseButton.gameObject, true);
            GuiCommands.SetActive(DecreaseButton.gameObject, TemperatureIndex > 0);

            Temperature = TemperatureIndex / 5f;
            OnSlotFillUpdated();
        }

        private void OnSlotFillUpdated() {
            if (m_Tool.AllSlotsFilled) {
                var input = ResearchToolUtility.GetInputMaterial(m_Tool, 0);
                if (!ResearchMaterialUtility.IsStableAtTemperature(input, Temperature)) {
                    CircuitUtility.SetLightStrength(m_Tool.Circuit, 0);
                    CircuitUtility.SetFlowSpeed(m_Tool.Circuit, 0);
                    ResearchMaterialUtility.ExplodeItem(ResearchToolUtility.GetInputMaterialItem(m_Tool, 0), ExplosionStyle.TemperatureBreakdownHot, 0.4f);
                } else {
                    float current = ResearchMaterialUtility.GetCurrent(input, InputVoltage, Temperature, m_Tool.DopingState);
                    CircuitUtility.SetLightStrength(m_Tool.Circuit, current);
                    CircuitUtility.SetFlowSpeed(m_Tool.Circuit, current);
                    ResearchToolUtility.SetHighMobilityStrength(m_Tool.SlotsEffectPosition, (input.SpecialTags & SpecialTag.HighMobility) != 0 ? current : 0);
                }
            } else {
                CircuitUtility.SetLightStrength(m_Tool.Circuit, 0);
                CircuitUtility.SetFlowSpeed(m_Tool.Circuit, 0);
                ResearchToolUtility.SetHighMobilityStrength(null, 0);
            }
        }
    }
}