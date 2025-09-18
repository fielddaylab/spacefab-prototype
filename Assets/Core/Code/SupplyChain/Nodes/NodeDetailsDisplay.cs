using TMPro;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class NodeDetailsDisplay : MonoBehaviour {
        public TMP_Text DisplayName;
        public SpriteRenderer[] Materials;
        public TMP_Text Cost;
        public TMP_Text Time;
        public SpriteRenderer Defense;
    }
}