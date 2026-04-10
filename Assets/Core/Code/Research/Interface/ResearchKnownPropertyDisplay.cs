using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
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
        [Header("Data Panel")]
        public TMP_Text MaterialTitle;
        public PointerListener SubmitButton;
        public ActiveGroup SubmitInProgress;
        public ResearchGuessDisplay Guesser;

        [NonSerialized] public StringHash32 RootId;

        private void Awake() {
            DisplayNull();

            Find.State<ResearchSelectionState>().OnUpdated.Register(DisplayCurrent);
            Guesser.OnClose.Register(() => {
                //RowHighlight.gameObject.SetActive(false);
                DisplayCurrent(Find.State<ResearchSelectionState>().Current);
            });

            //SubmitButton.onClick.Register(() => Routine.Start(this, OnClickSubmit()).TryManuallyUpdate(0));
            //SpaceFabGame.Events.Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_KnowledgeUpdated, OnKnowledgeUpdated);
        }

        private void OnDestroy() {
            Game.Events?.DeregisterAllForContext(this);
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

        private void OnKnowledgeUpdated(ResearchMaterialKnowledgePair pair) {
            if (pair.MaterialId != RootId) {
                return;
            }
        }

        private void DisplayNull() {
            RootId = default;
            MaterialTitle.SetText("???");
        }

        private void DisplayCurrent(ResearchMaterial material) {
            if (!material) {
                DisplayNull();
                return;
            }

            MaterialTitle.SetText(material.UnknownDisplayName);
        }

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            return null;
        }
    }
}