using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    [CreateAssetMenu(menuName = "ChipFab/Etch Mask Data")]
    public class EtchMaskData : ScriptableObject
    {
        public MaskId MaskId;
        public Vector3[] TracePoints;
    }
}