using BeauUtil;
using FieldDay;
using FieldDay.Collections;
using FieldDay.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    [RequireComponent(typeof(ResearchObservationChip))]
    public sealed class ResearchHypothesisChip : MonoBehaviour {
        public LayoutOptions Layout;
        public float ChipBaseY = -28;
        public ResearchObservationChip[] Dependencies;

        public GameObject WrongStationGroup;
    }

    static public partial class ResearchMaterialUtility {
    }
}