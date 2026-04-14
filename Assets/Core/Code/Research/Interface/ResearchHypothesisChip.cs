using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Collections;
using FieldDay.UI.Widgets;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    [RequireComponent(typeof(ResearchObservationChip))]
    public sealed class ResearchHypothesisChip : MonoBehaviour {
        public LayoutOptions Layout;
        public float ChipBaseY = -28;
        public ResearchObservationChip[] Dependencies;

        public GameObject WrongStationGroup;

        [NonSerialized] public ResearchObservationChip Base;
        [NonSerialized] public ResearchChipId CurrentChip;
        [NonSerialized] public StringHash32 CurrentContext;
    }

    static public partial class ResearchMaterialUtility {
        static public void PopulateHypothesisChip(ResearchHypothesisChip hypothesis, ResearchChipId chipId, StringHash32 materialContext) {
            ResearchChipMetadata meta = ResearchChipUtility.Metadata(chipId);
            Assert.True(meta.DependencyA != ResearchChipId.None);

            int dependencyCount = meta.DependencyB != ResearchChipId.None ? 2 : 1;
            using(TempReferenceBuffer<RectTransform> chipRects = TempReferenceBuffer<RectTransform>.Create(dependencyCount)) {
                PopulateObservationChip(hypothesis.Dependencies[0], meta.DependencyA, materialContext);
                hypothesis.Dependencies[0].gameObject.SetActive(true);
                chipRects.Add(hypothesis.Dependencies[0].Rect);

                if (dependencyCount > 1) {
                    PopulateObservationChip(hypothesis.Dependencies[1], meta.DependencyB, materialContext);
                    hypothesis.Dependencies[1].gameObject.SetActive(true);
                    chipRects.Add(hypothesis.Dependencies[1].Rect);
                } else {
                    hypothesis.Dependencies[1].gameObject.SetActive(false);
                }

                Positioning.VerticalLayout(chipRects, hypothesis.Layout, hypothesis.ChipBaseY);
            }

            PopulateObservationChip(hypothesis.CacheComponent(ref hypothesis.Base), chipId, materialContext);

            hypothesis.CurrentChip = chipId;
            hypothesis.CurrentContext = materialContext;
        }

        static public void UpdateHypothesisContext(ResearchHypothesisChip hypothesis, StringHash32 materialContext) {
            if (hypothesis.CurrentContext == materialContext) {
                return;
            }

            hypothesis.CurrentContext = materialContext;

            if (hypothesis.CurrentChip == ResearchChipId.None) {
                return;
            }

            if (ResearchChipUtility.RequiresContext(hypothesis.CurrentChip)) {
                ApplyContextualTextToObservationChip(hypothesis.CacheComponent(ref hypothesis.Base), hypothesis.CurrentChip, materialContext);
            }

            ResearchChipMetadata meta = ResearchChipUtility.Metadata(hypothesis.CurrentChip);
            if (meta.DependencyA != ResearchChipId.None && ResearchChipUtility.RequiresContext(meta.DependencyA)) {
                ApplyContextualTextToObservationChip(hypothesis.Dependencies[0], meta.DependencyA, materialContext);
            }
            if (meta.DependencyB != ResearchChipId.None && ResearchChipUtility.RequiresContext(meta.DependencyB)) {
                ApplyContextualTextToObservationChip(hypothesis.Dependencies[1], meta.DependencyB, materialContext);
            }
        }
    }
}