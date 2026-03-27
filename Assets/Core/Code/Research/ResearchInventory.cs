using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;

namespace SpaceFab.Research {
    public sealed class ResearchInventory : SharedStateComponent {
        public HashSet<StringHash32> KnownMaterials = SetUtils.Create<StringHash32>(16);
        public Dictionary<StringHash32, ResearchMaterialKnowledge> MaterialKnowledge = MapUtils.Create<StringHash32, ResearchMaterialKnowledge>(16);
        public Dictionary<StringHash32, ResearchObservationList> MaterialGuesses = MapUtils.Create<StringHash32, ResearchObservationList>(16);
    }

    public struct ResearchObservationList {
        public const int MaxObservations = 8;

        public int Count;
        public StringHash32 Context;
        private unsafe fixed byte m_Buffer[MaxObservations];

        public unsafe ResearchChipId this[int index] {
            get {
                Assert.True(index >= 0 && index < Count);
                return (ResearchChipId) m_Buffer[index];
            }
            set {
                Assert.True(index >= 0 && index < Count);
                m_Buffer[index] = (byte) value;
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

            Assert.False(Count < MaxObservations, "Observation list has run out of room");
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

        public void Clear() {
            Count = 0;
            Context = default;
        }
    }

    public struct ResearchMaterialKnowledge {
        public BitSet32 KnownProperties;

        public const int Bit_KnownsName = 31;
    }

    static public partial class ResearchMaterialUtility {
        static public readonly StringHash32 Event_KnowledgeUpdated = "Research::MaterialKnowledgeUpdated";
        static public readonly StringHash32 Event_GoalHintRequested = "Research::GoalHintRequested";

        static public StringHash32 GetRootMaterial(ResearchMaterial material) {
            return material.AssetId;
        }

        static public ResearchMaterialKnowledge GetKnownCategories(StringHash32 materialId) {
            Find.State<ResearchInventory>().MaterialKnowledge.TryGetValue(materialId, out var knowledge);
            return knowledge;
        }

        static public bool IsNameKnown(StringHash32 materialId) {
            Find.State<ResearchInventory>().MaterialKnowledge.TryGetValue(materialId, out var knowledge);
            return knowledge.KnownProperties.IsSet(ResearchMaterialKnowledge.Bit_KnownsName);
        }

        static public void SetKnownCategories(StringHash32 materialId, ResearchMaterialKnowledge knowledge) {
            Find.State<ResearchInventory>().MaterialKnowledge[materialId] = knowledge;
        }

        static public ResearchObservationList GetObservations(StringHash32 materialId) {
            Find.State<ResearchInventory>().MaterialGuesses.TryGetValue(materialId, out var guess);
            return guess;
        }

        static public void SetObservations(StringHash32 materialId, ResearchObservationList guess) {
            Find.State<ResearchInventory>().MaterialGuesses[materialId] = guess;
        }

        static public bool AddKnowledgeFlag(StringHash32 materialId, ResearchMaterialKnowledge knowledge) {
            ResearchInventory inv = Find.State<ResearchInventory>();
            inv.MaterialKnowledge.TryGetValue(materialId, out var alreadyKnown);
            if ((alreadyKnown & knowledge) == knowledge) {
                return false;
            }

            knowledge |= alreadyKnown;

            if ((knowledge & ResearchMaterialKnowledge.Name) == 0) {
                if ((knowledge & ResearchMaterialKnowledge.AllBasic) == ResearchMaterialKnowledge.AllBasic) {
                    knowledge |= ResearchMaterialKnowledge.Name;
                }
            }

            inv.MaterialKnowledge[materialId] = knowledge;

            ResearchMaterialKnowledgePair pair = new ResearchMaterialKnowledgePair() {
                MaterialId = materialId,
                Knowledge = knowledge
            };
            SpaceFabGame.Events.Queue(Event_KnowledgeUpdated, EvtArgs.Create(pair));
            return true;
        }

        static public ResearchMaterialGuessResult ProcessGuess(StringHash32 materialId) {
            ResearchInventory inv = Find.State<ResearchInventory>();
            ResearchMaterial mat = Find.NamedAsset<ResearchMaterial>(materialId);
            inv.MaterialGuesses.TryGetValue(materialId, out var guess);
            inv.MaterialKnowledge.TryGetValue(materialId, out var knowledge);

            ResearchMaterialKnowledge originalKnowledge = knowledge;
            ResearchMaterialKnowledge submittedKnowledge = default;

            if (guess.Electric != ElectricalTag.Unknown) {
                submittedKnowledge |= ResearchMaterialKnowledge.Electrical;
                if (guess.Electric == mat.Electrical) {
                    guess.Electric = default;
                    knowledge |= ResearchMaterialKnowledge.Electrical;
                }
            }

            if (guess.Dopant.HasValue) {
                submittedKnowledge |= ResearchMaterialKnowledge.Dopant;
                if (guess.Dopant == mat.DopantType) {
                    guess.Dopant = default;
                    knowledge |= ResearchMaterialKnowledge.Dopant;
                }
            }

            if (guess.Thermal.HasValue) {
                submittedKnowledge |= ResearchMaterialKnowledge.Thermal;
                if (guess.Thermal == mat.Thermal) {
                    guess.Thermal = default;
                    knowledge |= ResearchMaterialKnowledge.Thermal;
                }
            }

            if (guess.Special.HasValue) {
                submittedKnowledge |= ResearchMaterialKnowledge.Special;
                if (guess.Special == mat.SpecialTags) {
                    guess.Special = default;
                    knowledge |= ResearchMaterialKnowledge.Special;
                }
            }

            if ((knowledge & ResearchMaterialKnowledge.Name) == 0) {
                if ((knowledge & ResearchMaterialKnowledge.AllBasic) == ResearchMaterialKnowledge.AllBasic) {
                    knowledge |= ResearchMaterialKnowledge.Name;
                }
            }

            inv.MaterialGuesses[materialId] = guess;
            inv.MaterialKnowledge[materialId] = knowledge;

            if (knowledge != originalKnowledge) {
                ResearchMaterialKnowledgePair pair = new ResearchMaterialKnowledgePair() {
                    MaterialId = materialId,
                    Knowledge = knowledge
                };
                SpaceFabGame.Events.Queue(Event_KnowledgeUpdated, EvtArgs.Create(pair));
            }

            ResearchMaterialGuessResult result;
            result.Correct = knowledge ^ originalKnowledge;
            result.Incorrect = submittedKnowledge ^ result.Correct;
            return result;
        }
    }

    public struct ResearchMaterialGuessResult {
        public ResearchMaterialKnowledge Correct;
        public ResearchMaterialKnowledge Incorrect;
    }
}