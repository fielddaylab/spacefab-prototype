using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign {
    public class SpriteDB : MonoBehaviour
    {
        public static SpriteDB Instance;

        [Header("Metal")]
        public Sprite Metal;
        public PathLibrary MetalLibrary;

        [Header("Transistors")]
        public Sprite Transistor;
        public PathLibrary TransistorLibrary;
        public Color NColor;
        public Color PColor;

        private void Awake()
        {
            Instance = this;
        }
    }
}