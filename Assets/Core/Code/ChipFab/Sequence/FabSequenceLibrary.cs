using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab {
    public class FabSequenceLibrary : MonoBehaviour
    {
        public static FabSequenceLibrary Instance;

        [Header("Chunks")]
        public Sprite NChunk;
        public Sprite PChunk;
        public Sprite MetalChunk;

        [Header("Steps")]
        public Sprite AddStencilOxidePrefab;
        public Sprite AddStencilSputterPrefab;
        public Sprite ApplyResistPrefab;
        public Sprite DrawPatternPrefab;
        public Sprite EtchPatternPrefab;
        public Sprite FillStencilSputterPrefab;
        public Sprite FillstencilDopeNPrefab;
        public Sprite FillstencilDopePPrefab;

        private void Awake()
        {
            Instance = this;
        }
    }

    public static class SequenceUtility
    {
        public static Sprite LookupStep(SequenceStepID id, ChunkID chunkId)
        {
            FabSequenceLibrary library = FabSequenceLibrary.Instance;
            switch (id)
            {
                case SequenceStepID.AddStencil_OXIDE:
                    return library.AddStencilOxidePrefab;
                case SequenceStepID.AddStencil_SPUTTER:
                    return library.AddStencilSputterPrefab;
                case SequenceStepID.ApplyResist:
                    return library.ApplyResistPrefab;
                case SequenceStepID.DrawPattern:
                    return library.DrawPatternPrefab;
                case SequenceStepID.EtchPattern:
                    return library.EtchPatternPrefab;
                case SequenceStepID.FillStencil_SPUTTER:
                    return library.FillStencilSputterPrefab;
                case SequenceStepID.FillStencil_DOPE:
                    if (chunkId == ChunkID.N) { return library.FillstencilDopeNPrefab; }
                    else if (chunkId == ChunkID.P) { return library.FillstencilDopePPrefab; }
                    return null;
                default:
                    return null;
            }
        }

        public static Sprite LookupChunk(ChunkID id)
        {
            FabSequenceLibrary library = FabSequenceLibrary.Instance;
            switch (id)
            {
                case ChunkID.N:
                    return library.NChunk;
                case ChunkID.P:
                    return library.PChunk;
                case ChunkID.Metal:
                    return library.MetalChunk;
                default:
                    return null;
            }
        }
    }
}