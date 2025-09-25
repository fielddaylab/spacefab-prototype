using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum WaferLayerType
    {
        ResistUndeveloped,
        ResistDeveloped,
        Oxide,
    }

    public class WaferLayer : MonoBehaviour
    {
        public WaferLayerType LayerType;
    }
}