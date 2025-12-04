using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.SharedState;
using FieldDay.UI.Widgets;
using SpaceFab.Research;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
	public sealed class ResearchKnownPropertyChipWidget : GuiWidget {
        public RectGraphic Background;
        public TMP_Text Text;
        public GameObject ConfirmedGroup;

        public void SetHidden() {
            gameObject.SetActive(false);
        }

        public void SetEmpty() {
            gameObject.SetActive(true);
            Background.Outline = true;
            Background.color = Text.color = ColorBank.LightGray;
            Text.text = "---";
            ConfirmedGroup.SetActive(false);
        }

        public void SetLabel(string label, bool confirmed) {
            gameObject.SetActive(true);
            Background.Outline = false;
            Background.color = ColorBank.Black;
            Text.color = confirmed ? ColorBank.White : ColorBank.LightGray;
            Text.text = label;
            ConfirmedGroup.SetActive(!confirmed);
        }
    }
}