using BeauUtil;
using FieldDay.Components;
using FieldDay.Rendering;
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
            //rig.Renderer.sharedMaterial = material.Material;
            rig.Label.SetText(material.SampleNumber);
        }
    }
}