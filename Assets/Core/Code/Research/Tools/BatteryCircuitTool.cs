using BeauRoutine;
using BeauUtil;
using FieldDay.Components;
using FieldDay.UI;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class BatteryCircuitTool : MonoBehaviour {
        [Range(-1, 1)] public float InputVoltage;
        [Range(0, 1)] public float Temperature;

        public ResearchSpriteButton IncreaseButton;
        public ResearchSpriteButton DecreaseButton;
        public SpriteRenderer VoltageIcon;
        public Transform BatteryFlip;
        public Sprite[] VoltageIcons;

        [NonSerialized] private ResearchTool m_Tool;
        [NonSerialized] public int VoltageIndex;

        private void Awake() {
            this.CacheComponent(ref m_Tool);

            VoltageIndex = 3;
            VoltageIcon.sprite = VoltageIcons[3];
            InputVoltage = 0.5f;
            IncreaseButton.gameObject.SetActive(true);
            DecreaseButton.gameObject.SetActive(true);
            BatteryFlip.localEulerAngles = new Vector3(0, 0, 0);

            m_Tool.OnInputSlotsUpdated.Register(OnSlotFillUpdated);

            IncreaseButton.Cursor.onClick.Register(OnClickIncrease);
            DecreaseButton.Cursor.onClick.Register(OnClickDecrease);
        }

        private void OnDisable() {
            VoltageIndex = 3;
            VoltageIcon.sprite = VoltageIcons[3];
            InputVoltage = 0.5f;
            IncreaseButton.gameObject.SetActive(true);
            DecreaseButton.gameObject.SetActive(true);
            BatteryFlip.localEulerAngles = new Vector3(0, 0, 0);
        }

        private void OnClickIncrease() {
            VoltageIndex++;
            OnVoltageAdjusted();
        }

        private void OnClickDecrease() {
            VoltageIndex--;
            OnVoltageAdjusted();
        }

        private void OnVoltageAdjusted() {
            VoltageIcon.sprite = VoltageIcons[VoltageIndex];
            if (VoltageIndex < 2) {
                BatteryFlip.localEulerAngles = new Vector3(0, 0, 180);
            } else if (VoltageIndex > 2) {
                BatteryFlip.localEulerAngles = new Vector3(0, 0, 0);
            }
            InputVoltage = (VoltageIndex - 2) / 2f;
            OnSlotFillUpdated();

            GuiCommands.SetActive(IncreaseButton.gameObject, VoltageIndex < 4);
            GuiCommands.SetActive(DecreaseButton.gameObject, VoltageIndex > 0);
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