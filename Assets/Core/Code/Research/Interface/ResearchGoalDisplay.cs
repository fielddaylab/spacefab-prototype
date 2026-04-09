using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Collections;
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
using UnityEngine.UIElements;

namespace SpaceFab.Research {
    [PreloadOrder(102)]
	public sealed class ResearchGoalDisplay : MonoBehaviour, IScenePreload, ISceneLateInitialize {
        public TMP_Text GoalLabel;
        public ResearchGoalRow[] Rows;
        public ResearchMaterialGoal[] Objectives;
        public GameObject EndLevelButtonGroup;
        public PointerListener BackToLevelSelectButton;
        public LayoutOptions ContentLayout;
        public LayoutSizeGroup LayoutSizer;

        [NonSerialized] public BitSet32 Completed;

        void ISceneLateInitialize.LateInitialize() {
            Assert.True(Objectives.Length <= Rows.Length);
            if (Objectives.Length > 0) {
                SpaceFabGame.Events.Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_KnowledgeUpdated, OnMaterialKnowledgeUpdated); 
            }

            int rowCount = 0;
            using(TempReferenceBuffer<RectTransform> rowLayoutRefs = TempReferenceBuffer<RectTransform>.Create(Rows.Length)) {
                rowLayoutRefs.Add(GoalLabel.rectTransform);
                foreach(var objective in Objectives) {
                    ResearchGoalRow row = Rows[rowCount++];
                    ResearchMaterialUtility.PopulateObservationChip(row.Display, objective.Chip, objective.ContextId);
                    row.gameObject.SetActive(true);
                    rowLayoutRefs.Add((RectTransform) row.transform);
                }

                float height = Positioning.VerticalLayout(rowLayoutRefs, ContentLayout, 0);
                LayoutSizer.Root.GetComponent<LayoutSizeInfo>().Size.y = height;
                LayoutSizer.Sync();
            }

            for(int i = rowCount; i < Rows.Length; i++) {
                Rows[i].gameObject.SetActive(false);
            }

            LayoutSizer.Sync();

            if (rowCount == 0) {
                EndLevelButtonGroup.SetActive(true);
            }

            BackToLevelSelectButton.onClick.AddListener(() => {
                Game.Scenes.LoadMainScene(SceneReference.FromName("ResearchLoader"));
            });
        }

        private void OnDestroy() {
            Game.Events?.DeregisterAllForContext(this);
        }

        private void OnMaterialKnowledgeUpdated(ResearchMaterialKnowledgePair pair) {
            bool wasComplete = Completed.Count == Objectives.Length;
            if (wasComplete) {
                return;
            }

            for (int i = 0; i < Objectives.Length; i++) {
                if (Completed.IsSet(i)) {
                    continue;
                }

                if (pair.MaterialId != Objectives[i].ContextId) {
                    continue;
                }

                if ((pair.Chip & Objectives[i].Chip) == Objectives[i].Chip) {
                    Completed.Set(i);

                    //Rows[i].Checkbox.SetAlpha(0.5f);
                    //Rows[i].CrossOff.enabled = true;
                    //Rows[i].Hint.gameObject.SetActive(false);
                    FlashAnim.Play(Rows[i].Flash, Color.white, FlashAnim.Default);
                }
            }

            if (Completed.Count == Objectives.Length) {
                EndLevelButtonGroup.SetActive(true);
            }
        }

        public IEnumerator<WorkSlicer.Result?> Preload() {
            if (ResearchGame.CurrentLevel != null) {
                Objectives = ResearchGame.CurrentLevel.Objectives;
                GoalLabel.SetText(ResearchGame.CurrentLevel.Label + " goals");
            }
            return null;
        }

        private void OnGoalHintClicked(CursorHint.EventData evtData) {
            ResearchGoalRow row = (ResearchGoalRow)evtData.Source.UserData;
            //SpaceFabGame.Events.Dispatch(ResearchMaterialUtility.Event_GoalHintRequested, EvtArgs.Create(row.Goal));
        }
    }
}