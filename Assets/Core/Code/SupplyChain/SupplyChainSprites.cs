using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [CreateAssetMenu(menuName = "SupplyChain/Sprite Settings")]
    public sealed class SupplyChainSprites : GlobalAsset {
        public Sprite[] MaterialSprites;
        public Sprite[] MaterialSpriteOutlines;
        public Sprite[] MaterialSpriteIcons;
        public Sprite[] DefenseSprites;

        public Sprite MaterialSprite(FabMaterial mat) {
            if (mat > 0) {
                return MaterialSprites[(int) mat - 1];
            }
            return null;
        }

        public Sprite MaterialSpriteOutline(FabMaterial mat) {
            if (mat > 0) {
                return MaterialSpriteOutlines[(int)mat - 1];
            }
            return null;
        }

        public Sprite MaterialSpriteTiny(FabMaterial mat) {
            if (mat > 0) {
                return MaterialSpriteIcons[(int) mat - 1];
            }
            return null;
        }

        public Sprite DefenseSprite(int defense) {
            return DefenseSprites[defense];
        }
    }
}