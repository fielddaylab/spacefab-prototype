using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class GameDB : MonoBehaviour
    {
        public static GameDB Instance;

        public Color NDopantColor;
        public Color PDopantColor;

        [Header("Wafer Resist")]
        public Sprite ResistFull;
        public Sprite ResistDeveloped;

        [Header("Wafer Oxide")]
        public Sprite OxideFull;
        public Sprite OxidePatterned;

        [Header("Wafer Metal")]
        public Sprite MetalFull;
        public Sprite MetalStripped;
        public Sprite MetalOxideFilled; // duplicate?

        [Header("Wafer Semiconductor")]
        public Sprite SemiEmpty;
        public Sprite SemiDopedN;
        public Sprite SemiDopedP;
        public Sprite SemiDopedNP;

        private void Awake()
        {
            Instance = this;
        }
    }
}