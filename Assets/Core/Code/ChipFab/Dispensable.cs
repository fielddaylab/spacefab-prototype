using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum DispensableType
    {
        Wafer,
        Metal,
        Dopant
    }

    public class Dispensable : MonoBehaviour
    {
        public DispensableType Type;
    }
}