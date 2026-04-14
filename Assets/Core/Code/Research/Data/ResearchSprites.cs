using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Sprites")]
    public sealed class ResearchSprites : GlobalAsset {
        public Sprite SingleAtomMaterial;
        public Sprite MultiAtomMaterial;

        public Sprite[] AtomIcons;
    }
}