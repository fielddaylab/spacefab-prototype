using BeauRoutine;
using System;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchValenceDiagram : MonoBehaviour {
        public MeshRenderer Nucleus;
        public float ScaleReference = 1;
        public TMP_Text Symbol;

        [Header("Electrons")]
        public float ElectronOrbitOffset = 0.5f;
        public Transform[] Electrons;
    }

    static public partial class ResearchMaterialUtility {
        static public void PopulateDiagram(ResearchValenceDiagram diagram, ResearchMaterial material, bool symbolKnown) {
            diagram.Nucleus.sharedMaterial = material.Material;
            diagram.Symbol.SetText(symbolKnown ? material.ChemicalSymbol : "?");

            Transform nucleusTransform = diagram.Nucleus.transform;
            Vector3 centerPos = nucleusTransform.localPosition;
            float scale = material.Size / diagram.ScaleReference;
            nucleusTransform.SetScale(scale);

            float electronOffset = scale * 0.5f + diagram.ElectronOrbitOffset;
            
            float angleRad = Mathf.Deg2Rad * 100f;
            float angleIncrement = Mathf.PI * 2 / diagram.Electrons.Length;

            int electronCount = Math.Min(material.ValenceElectrons, diagram.Electrons.Length);
            
            for(int i = 0; i < electronCount; i++) {
                diagram.Electrons[i].gameObject.SetActive(true);
                diagram.Electrons[i].localPosition = new Vector3(
                    centerPos.x + Mathf.Cos(angleRad) * electronOffset,
                    centerPos.y + Mathf.Sin(angleRad) * electronOffset,
                    centerPos.z
                    );
                angleRad += angleIncrement;
            }

            for(int i = electronCount; i < diagram.Electrons.Length; i++) {
                diagram.Electrons[i].gameObject.SetActive(false);
            }
        }
    }
}