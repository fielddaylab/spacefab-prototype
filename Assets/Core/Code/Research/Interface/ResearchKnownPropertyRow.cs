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
	public sealed class ResearchKnownPropertyRow : MonoBehaviour {
        public ResearchKnownPropertyChipWidget[] Chips;
        public CursorHint Click;
        public Graphic Flash;
        public CanvasGroup Group;

        [NonSerialized] public int ChipsWritten;

        public void WriteEmptyRow() {
            PrepareWrite();
            FinishWrite();
        }

        public void PrepareWrite() {
            ChipsWritten = 0;
        }

        public void WriteChip(string label, bool confirmed) {
            Chips[ChipsWritten++].SetLabel(label, confirmed);
        }

        public void FinishWrite() {
            for(int i = ChipsWritten; i < Chips.Length; i++) {
                Chips[i].SetEmpty();
            }
        }
    }
}