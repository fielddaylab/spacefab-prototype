using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum SequenceStepID
    {
        AddStencil_OXIDE,
        AddStencil_SPUTTER,
        ApplyResist,
        DrawPattern,
        EtchPattern,
        FillStencil_SPUTTER,
        FillStencil_DOPE,
    }

    public enum ChunkID
    {
        N,
        P,
        Metal
    }

    [Serializable]
    public struct SequenceStep
    {
        public ChunkID Chunk;
        public SequenceStepID Step;
    }

    [CreateAssetMenu(menuName = "ChipFab/Fab Sequence")]
    public class FabSequence : ScriptableObject
    {
        public List<SequenceStep> Steps;
    }

    public class FabSequenceCopy
    {
        public List<SequenceStep> Steps;

        public void Copy(FabSequence sequence)
        {
            Steps = new List<SequenceStep>();
            foreach (var step in sequence.Steps)
            {
                Steps.Add(step);
            }
        }

        public void Clear()
        {
            if (Steps != null)
            {
                Steps.Clear();
            }
        }
    }
}