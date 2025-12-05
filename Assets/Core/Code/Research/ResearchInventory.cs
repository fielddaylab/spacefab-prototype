using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;

namespace SpaceFab.Research {
    public sealed class ResearchInventory : SharedStateComponent {
        public HashSet<StringHash32> KnownMaterials = SetUtils.Create<StringHash32>(16);
        public Dictionary<StringHash32, ResearchMaterialKnowledge> MaterialKnowledge = MapUtils.Create<StringHash32, ResearchMaterialKnowledge>(16);
        public Dictionary<StringHash32, ResearchMaterialGuessState> MaterialGuesses = MapUtils.Create<StringHash32, ResearchMaterialGuessState>(16);
    }

    public struct ResearchMaterialGuessState {
        public ElectricalTag Electric;
        public DopantType Dopant;

        public ThermalTag? Thermal;
        public SpecialTag? Special;
    }

    [Flags]
    public enum ResearchMaterialKnowledge {
        Electrical = 0x01,
        Thermal = 0x02,
        Special = 0x04,
        Dopant = 0x08,

        All = Electrical | Thermal | Special,
        AllIncludingDopant = Electrical | Thermal | Special | Dopant
    }

    static public partial class ResearchMaterialUtility {
        static public StringHash32 GetRootMaterial(StringHash32 materialId) {
            ResearchMaterial mat = Find.NamedAsset<ResearchMaterial>(materialId);
            while(!mat.Parent.IsEmpty) {
                mat = Find.NamedAsset<ResearchMaterial>(mat.Parent);
            }
            return mat.AssetId;
        }

        static public StringHash32 GetRootMaterial(ResearchMaterial material) {
            while (!material.Parent.IsEmpty) {
                material = Find.NamedAsset<ResearchMaterial>(material.Parent);
            }
            return material.AssetId;
        }

        static public ResearchMaterialKnowledge GetKnownCategories(StringHash32 materialId) {
            Find.State<ResearchInventory>().MaterialKnowledge.TryGetValue(materialId, out var knowledge);
            return knowledge;
        }

        static public void SetKnownCategories(StringHash32 materialId, ResearchMaterialKnowledge knowledge) {
            Find.State<ResearchInventory>().MaterialKnowledge[materialId] = knowledge;
        }

        static public ResearchMaterialGuessState GetGuess(StringHash32 materialId) {
            Find.State<ResearchInventory>().MaterialGuesses.TryGetValue(materialId, out var guess);
            return guess;
        }

        static public void SetGuess(StringHash32 materialId, ResearchMaterialGuessState guess) {
            Find.State<ResearchInventory>().MaterialGuesses[materialId] = guess;
        }

        static public bool ProcessGuess(StringHash32 materialId) {
            ResearchInventory inv = Find.State<ResearchInventory>();
            ResearchMaterial mat = Find.NamedAsset<ResearchMaterial>(materialId);
            inv.MaterialGuesses.TryGetValue(materialId, out var guess);
            inv.MaterialKnowledge.TryGetValue(materialId, out var knowledge);

            bool areAllGuessesCorrect = true;

            if (guess.Electric != ElectricalTag.Unknown) {
                if (guess.Electric == mat.Electrical && guess.Dopant == mat.DopantType) {
                    guess.Electric = default;
                    guess.Dopant = default;
                    knowledge |= ResearchMaterialKnowledge.Electrical;
                } else {
                    areAllGuessesCorrect = false;
                }
            }

            if (guess.Thermal.HasValue) {
                if (guess.Thermal == mat.Thermal) {
                    guess.Thermal = default;
                    knowledge |= ResearchMaterialKnowledge.Thermal;
                } else {
                    areAllGuessesCorrect = false;
                }
            }
            if (guess.Special.HasValue) {
                if (guess.Special == mat.SpecialTags) {
                    guess.Special = default;
                    knowledge |= ResearchMaterialKnowledge.Special;
                } else {
                    areAllGuessesCorrect = false;
                }
            }

            if (areAllGuessesCorrect) {
                inv.MaterialGuesses[materialId] = guess;
                inv.MaterialKnowledge[materialId] = knowledge;
            }

            return areAllGuessesCorrect;
        }
    }
}