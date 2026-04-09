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
        public ResearchSpriteButton FlipButton;
        public SpriteRenderer VoltageIcon;
        public Transform BatteryFlip;

        [NonSerialized] public int VoltageIndex;
        [NonSerialized] public bool CanAdjust = true;
        [NonSerialized] private ResearchVoltageConfig m_Config;

        public CastableEvent<float> OnVoltageModified = new CastableEvent<float>(1);

        private void Awake() {
            if (IncreaseButton) {
                IncreaseButton.gameObject.SetActive(true);
                IncreaseButton.Cursor.onClick.Register(OnClickIncrease);
            }
            if (DecreaseButton) {
                DecreaseButton.gameObject.SetActive(true);
                DecreaseButton.Cursor.onClick.Register(OnClickDecrease);
            }
            if (FlipButton) {
                FlipButton.gameObject.SetActive(true);
                FlipButton.Cursor.onClick.Register(OnClickFlip);
            }

            BatteryFlip.localEulerAngles = new Vector3(0, 0, 0);

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
                        if (IncreaseButton) {
                            GuiCommands.SetActive(IncreaseButton.gameObject, false);
                        }
                        if (DecreaseButton) {
                            GuiCommands.SetActive(DecreaseButton.gameObject, false);
                        }
                        if (FlipButton) {
                            GuiCommands.SetActive(FlipButton.gameObject, false);
                        }
                    }
                } else {
                    if (IncreaseButton) {
                        GuiCommands.SetActive(IncreaseButton.gameObject, CanAdjust && VoltageIndex < m_Config.Voltages.Length - 1);
                    }
                    if (DecreaseButton) {
                        GuiCommands.SetActive(DecreaseButton.gameObject, CanAdjust && VoltageIndex > 0);
                    }
                    if (FlipButton) {
                        GuiCommands.SetActive(FlipButton.gameObject, true);
                    }
                }
            }
        }

        private void ResetToolState() {
            VoltageIndex = m_Config.DefaultIndex;
            VoltageIcon.sprite = m_Config.VoltageIcons[VoltageIndex];
            InputVoltage = m_Config.Voltages[VoltageIndex];
            if (IncreaseButton) {
                IncreaseButton.gameObject.SetActive(CanAdjust && VoltageIndex < m_Config.Voltages.Length - 1);
            }
            if (DecreaseButton) {
                DecreaseButton.gameObject.SetActive(CanAdjust && VoltageIndex > 0);
            }
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

        private void OnClickFlip() {
            VoltageIndex = 2 * m_Config.CenterIndex - VoltageIndex;
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

            if (IncreaseButton) {
                GuiCommands.SetActive(IncreaseButton.gameObject, CanAdjust && VoltageIndex < m_Config.Voltages.Length - 1);
            }

            if (DecreaseButton) {
                GuiCommands.SetActive(DecreaseButton.gameObject, CanAdjust && VoltageIndex > 0);
            }

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