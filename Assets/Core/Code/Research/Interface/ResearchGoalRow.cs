using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.SharedState;
using FieldDay.UI;
using SpaceFab.Research;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
	public sealed class ResearchGoalRow : MonoBehaviour {
        public TMP_Text Text;
        public Graphic Checkbox;
        public Graphic CrossOff;
        public Graphic Flash;
    }
}