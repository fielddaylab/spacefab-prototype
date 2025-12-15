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

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ResetSequence();

            Game.Events.Register(GameEvents.StationCompleted, HandleStationCompleted);
            Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);

            if (CurrSequence.Steps.Count > 0)
            {
                UpdateSequenceDisplay(m_currIndex);
            }
            else
            {
                FinishSequenceDisplay();
            }
        }

        private void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }

            Game.Events.Deregister(GameEvents.StationCompleted, HandleStationCompleted);
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

            LevelMgr.Instance.Evaluate();
        }

        private void ResetSequence()
        {
            m_currIndex = 0;
            CurrSequence.Clear();
            CurrSequence.Copy(ChipFabConfig.Instance.CurrLevel.FabSequence());
            UpdateSequenceDisplay(m_currIndex);
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
            if (m_currIndex >= CurrSequence.Steps.Count) {
                return false;
            }

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

        private void HandleNewWaferCreated()
        {
            ResetSequence();
        }

        #endregion // Handlers
    }
}