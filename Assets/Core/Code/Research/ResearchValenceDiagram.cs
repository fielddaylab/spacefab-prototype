using BeauRoutine;
using BeauUtil.UI;
using FieldDay;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    public sealed class ResearchValenceDiagram : MonoBehaviour {
        public RegularPolyGraphic Nucleus;
        public float ScaleReference = 1;
        public TMP_Text Symbol;

        [Header("Electrons")]
        public float ElectronOrbitMultiplier = 1;
        public float ElectronOrbitOffset = 0.5f;
        public Transform[] Electrons;
    }

    static public partial class ResearchMaterialUtility {
        static public void PopulateDiagram(ResearchValenceDiagram diagram, ResearchMaterial material, bool symbolKnown) {
            diagram.Nucleus.color = material.DiagramColor;
            diagram.Symbol.SetText(symbolKnown ? material.ChemicalSymbol : "?");

            Transform nucleusTransform = diagram.Nucleus.transform;
            Vector3 centerPos = nucleusTransform.localPosition;
            float scale = material.Atoms[0].Size / diagram.ScaleReference;
            nucleusTransform.SetScale(scale);

            float electronOffset = diagram.ElectronOrbitMultiplier * scale + diagram.ElectronOrbitOffset;
            
            float angleRad = Mathf.Deg2Rad * 100f;
            float angleIncrement = Mathf.PI * 2 / diagram.Electrons.Length;

            int electronCount = Math.Min(material.Atoms[0].ValenceElectrons, diagram.Electrons.Length);
            
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