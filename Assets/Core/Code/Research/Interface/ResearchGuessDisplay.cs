using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Collections;
using FieldDay.HID;
using FieldDay.Scenes;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.Research {
    [PreloadOrder(150)]
    public sealed class ResearchGuessDisplay : MonoBehaviour, IScenePreload, IOnGuiUpdate {
        public ResearchObservationChip[] GuessButtons;
        public LayoutOptions VerticalLayout;
        public LayoutSizeGroup BoxSizer;

        public CastableEvent<ResearchChipId> OnChipAdded = new CastableEvent<ResearchChipId>();
        public ActionEvent OnClose = new ActionEvent();

        [NonSerialized] public int ChipCount;
        [NonSerialized] public bool IsOpen;

        public void Open() {
            if (!IsOpen) {
                IsOpen = true;
                PopulateGuessList();
                BuildLayout();
                UpdateSelectionMask();
                GuiCommands.SetActive(this, true);
            }
        }

        private void PopulateGuessList() {
            ResearchToolState toolState = Find.State<ResearchToolState>();
            ResearchTool tool = toolState.CurrentTool;
            ResearchSelectionState selectionState = Find.State<ResearchSelectionState>();
            StringHash32 contextId = selectionState.Context ? selectionState.Context.AssetId : null;
            
            ChipCount = 0;
            if (tool.isActiveAndEnabled && selectionState.Current) {
                ResearchChipId[] chips = tool.AvailableObservations;
                for (int i = 0; i < chips.Length; i++) {
                    ResearchChipId data = chips[i];
                    if (!IsVisible(data, toolState.CurrentUnlocks)) {
                        continue;
                    }

                    ResearchObservationChip chip = GuessButtons[ChipCount++];
                    chip.gameObject.SetActive(true);
                    ResearchMaterialUtility.PopulateObservationChip(chip, data, contextId);
                }
            }

            for(int i = ChipCount; i < GuessButtons.Length; i++) {
                GuessButtons[i].gameObject.SetActive(false);
            }
        }

        static private bool IsVisible(ResearchChipId chipId, ResearchToolsMask mask) {
            switch (ResearchChipUtility.Category(chipId)) {
                case ResearchChipCategory.SpecialVoltage:
                    return (mask & ResearchToolsMask.AdjustableBattery) != 0;
                case ResearchChipCategory.SpecialMobility:
                case ResearchChipCategory.SpecialLight:
                    return (mask & ResearchToolsMask.SpecialProperties) != 0;
                case ResearchChipCategory.ThermalResistance:
                    return (mask & ResearchToolsMask.ThermalHighHeat) != 0;
                default:
                    return true;
            }
        }

        private void BuildLayout() {
            using(var activeChildren = Positioning.QueryActiveChildren(BoxSizer.Root)) {
                float verticalSize = Positioning.VerticalLayout(activeChildren, VerticalLayout, 0);
                BoxSizer.SetSize(0, verticalSize);
            }
        }

        private void Close() {
            if (IsOpen) {
                IsOpen = false;
                OnClose.Invoke();
                GuiCommands.SetActive(this, false);
            }
        }

        private void OnEnable() {
            Game.Gui.RegisterUpdate(this);
            //Find.State<ResearchSelectionState>().Locked = true;
            IsOpen = true;
        }

        private void OnDisable() {
            if (Game.IsShuttingDown) {
                return;
            }

            ChipCount = 0;
            Game.Gui.DeregisterUpdate(this);
            //Find.State<ResearchSelectionState>().Locked = false;
            IsOpen = false;
        }

        public void UpdateSelectionMask() {
            ResearchSelectionState selectionState = Find.State<ResearchSelectionState>();
            if (!selectionState.Current) {
                return;
            }

            StringHash32 selectionId = selectionState.Current.AssetId;
            StringHash32 contextId = selectionState.Context ? selectionState.Context.AssetId : null;

            ResearchMaterialKnowledge knowledge = ResearchMaterialUtility.GetKnownProperties(selectionId);
            ResearchObservationList observations = ResearchMaterialUtility.GetObservations(selectionId);
            BitSet32 excludedCategories = ResearchMaterialUtility.GetLockedCategoryMask(knowledge, contextId);

            for(int i = 0; i < ChipCount; i++) {
                ResearchObservationChip chip = GuessButtons[i];
                bool exclude = observations.Has(chip.Chip) || excludedCategories.IsSet((int) ResearchChipUtility.Category(chip.Chip));
                chip.Cursor.enabled = !exclude;
                chip.InputBlocker.enabled = exclude;
            }
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            ResearchSelectionState selectionState = Find.State<ResearchSelectionState>();
            selectionState.OnUpdated.Register(Close);
            selectionState.OnUpdatedContext.Register(Close);
            ResearchToolState toolState = Find.State<ResearchToolState>();
            toolState.OnCurrentToolUpdated.Register(Close);
            toolState.OnCurrentToolChangedState.Register(Close);

            for(int i = 0; i < GuessButtons.Length; i++) {
                ResearchObservationChip chip = GuessButtons[i];
                chip.Cursor.UserData = chip;
                chip.Cursor.onClick.Register(OnGuessClicked);
            }

            return null;
        }

        private void OnGuessClicked(PointerListener.EventData evt) {
            ResearchChipId chip = ((ResearchObservationChip) (evt.Source.UserData)).Chip;
            StringHash32 materialId = Find.State<ResearchSelectionState>().Current.AssetId;
            ResearchObservationList list = ResearchMaterialUtility.GetObservations(materialId);
            if (list.TryAdd(chip, out ResearchChipId replaced)) {
                ResearchMaterialUtility.SetObservations(materialId, list);
                OnChipAdded.Invoke(chip);
                UpdateSelectionMask();
            }
        }

        void IOnGuiUpdate.OnGuiUpdate() {
            if (Game.Input.IsMousePressed(MouseButton.Right)) {
                Close();
            } else if (Game.Input.IsMousePressed(MouseButton.Left)) {
                if (!Game.Input.IsPointerOverHierarchy(transform)) {
                    Close();
                }
            }
        }
    }
}