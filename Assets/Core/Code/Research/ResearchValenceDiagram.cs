using BeauRoutine;
using BeauUtil.UI;
using FieldDay;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    public sealed class ResearchValenceDiagram : MonoBehaviour {
        [Header("Single Atom")]
        public ActiveGroup AtomGroup;
        public Image Nucleus;
        public float ScaleReference = 1;
        public TMP_Text Symbol;

        [Header("Single Atom Electrons")]
        public float ElectronOrbitMultiplier = 1;
        public float ElectronOrbitOffset = 0.5f;
        public Transform[] Electrons;

        [Header("Compound View")]
        public ActiveGroup CompoundGroup;
        public Image[] CompoundAtoms;
    }

    static public partial class ResearchMaterialUtility {
        static public void PopulateDiagram(ResearchValenceDiagram diagram, ResearchMaterial material, bool symbolKnown) {
            if (material.Atoms.Length > 1) {
                PopulateMultiAtomDiagram(diagram, material, symbolKnown);
            } else {
                PopulateSingleAtomDiagram(diagram, material, symbolKnown);
            }
        }

        static private void PopulateSingleAtomDiagram(ResearchValenceDiagram diagram, ResearchMaterial material, bool symbolKnown) {
            ResearchSprites sprites = Find.GlobalAsset<ResearchSprites>();

            diagram.CompoundGroup.SetActive(false);
            diagram.AtomGroup.SetActive(true);

            diagram.Nucleus.sprite = sprites.AtomIcons[(int) material.Atoms[0].Appearance];
            diagram.Nucleus.color = material.Atoms[0].Color;
            diagram.Symbol.SetText(symbolKnown ? material.ChemicalSymbol : "?");

            Transform nucleusTransform = diagram.Nucleus.transform;
            Vector3 centerPos = nucleusTransform.localPosition;
            float scale = material.Atoms[0].Size / diagram.ScaleReference;
            nucleusTransform.SetScale(scale);

            float electronOffset = diagram.ElectronOrbitMultiplier * scale + diagram.ElectronOrbitOffset;

            float angleRad = Mathf.Deg2Rad * 100f;
            float angleIncrement = Mathf.PI * 2 / diagram.Electrons.Length;

            int electronCount = Math.Min(material.Atoms[0].ValenceElectrons, diagram.Electrons.Length);

            for (int i = 0; i < electronCount; i++) {
                diagram.Electrons[i].gameObject.SetActive(true);
                diagram.Electrons[i].localPosition = new Vector3(
                    centerPos.x + Mathf.Cos(angleRad) * electronOffset,
                    centerPos.y + Mathf.Sin(angleRad) * electronOffset,
                    centerPos.z
                    );
                angleRad += angleIncrement;
            }

            for (int i = electronCount; i < diagram.Electrons.Length; i++) {
                diagram.Electrons[i].gameObject.SetActive(false);
            }
        }

        static private void PopulateMultiAtomDiagram(ResearchValenceDiagram diagram, ResearchMaterial material, bool symbolKnown) {
            ResearchSprites sprites = Find.GlobalAsset<ResearchSprites>();

            diagram.AtomGroup.SetActive(false);
            diagram.CompoundGroup.SetActive(true);

            for(int i = 0; i < diagram.CompoundAtoms.Length; i++) {
                Image atomVisual = diagram.CompoundAtoms[i];
                AtomicStructure atomData = material.Atoms[i % material.Atoms.Length];
                float scale = atomData.Size / diagram.ScaleReference;
                atomVisual.transform.SetScale(scale);
                atomVisual.sprite = sprites.AtomIcons[(int)atomData.Appearance];
                atomVisual.color = atomData.Color;
            }
        }
    }
}