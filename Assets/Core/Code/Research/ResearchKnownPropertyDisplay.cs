using FieldDay;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.SharedState;
using SpaceFab.Research;
using System;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
	public sealed class ResearchKnownPropertyDisplay : MonoBehaviour {
		public TMP_Text MaterialTitle;
        public TMP_Text ElectricProperty;
        public TMP_Text ThermalProperty;
        public TMP_Text SpecialProperty;

        private void Awake() {
            DisplayNull();

            Find.State<ResearchSelectionState>().OnUpdated.Register(DisplayCurrent);
        }

        private void DisplayNull() {
            MaterialTitle.SetText("???");
            ElectricProperty.SetText("???");
            ThermalProperty.SetText("???");
            SpecialProperty.SetText("???");
        }

        private void DisplayCurrent(ResearchMaterial material) {
            if (!material) {
                DisplayNull();
                return;
            }

            MaterialTitle.SetText(material.DisplayName);
        }
    }
}