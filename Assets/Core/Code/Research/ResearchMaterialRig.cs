using BeauPools;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Rendering;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchMaterialRig : BatchedComponent {
        [Header("Body")]
        public SpriteRenderer Renderer;
        public Transform RendererPosition;

        [Header("Shadow")]
        public SpriteRenderer ShadowRenderer;
        public Transform ShadowPosition;

        [Header("Other")]
        public TMP_Text Label;
        public GameObject Highlight;
    }

    static public partial class ResearchMaterialUtility {
        static public void ApplyPropertiesToRig(ResearchMaterialRig rig, ResearchMaterial material) {
            ResearchSprites sprites = Find.GlobalAsset<ResearchSprites>();
            if (material.Atoms.Length > 1) {
                rig.Renderer.sprite = sprites.MultiAtomMaterial;
            } else {
                rig.Renderer.sprite = sprites.SingleAtomMaterial;
            }
            rig.Renderer.color = material.GemColor;
            rig.ShadowRenderer.sprite = rig.Renderer.sprite;
            rig.Label.SetText(material.SampleNumber);

            float rotation = (material.AssetId.HashValue) / (float) uint.MaxValue;
            rig.RendererPosition.localRotation = rig.ShadowPosition.localRotation = Quaternion.Euler(0, 0, rotation * 360);

            float scale = material.GemScale;
            rig.RendererPosition.localScale = rig.ShadowPosition.localScale = new Vector3(scale, scale, 1);
        }

        static public float CalculateMaterialScaleFactor(ResearchMaterial material) {
            float atomicSize = 0;
            int atoms = 0;
            for(int i = 0; i < material.Atoms.Length; i++) {
                AtomicStructure atom = material.Atoms[i];
                atomicSize += atom.Size * atom.Count;
                atoms += atom.Count;
            }
            atomicSize /= atoms;
            return CalculateAtomicSizeFactor(atomicSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float CalculateAtomicSizeFactor(float atomicSize) {
            return 0.35f + atomicSize / 100;
        }

        static public Color32 CalculateMaterialColor(ResearchMaterial material) {
            float totalWeight = 0;
            Color accumulator = new Color(0, 0, 0, 1);
            for (int i = 0; i < material.Atoms.Length; i++) {
                AtomicStructure atom = material.Atoms[i];
                float weight = atom.Size * atom.Count;
                accumulator.r += weight * atom.Color.r / 255f;
                accumulator.g += weight * atom.Color.g / 255f;
                accumulator.b += weight * atom.Color.b / 255f;
                totalWeight += weight;
            }
            accumulator.r /= totalWeight;
            accumulator.g /= totalWeight;
            accumulator.b /= totalWeight;
            return accumulator;
        }
    }
}