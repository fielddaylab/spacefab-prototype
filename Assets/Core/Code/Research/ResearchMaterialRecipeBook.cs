using System;
using System.Collections.Generic;
using BeauUtil;
using FieldDay.Assets;
using UnityEngine;

namespace SpaceFab.Research {
    [CreateAssetMenu(menuName = "Research/Recipe Book")]
    public sealed class ResearchMaterialRecipeBook : GlobalAsset {
        [Serializable]
        private struct Entry {
            [AssetName(typeof(ResearchMaterial))] public StringHash32 InputA;
            [AssetName(typeof(ResearchMaterial))] public StringHash32 InputB;
            [Space]
            [AssetName(typeof(ResearchMaterial))] public StringHash32 Output;
        }

        [SerializeField] private Entry[] m_Entries;

        [NonSerialized] private Dictionary<ulong, StringHash32> m_RecipeMap;

        public override void Mount() {
            if (m_RecipeMap == null) {
                m_RecipeMap = new Dictionary<ulong, StringHash32>(m_Entries.Length);
                foreach(var entry in m_Entries) {
                    m_RecipeMap.Add(GetKey(entry.InputA, entry.InputB), entry.Output);
                }
            }
        }

        static private ulong GetKey(StringHash32 inputA, StringHash32 inputB) {
            return inputA.HashValue > inputB.HashValue
                ? ((ulong)inputB.HashValue << 32) | inputA.HashValue
                : ((ulong)inputA.HashValue << 32) | inputB.HashValue;
        }

        public bool TryGetResult(StringHash32 inputA, StringHash32 inputB, out StringHash32 output) {
            return m_RecipeMap.TryGetValue(GetKey(inputA, inputB), out output);
        }
    }
}