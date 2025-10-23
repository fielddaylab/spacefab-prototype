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
        public Sprite NTransistor;
        public Sprite PTransistor;

        private void Awake()
        {
            Instance = this;
        }
    }
}