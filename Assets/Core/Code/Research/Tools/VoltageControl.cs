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
    public sealed class VoltageControl : MonoBehaviour, IScenePreload {
        [Range(-1, 1)] public float InputVoltage;

        public ResearchSpriteButton IncreaseButton;
        public ResearchSpriteButton DecreaseButton;
        public SpriteRenderer VoltageIcon;
        public Transform BatteryFlip;

        [NonSerialized] public int VoltageIndex;
        [NonSerialized] public bool CanAdjust = true;
        [NonSerialized] private ResearchVoltageConfig m_Config;

        public CastableEvent<float> OnVoltageModified = new CastableEvent<float>(1);

        private void Awake() {
            IncreaseButton.gameObject.SetActive(true);
            DecreaseButton.gameObject.SetActive(true);
            BatteryFlip.localEulerAngles = new Vector3(0, 0, 0);

            IncreaseButton.Cursor.onClick.Register(OnClickIncrease);
            DecreaseButton.Cursor.onClick.Register(OnClickDecrease);

            GetComponentInParent<ResearchTool>().OnReset.Register(ResetToolState);
        }

        private void OnDisable() {
            ResetToolState();
        }

        public void SetAdjustable(bool adjustable) {
            if (CanAdjust != adjustable) {
                CanAdjust = adjustable;
                if (!CanAdjust) {
                    if (VoltageIndex != m_Config.DefaultIndex) {
                        VoltageIndex = m_Config.DefaultIndex;
                        OnVoltageAdjusted();
                    } else {
                        GuiCommands.SetActive(IncreaseButton.gameObject, false);
                        GuiCommands.SetActive(DecreaseButton.gameObject, false);
                    }
                } else {
                    GuiCommands.SetActive(IncreaseButton.gameObject, CanAdjust && VoltageIndex < m_Config.Voltages.Length - 1);
                    GuiCommands.SetActive(DecreaseButton.gameObject, CanAdjust && VoltageIndex > 0);
                }
            }
        }

        private void ResetToolState() {
            VoltageIndex = m_Config.DefaultIndex;
            VoltageIcon.sprite = m_Config.VoltageIcons[VoltageIndex];
            InputVoltage = m_Config.Voltages[VoltageIndex];
            IncreaseButton.gameObject.SetActive(CanAdjust && VoltageIndex < m_Config.Voltages.Length - 1);
            DecreaseButton.gameObject.SetActive(CanAdjust && VoltageIndex > 0);
            BatteryFlip.localEulerAngles = new Vector3(0, 0, 0);
        }

        private void OnClickIncrease() {
            VoltageIndex++;
            Sfx.Play("Research.Tool.Button");
            OnVoltageAdjusted();
        }

        private void OnClickDecrease() {
            VoltageIndex--;
            Sfx.Play("Research.Tool.Button");
            OnVoltageAdjusted();
        }

        private void OnVoltageAdjusted() {
            VoltageIcon.sprite = m_Config.VoltageIcons[VoltageIndex];
            if (VoltageIndex < m_Config.CenterIndex) {
                BatteryFlip.localEulerAngles = new Vector3(0, 0, 180);
            } else if (VoltageIndex > m_Config.CenterIndex) {
                BatteryFlip.localEulerAngles = new Vector3(0, 0, 0);
            }
            InputVoltage = m_Config.Voltages[VoltageIndex];

            GuiCommands.SetActive(IncreaseButton.gameObject, CanAdjust && VoltageIndex < m_Config.Voltages.Length - 1);
            GuiCommands.SetActive(DecreaseButton.gameObject, CanAdjust && VoltageIndex > 0);

            OnVoltageModified.Invoke(InputVoltage);
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            ResearchVoltageConfig config = Find.GlobalAsset<ResearchVoltageConfig>();
            m_Config = config;

            VoltageIndex = config.DefaultIndex;
            VoltageIcon.sprite = config.VoltageIcons[VoltageIndex];
            InputVoltage = config.Voltages[VoltageIndex];
            return null;
        }
    }
}