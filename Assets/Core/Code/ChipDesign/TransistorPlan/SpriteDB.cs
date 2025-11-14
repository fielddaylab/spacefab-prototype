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
        public Sprite NSide;
        public Sprite PSide;
        public Sprite InvertedOverlay;
        public Sprite InvertedOverlayBase;

        [Header("Vias")]
        public Sprite Via;
        public Sprite ViaHigh;
        public Sprite ViaLow;
        public Sprite ViaNeutral;
        public Sprite ViaUnstable;

        [Header("Gates")]
        public Sprite Gate;
        public Sprite GateHigh;
        public Sprite GateLow;
        public Sprite GateUnstable;

        [Header("IO")]
        public Sprite IOInner;
        public Sprite IOOuter;

        [Header("Flow")]
        public Sprite FlowHi;
        public Sprite FlowLo;
        public Sprite FlowUnstable;


        private void Awake()
        {
            Instance = this;
        }
    }
}