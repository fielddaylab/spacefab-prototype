using System;
using BeauPools;
using FieldDay.Components;
using FieldDay.UI;
using TMPro;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class PortDetailsDisplay : BatchedComponent, IPoolAllocHandler {
        public Vector2 Size;
        public SpriteRenderer Outline;

        [Header("Data")]
        public TMP_Text DisplayName;
        public SpriteRenderer[] Materials;
        public TMP_Text Cost;
        public TMP_Text Time;
        public SpriteRenderer Defense;

        [NonSerialized] public Port Parent;

        void IPoolAllocHandler.OnAlloc() {
        }

        void IPoolAllocHandler.OnFree() {
            Parent = null;
        }
    }
}