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

        [NonSerialized] public StringHash32 SelectedId;

        private void Awake() {
            DisplayNull();

            ResearchSelectionState selectionState = Find.State<ResearchSelectionState>();
            selectionState.OnUpdated.Register(DisplayCurrent);
            selectionState.OnUpdatedContext.Register(OnContextChanged);

            ResearchToolState researchToolState = Find.State<ResearchToolState>();
            researchToolState.OnCurrentToolUpdated.Register(UpdateAddButtonAvailability);

            AddObservationButton.onClick.AddListener(() => {
                Guesser.Open();
            });

            Guesser.OnChipAdded.Register(RefreshList);

            //SubmitButton.onClick.Register(() => Routine.Start(this, OnClickSubmit()).TryManuallyUpdate(0));
            //SpaceFabGame.Events.Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_KnowledgeUpdated, OnKnowledgeUpdated);
        }

        private void OnDestroy() {
            Game.Events?.DeregisterAllForContext(this);
        }

        private void UpdateAddButtonAvailability() {
            ResearchToolState researchToolState = Find.State<ResearchToolState>();
            AddObservationButton.gameObject.SetActive(researchToolState.CurrentTool.isActiveAndEnabled);
        }

        private IEnumerator OnClickSubmit() {
            Find.State<ResearchSelectionState>().Locked = true;

            SubmitButton.gameObject.SetActive(false);
            SubmitInProgress.SetActive(true);

            yield return 1;

            Find.State<ResearchSelectionState>().Locked = false;

            SubmitInProgress.SetActive(false);
            DisplayCurrent(Find.State<ResearchSelectionState>().Current);
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
                ResearchMaterialUtility.PopulateObservationChip(display, chip, context);
            }

            StringHash32 selectionContext = AssetUtility.IdOf(Find.State<ResearchSelectionState>().Context);

            for (int i = 0; i < obsList.Count; i++) {
                ResearchChipId chip = obsList[i];
                ResearchObservationChip display = ChipList[chipIndex++];
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