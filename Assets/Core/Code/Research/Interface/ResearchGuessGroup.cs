using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Scenes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchGuessGroup : MonoBehaviour, IScenePreload {
        public ResearchGuessButtonWidget[] Buttons;

        [NonSerialized] public BitSet32 SelectedIndices;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            return null;
        }
    }
}