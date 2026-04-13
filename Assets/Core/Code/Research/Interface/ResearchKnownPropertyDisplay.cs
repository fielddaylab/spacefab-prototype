using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI;
using FieldDay.UI.Animation;
using SpaceFab.Research;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.Research {
    [PreloadOrder(101)]
	public sealed class ResearchKnownPropertyDisplay : MonoBehaviour, IScenePreload {
        public ResearchObservationChip[] ChipList;
        public TMP_Text MaterialTitle;
        public PointerListener AddObservationButton;
        public PointerListener SubmitButton;
        public ActiveGroup SubmitInProgress;
        public ResearchGuessDisplay Guesser;
        public ResearchHypothesisPanel Hypothesizer;
        public CursorHint WaitingCursor;

        [NonSerialized] public StringHash32 SelectedId;

        private void Awake() {
            DisplayNull();

            ResearchSelectionState selectionState = Find.State<ResearchSelectionState>();
            selectionState.OnUpdated.Register(DisplayCurrent);
            selectionState.OnUpdatedContext.Register(OnContextChanged);

            ResearchToolState researchToolState = Find.State<ResearchToolState>();
            researchToolState.OnCurrentToolUpdated.Register(UpdateAddButtonAvailability);

            Hypothesizer.OnHypothesisUpdated.Register(OnHypothesisUpdated);

            AddObservationButton.onClick.AddListener(() => {
                Guesser.Open();
            });

            Guesser.OnChipAdded.Register(RefreshList);
            SubmitButton.onClick.Register(OnSubmitClicked);
        }

        private void OnDestroy() {
            Game.Events?.DeregisterAllForContext(this);
        }

        private void UpdateAddButtonAvailability() {
            ResearchToolState researchToolState = Find.State<ResearchToolState>();
            AddObservationButton.gameObject.SetActive(researchToolState.CurrentTool.isActiveAndEnabled);
        }

        private void OnSubmitClicked() {
            Find.State<ResearchSelectionState>().Locked = true;
            Game.Input.PauseRaycasts();
            CursorHint.TryLock(WaitingCursor);
            Routine.Start(this, SubmitSequence());
        }

        private IEnumerator SubmitSequence() {
            yield return 1;

            ResearchSelectionState selectionState = Find.State<ResearchSelectionState>();
            StringHash32 currentSelection = selectionState.Current.AssetId;
            StringHash32 contextId = AssetUtility.IdOf(selectionState.Context);
            ResearchChipId hypothesis = Hypothesizer.CurrentHypothesis;
            ResearchChipMetadata meta = ResearchChipUtility.Metadata(hypothesis);

            ResearchChipId actualChip = ResearchChipUtility.Unalias(hypothesis);

            ResearchMaterialKnowledge knowledge = ResearchMaterialUtility.GetKnownProperties(currentSelection);
            ResearchObservationList obsList = ResearchMaterialUtility.GetObservations(currentSelection);

            bool hasDependencies = true;

            ResearchChipId dependency = meta.DependencyA;
            ResearchChipMetadata dependencyMeta = ResearchChipUtility.Metadata(dependency);
            if (dependency != ResearchChipId.None) {
                if (ResearchChipUtility.IsProperty(dependency)) {
                    if (knowledge.Has(dependency)) {
                        // TODO: animate
                    } else {
                        hasDependencies = false;
                    }
                } else {
                    if (obsList.Has(dependency)) {
                        if (!dependencyMeta.Evaluator(dependency, selectionState.Current, selectionState.Context)) {
                            hasDependencies = false;
                            obsList.Remove(dependency);
                        } else {
                            // TODO: animate
                        }
                    } else {
                        hasDependencies = false;
                        if (obsList.HasCategory(ResearchChipUtility.Category(dependency), out ResearchChipId found)) {
                            dependencyMeta = ResearchChipUtility.Metadata(found);
                            if (!dependencyMeta.Evaluator(found, selectionState.Current, selectionState.Context)) {
                                obsList.Remove(found);
                            }
                        }
                    }
                }
            }

            dependency = meta.DependencyB;
            dependencyMeta = ResearchChipUtility.Metadata(dependency);
            if (dependency != ResearchChipId.None) {
                if (ResearchChipUtility.IsProperty(dependency)) {
                    if (knowledge.Has(dependency)) {
                        // TODO: animate
                    } else {
                        hasDependencies = false;
                    }
                } else {
                    if (obsList.Has(dependency)) {
                        if (!dependencyMeta.Evaluator(dependency, selectionState.Current, selectionState.Context)) {
                            hasDependencies = false;
                            obsList.Remove(dependency);
                        } else {
                            // TODO: animate
                        }
                    } else {
                        hasDependencies = false;
                        if (obsList.HasCategory(ResearchChipUtility.Category(dependency), out ResearchChipId found)) {
                            dependencyMeta = ResearchChipUtility.Metadata(found);
                            if (!dependencyMeta.Evaluator(found, selectionState.Current, selectionState.Context)) {
                                obsList.Remove(found);
                            }
                        }
                    }
                }
            }

            if (hasDependencies) {
                obsList.Remove(meta.DependencyA);
                obsList.Remove(meta.DependencyB);
                knowledge.TryAdd(actualChip, contextId);
                ResearchMaterialUtility.SetKnownProperties(currentSelection, knowledge);
            }
            ResearchMaterialUtility.SetObservations(currentSelection, obsList);
            SpaceFabGame.Events.Queue(ResearchMaterialUtility.Event_KnowledgeUpdated, EvtArgs.Create(new ResearchMaterialKnowledgePair() {
                Chip = actualChip,
                ContextId = contextId,
                MaterialId = SelectedId
            }));

            RefreshList();
            CursorHint.Unlock(WaitingCursor);
            Game.Input.ResumeRaycasts();
            Find.State<ResearchSelectionState>().Locked = false;
        }

        private void OnHypothesisUpdated(ResearchChipId hypothesis) {
            ResearchChipMetadata meta = ResearchChipUtility.Metadata(hypothesis);
            ResearchObservationList obsList = ResearchMaterialUtility.GetObservations(SelectedId);
            ResearchMaterialKnowledge propList = ResearchMaterialUtility.GetKnownProperties(SelectedId);

            bool submitAvailable = !propList.Has(ResearchChipUtility.Unalias(hypothesis));
            ResearchChipCategory propCategory = ResearchChipUtility.Category(hypothesis);
            if (ResearchChipUtility.CategoryIsExclusive(propCategory)) {
                submitAvailable &= !propList.HasCategory(propCategory);
            }

            submitAvailable &= obsList.HasCategory(ResearchChipUtility.Category(meta.DependencyA), out _);
            submitAvailable &= meta.DependencyB == ResearchChipId.None || obsList.HasCategory(ResearchChipUtility.Category(meta.DependencyB), out _);

            SubmitButton.gameObject.SetActive(submitAvailable);
        }

        private void OnChipClicked(PointerListener.EventData evt) {
            ResearchChipId chip = ((ResearchObservationChip) evt.Source.UserData).Chip;
            if (ResearchChipUtility.IsProperty(chip)) {
                return;
            }

            ResearchObservationList obsList = ResearchMaterialUtility.GetObservations(SelectedId);
            if (obsList.Remove(chip)) {
                ResearchMaterialUtility.SetObservations(SelectedId, obsList);
                RefreshList();
                if (Guesser.IsOpen) {
                    Guesser.UpdateSelectionMask();
                }
            }
        }

        private void DisplayNull() {
            SelectedId = default;
            MaterialTitle.SetText("???");
            for(int i = 0; i < ChipList.Length; i++) {
                ChipList[i].gameObject.SetActive(false);
            }
            SubmitButton.gameObject.SetActive(false);
        }
        
        private void OnContextChanged() {
            DisplayCurrent(Find.State<ResearchSelectionState>().Current);
        }

        private void DisplayCurrent(ResearchMaterial material) {
            if (!material) {
                DisplayNull();
                return;
            }

            SelectedId = material.AssetId;
            MaterialTitle.SetText(material.UnknownDisplayName);
            RefreshList();
        }

        private void RefreshList() {
            ResearchObservationList obsList = ResearchMaterialUtility.GetObservations(SelectedId);
            ResearchMaterialKnowledge propList = ResearchMaterialUtility.GetKnownProperties(SelectedId);

            int totalCount = obsList.Count + propList.Count;

            Assert.True(totalCount <= ChipList.Length, "No more chips available to display!");

            int chipIndex = 0;
            for(int i = 0; i < propList.Count; i++) {
                ResearchChipId chip = propList.Chip(i);
                StringHash32 context = propList.Context(i);
                ResearchObservationChip display = ChipList[chipIndex++];
                display.gameObject.SetActive(true);
                display.Cursor.Tooltip = string.Empty;
                ResearchMaterialUtility.PopulateObservationChip(display, chip, context);
            }

            StringHash32 selectionContext = AssetUtility.IdOf(Find.State<ResearchSelectionState>().Context);

            for (int i = 0; i < obsList.Count; i++) {
                ResearchChipId chip = obsList[i];
                ResearchObservationChip display = ChipList[chipIndex++];
                display.Cursor.Tooltip = "Click to remove observation from list";
                display.gameObject.SetActive(true);
                ResearchMaterialUtility.PopulateObservationChip(display, chip, selectionContext);
            }

            for (int i = totalCount; i < ChipList.Length; i++) {
                ChipList[i].gameObject.SetActive(false);
            }

            LayoutOptions layout = default;
            layout.NormalizedAlignment = 1;
            layout.Source = LayoutSource.Size;
            layout.Spacing = 2;

            using(var children = Positioning.QueryActiveChildren((RectTransform) ChipList[0].Rect.parent)) {
                Positioning.VerticalLayout(children, layout, 0);
            }

            OnHypothesisUpdated(Hypothesizer.CurrentHypothesis);
        }

        private ResearchObservationChip GetChipForId(ResearchChipId chipId) {
            for(int i = 0; i < ChipList.Length; i++) {
                if (!ChipList[i].isActiveAndEnabled) {
                    return null;
                }

                if (ChipList[i].Chip == chipId) {
                    return ChipList[i];
                }
            }

            return null;
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            for(int i = 0; i < ChipList.Length; i++) {
                ResearchObservationChip chip = ChipList[i];
                chip.Cursor.UserData = chip;
                chip.Cursor.onClick.Register(OnChipClicked);
            }
            return null;
        }
    }
}