using BeauRoutine.Extensions;
using BeauUtil;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceFab {
    public sealed class DissolveTransitionRenderer : MonoBehaviour {
        [Serializable]
        public struct InkTextureParams {
            public float MinOffsetX;
            public float MaxOffsetX;
            public float MinOffsetY;
            public float MaxOffsetY;
        }

        public Material Material;
        public InkTextureParams InkParams;
        public InkTextureParams Mask1Params;
        public InkTextureParams Mask2Params;

        [NonSerialized] private bool m_Initialized;
        [NonSerialized] private float m_LastCutoff;
        [NonSerialized] private bool m_LastInverted;

        private void Awake() {
            Initialize();
            RandomizeBackground();
            RandomizeMasks();
        }

        private void Initialize() {
            if (!m_Initialized) {
                Material.SetFloat("_Cutoff", 0);
                Material.DisableKeyword("INVERT_CUTOFF");
                m_LastCutoff = 0;
                m_LastInverted = false;
                m_Initialized = true;
            }
        }

        public bool RandomizeBackground() {
            if (m_LastCutoff > 0) {
                return false;
            }

            RandomizeTextureOffsets(Material, "_InkTex", InkParams);
            return true;
        }

        public bool RandomizeMasks() {
            if (m_LastCutoff > 0 && m_LastCutoff < 1) {
                return false;
            }

            RandomizeTextureOffsets(Material, "_MaskTex0", Mask1Params);
            RandomizeTextureOffsets(Material, "_MaskTex1", Mask1Params);
            Material.SetFloat("_Mask0Rot", RNG.Instance.NextFloat(Mathf.PI * 2));
            Material.SetFloat("_Mask1Rot", RNG.Instance.NextFloat(Mathf.PI * 2));
            return true;
        }

        static private void RandomizeTextureOffsets(Material material, string paramName, in InkTextureParams texParams) {
            float x = RNG.Instance.NextFloat(texParams.MinOffsetX, texParams.MaxOffsetX);
            float y = RNG.Instance.NextFloat(texParams.MinOffsetY, texParams.MaxOffsetY);
            material.SetTextureOffset(paramName, new Vector2(x, y));
        }

        public void SetInvertedBlend(bool inverted) {
            Initialize();

            if (m_LastInverted != inverted) {
                m_LastInverted = inverted;
                if (inverted) {
                    Material.EnableKeyword("INVERT_CUTOFF");
                } else {
                    Material.DisableKeyword("INVERT_CUTOFF");
                }
            }
        }

        public void SetCutoff(float cutoff) {
            Initialize();

            if (!Mathf.Approximately(cutoff, m_LastCutoff)) {
                m_LastCutoff = cutoff;
                Material.SetFloat("_Cutoff", cutoff);
            }
        }
    }
}