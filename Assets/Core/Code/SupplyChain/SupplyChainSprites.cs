using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [CreateAssetMenu(menuName = "SupplyChain/Sprite Settings")]
    public sealed class SupplyChainSprites : GlobalAsset {
        public Sprite[] MaterialSprites;
        public Sprite[] MaterialSpriteOutlines;
        public Sprite[] MaterialSpriteIcons;

        public float[] DefenseThresholds = new float[6];
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

        public int GetDefenseIndex(float percent) {
            for (int i = DefenseThresholds.Length; i-- > 0;) {
                if (DefenseThresholds[i] <= percent) {
                    return i;
                }
            }
            return 0;
        }
    }
}