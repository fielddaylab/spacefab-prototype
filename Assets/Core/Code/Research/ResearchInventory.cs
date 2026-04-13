using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;

namespace SpaceFab.Research {
    public sealed class ResearchInventory : SharedStateComponent {
        public Dictionary<StringHash32, ResearchMaterialKnowledge> MaterialKnowledge = MapUtils.Create<StringHash32, ResearchMaterialKnowledge>(16);
        public Dictionary<StringHash32, ResearchObservationList> MaterialObservations = MapUtils.Create<StringHash32, ResearchObservationList>(16);
    }

    public struct ResearchObservationList {
        public const int MaxObservations = 10;

        public ushort Count;
        private unsafe fixed byte m_Buffer[MaxObservations];

        public unsafe ResearchChipId this[int index] {
            get {
                Assert.True(index >= 0 && index < Count);
                return (ResearchChipId) m_Buffer[index];
            }
        }

        public unsafe bool Has(ResearchChipId chipId) {
            for (int i = Count; i-- > 0;) {
                if (m_Buffer[i] == (byte)chipId) {
                    return true;
                }
            }

            return false;
        }

        public unsafe bool TryAdd(ResearchChipId chipId, out ResearchChipId replaced) {
            Assert.True(!ResearchChipUtility.IsProperty(chipId), "Chip {0} is not observation", chipId);

            for (int i = Count; i-- > 0;) {
                if (m_Buffer[i] == (byte) chipId) {
                    replaced = default;
                    return false;
                }
            }

            ResearchChipCategory category = ResearchChipUtility.Category(chipId);
            if (ResearchChipUtility.CategoryIsExclusive(category)) {
                for (int i = Count; i-- > 0;) {
                    if (ResearchChipUtility.Category((ResearchChipId) m_Buffer[i]) == category) {
                        replaced = (ResearchChipId) m_Buffer[i];
                        m_Buffer[i] = (byte)chipId;
                        return true;
                    }
                }
            }

            Assert.True(Count < MaxObservations, "Observation list has run out of room");
            m_Buffer[Count++] = (byte) chipId;
            replaced = ResearchChipId.None;
            return true;
        }

        public unsafe bool Remove(ResearchChipId chipId) {
            for(int i = Count; i-- > 0;) {
                if (m_Buffer[i] == (byte) chipId) {
                    for(int j = i; j < Count - 1; j++) {
                        m_Buffer[j] = m_Buffer[j + 1];
                    }
                    Count--;
                    return true;
                }
            }

            return false;
        }

        public unsafe int RemoveChipsWithContext(ResearchChipId* removedList) {
            int removed = 0;
            for(int i = Count; i-- > 0;) {
                if (ResearchChipUtility.RequiresContext((ResearchChipId)m_Buffer[i])) {
                    if (removedList != null) {
                        *removedList++ = (ResearchChipId) m_Buffer[i];
                    }
                    for(int j = i + 1; j < Count; j++) {
                        m_Buffer[j - 1] = m_Buffer[j];
                    }
                    Count--;
                    removed++;
                }
            }
            return removed;
        }

        public void Clear() {
            Count = 0;
        }
    }

    public struct ResearchMaterialKnowledge {
        public const int MaxProperties = 6;

        public byte Count;
        private unsafe fixed byte m_IdBuffer[MaxProperties];
        private unsafe fixed uint m_ContextBuffer[MaxProperties];

        public unsafe ResearchChipId Chip(int index) {
            Assert.True(index >= 0 && index < Count);
            return (ResearchChipId)m_IdBuffer[index];
        }

        public unsafe StringHash32 Context(int index) {
            Assert.True(index >= 0 && index < Count);
            return new StringHash32(m_ContextBuffer[index]);
        }

        public unsafe bool Has(ResearchChipId chipId) {
            for (int i = Count; i-- > 0;) {
                if (m_IdBuffer[i] == (byte) chipId) {
                    return true;
                }
            }

            return false;
        }

        public unsafe bool HasCategory(ResearchChipCategory categoryId) {
            for (int i = Count; i-- > 0;) {
                if (ResearchChipUtility.Category((ResearchChipId) m_IdBuffer[i]) == categoryId) {
                    return true;
                }
            }

            return false;
        }

        public unsafe bool TryAdd(ResearchChipId chipId, StringHash32 context) {
            ResearchChipCategory category = ResearchChipUtility.Category(chipId);

            Assert.True(ResearchChipUtility.IsProperty(category), "Chip {0} is not property", chipId);
            Assert.True(!ResearchChipUtility.CategoryRequiresContext(category) || !context.IsEmpty, "Context not set appropriately");

            for (int i = Count; i-- > 0;) {
                if (m_IdBuffer[i] == (byte) chipId && m_ContextBuffer[i] == context.HashValue) {
                    return false;
                }
            }

            if (ResearchChipUtility.CategoryIsExclusive(category)) {
                for (int i = Count; i-- > 0;) {
                    if (ResearchChipUtility.Category((ResearchChipId) m_IdBuffer[i]) == category
                        && m_ContextBuffer[i] == context.HashValue) {
                        return false;
                    }
                }
            }

            Assert.True(Count < MaxProperties, "Property list has run out of room");
            m_IdBuffer[Count] = (byte)chipId;
            m_ContextBuffer[Count++] = context.HashValue;
            return true;
        }

        public void Clear() {
            Count = 0;
        }
    }

    static public partial class ResearchMaterialUtility {
        static public readonly StringHash32 Event_KnowledgeUpdated = "Research::MaterialKnowledgeUpdated";
        static public readonly StringHash32 Event_GoalHintRequested = "Research::GoalHintRequested";

        static public ResearchMaterialKnowledge GetKnownProperties(StringHash32 materialId) {
            Find.State<ResearchInventory>().MaterialKnowledge.TryGetValue(materialId, out var knowledge);
            return knowledge;
        }

        static public BitSet32 GetLockedCategoryMask(ResearchMaterialKnowledge knowledge, StringHash32 context) {
            BitSet32 categoryMask = default;
            for(int i = 0; i < knowledge.Count; i++) {
                if (context != knowledge.Context(i)) {
                    continue;
                }

                ResearchChipMetadata meta = ResearchChipUtility.Metadata(knowledge.Chip(i));
                categoryMask.Set((int) meta.Category);

                if (meta.Category == ResearchChipCategory.PropertyDopantN || meta.Category == ResearchChipCategory.PropertyDopantP) {
                    categoryMask.Set((int) ResearchChipCategory.Diode);
                    categoryMask.Set((int) ResearchChipCategory.Valence);
                    categoryMask.Set((int) ResearchChipCategory.DopingConductivity);
                    categoryMask.Set((int) ResearchChipCategory.Radius);
                }

                categoryMask.Set((int) ResearchChipUtility.Category(meta.DependencyA));
                if (meta.DependencyB != ResearchChipId.None) {
                    categoryMask.Set((int) ResearchChipUtility.Category(meta.DependencyB));
                }
            }

            return categoryMask;
        }

        static public bool IsNameKnown(StringHash32 materialId) {
            // Find.State<ResearchInventory>().MaterialKnowledge.TryGetValue(materialId, out var knowledge);
            // return knowledge.KnownProperties.IsSet(ResearchMaterialKnowledge.Bit_KnownsName);
            return false;
        }

        static public ResearchObservationList GetObservations(StringHash32 materialId) {
            Find.State<ResearchInventory>().MaterialObservations.TryGetValue(materialId, out var guess);
            return guess;
        }

        static public void SetObservations(StringHash32 materialId, ResearchObservationList guess) {
            Find.State<ResearchInventory>().MaterialObservations[materialId] = guess;
        }
    }
}