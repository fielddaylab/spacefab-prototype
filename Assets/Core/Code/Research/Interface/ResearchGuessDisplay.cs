using BeauUtil;
using FieldDay;
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
        public LayoutSizeInfo VerticalGroup;
        public LayoutSizeGroup BoxSizer;

        public ActionEvent OnClose = new ActionEvent();

        private void Close() {
            OnClose.Invoke();
            GuiCommands.SetActive(this, false);
        }

        private void OnEnable() {
            Game.Gui.RegisterUpdate(this);
            Find.State<ResearchSelectionState>().Locked = true;
        }

        private void OnDisable() {
            if (Game.IsShuttingDown) {
                return;
            }

            Game.Gui.DeregisterUpdate(this);
            Find.State<ResearchSelectionState>().Locked = false;
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            return null;
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