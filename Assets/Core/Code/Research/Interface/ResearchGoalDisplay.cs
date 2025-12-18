using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
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
using UnityEngine.UIElements;

namespace SpaceFab.Research {
    [PreloadOrder(102)]
	public sealed class ResearchGoalDisplay : MonoBehaviour, IScenePreload {
        public TMP_Text GoalLabel;
        public ResearchGoalRow[] Rows;
        public ResearchMaterialKnowledgePair[] Objectives;
        public GameObject EndLevelButtonGroup;
        public PointerListener BackToLevelSelectButton;

        [NonSerialized] public BitSet32 Completed;

        private void OnLateEnable() {
            Assert.True(Objectives.Length <= Rows.Length);
            if (Objectives.Length > 0) {
                SpaceFabGame.Events.Register<ResearchMaterialKnowledgePair>(ResearchMaterialUtility.Event_KnowledgeUpdated, OnMaterialKnowledgeUpdated); 
            }
            int rowCount = 0;
            using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                foreach (var objective in Objectives) {
                    psb.Builder.Clear();
                    ResearchGoalRow row = Rows[rowCount++];
                    ResearchMaterial material = Find.NamedAsset<ResearchMaterial>(objective.MaterialId);
                    psb.Builder.Append("Identify the");
                    int bitCount = Bits.Count(objective.Knowledge);
                    if ((objective.Knowledge & ResearchMaterialKnowledge.Electrical) != 0) {
                        psb.Builder.Append(" <b>Electrical</b>,");
                    }
                    if ((objective.Knowledge & ResearchMaterialKnowledge.Thermal) != 0) {
                        psb.Builder.Append(" <b>Thermal</b>,");
                    }
                    if ((objective.Knowledge & ResearchMaterialKnowledge.Special) != 0) {
                        psb.Builder.Append(" <b>Special</b>,");
                    }
                    psb.Builder.TrimEnd(StringUtils.DefaultCommaChar);
                    psb.Builder.Append(" properties of <b>").Append(material.UnknownDisplayName).Append("<b>");
                    row.Text.SetText(psb.Builder);
                    row.gameObject.SetActive(true);
                }
            }
            for(int i = rowCount; i < Rows.Length; i++) {
                Rows[i].gameObject.SetActive(false);
            }

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

                if (pair.MaterialId != Objectives[i].MaterialId) {
                    continue;
                }

                if ((pair.Knowledge & Objectives[i].Knowledge) == Objectives[i].Knowledge) {
                    Completed.Set(i);

                    Rows[i].Checkbox.SetAlpha(0.5f);
                    Rows[i].CrossOff.enabled = true;
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
            Game.Scenes.QueueOnEnable(this, OnLateEnable);
            return null;
        }
    }
}