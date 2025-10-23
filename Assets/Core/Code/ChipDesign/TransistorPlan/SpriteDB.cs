using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign {
    public class SpriteDB : MonoBehaviour
    {
        public static SpriteDB Instance;

        [Header("Metal")]
        public Sprite Metal;

        [Header("Transistors")]
        public Sprite Transistor;
        public Color NColor;
        public Color PColor;

        private void Awake()
        {
            Instance = this;
        }
    }
}