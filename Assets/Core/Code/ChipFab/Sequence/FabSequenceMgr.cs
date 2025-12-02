using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class FabSequenceMgr : MonoBehaviour
    {
        public static FabSequenceMgr Instance;

        public FabSequenceCopy CurrSequence = new FabSequenceCopy();
        private int m_currIndex;

        [Header("Display")]
        public SpriteRenderer ChunkRenderer;
        public SpriteRenderer StepRenderer;

        [Header("Results")]
        public GameObject ResultsPanel;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            m_currIndex = 0;
            CurrSequence.Copy(ChipFabConfig.Instance.CurrLevel.FabSequence());

            Game.Events.Register(GameEvents.StationCompleted, HandleStationCompleted);
            HideResultsPanel();

            if (CurrSequence.Steps.Count > 0)
            {
                UpdateSequenceDisplay(m_currIndex);
            }
            else
            {
                FinishSequenceDisplay();
            }
        }

        private void UpdateSequenceDisplay(int stepIndex)
        {
            Sprite chunkSprite = SequenceUtility.LookupChunk(CurrSequence.Steps[stepIndex].Chunk);
            Sprite stepSprite = SequenceUtility.LookupStep(CurrSequence.Steps[stepIndex].Step);

            ChunkRenderer.sprite = chunkSprite;
            StepRenderer.sprite = stepSprite;
        }

        private void FinishSequenceDisplay()
        {
            ChunkRenderer.sprite = null;
            StepRenderer.sprite = null;

            ShowResultsPanel();
        }

        private void HideResultsPanel()
        {
            ResultsPanel.SetActive(false);
        }

        private void ShowResultsPanel()
        {
            ResultsPanel.SetActive(true);
        }

        public SequenceStepID CurrStepID()
        {
            return CurrSequence.Steps[m_currIndex].Step;
        }

        public ChunkID CurrChunkID()
        {
            return CurrSequence.Steps[m_currIndex].Chunk;
        }

        public bool IsCurrStepAmong(List<SequenceStepID> ids)
        {
            var currStep = CurrSequence.Steps[m_currIndex].Step;
            foreach (var id in ids)
            {
                if (id == currStep) { return true; }
            }

            return false;
        }

        #region Handlers

        private void HandleStationCompleted()
        {
            m_currIndex++;
            if (m_currIndex < CurrSequence.Steps.Count)
            {
                UpdateSequenceDisplay(m_currIndex);
            }
            else
            {
                FinishSequenceDisplay();
            }
        }

        #endregion // Handlers
    }
}