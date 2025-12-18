using BeauUtil;
using FieldDay.Components;
using FieldDay.Rendering;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchMaterialRig : BatchedComponent {
        public MeshRenderer Renderer;
        public TMP_Text Label;
        public Transform ShadowPosition;
        public Transform RendererPosition;
        public GameObject Highlight;
    }

    static public partial class ResearchMaterialUtility {
        static public void ApplyPropertiesToRig(ResearchMaterialRig rig, ResearchMaterial material) {
            rig.Renderer.sharedMaterial = material.Material;
            rig.Label.SetText(ResearchMaterialUtility.IsNameKnown(material.AssetId) ? material.ChemicalSymbol : material.SampleNumber);
        }
    }
}