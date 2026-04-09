using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Debugging;
using FieldDay.Scenes;
using FieldDay.Scripting;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    [PreloadOrder(100)]
    public sealed class ResearchGame : SceneController {
        [AssetName(typeof(ResearchMaterial))] public StringHash32[] Materials;
        public ResearchToolsMask Unlocks;
        public ResearchChipId[] AvailableProperties;
        public ResearchChipId StartingHypothesis;
        
        [Header("-- DEBUG -- ")]
        [SerializeField, AssetName(typeof(ResearchLevel))] private StringHash32 m_DEBUGLevel;

        static public ResearchLevel CurrentLevel { get; private set; }

        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            ResearchInventory inventory = Find.State<ResearchInventory>();
            
            Game.Scenes.GetLoadContext(out SceneRequestContext context);
            StringHash32 levelName = context.Task.Name;
            if (DebugFlags.LaunchedFromThisScene) {
                levelName = StringHash32.First(levelName, m_DEBUGLevel);
            }

            if (!levelName.IsEmpty) {
                CurrentLevel = Find.NamedAsset<ResearchLevel>(levelName);
                Materials = CurrentLevel.AvailableMaterials;
                Unlocks = CurrentLevel.AvailableTools;
                AvailableProperties = CurrentLevel.AvailableProperties;
                StartingHypothesis = CurrentLevel.StartingHypothesis;

                foreach(var prepopulate in CurrentLevel.PrePopulate) {
                    //ResearchMaterialKnowledge knowledge = prepopulate.Chip;
                    //if ((knowledge & ResearchMaterialKnowledge.AllBasic) == ResearchMaterialKnowledge.AllBasic) {
                    //    knowledge |= ResearchMaterialKnowledge.Name;
                    //}
                    //inventory.MaterialKnowledge.Add(prepopulate.MaterialId, knowledge);
                }
            }

            ResearchHypothesisPanel hypothesisModule = Find.GuiModule<ResearchHypothesisPanel>();
            hypothesisModule.AvailableProperties = AvailableProperties;
            hypothesisModule.PropertyIndex = Array.IndexOf(AvailableProperties, StartingHypothesis);
            
            foreach(var material in Materials) {
                ResearchMaterialUtility.SpawnNewTrayItem(Find.NamedAsset<ResearchMaterial>(material));
                inventory.KnownMaterials.Add(material);
            }
            ResearchMaterialUtility.ArrangeTrayItems();
            yield return null;

            ResearchToolUtility.SetUnlocks(Unlocks);
        }

        protected override void OnSceneReady() {
            ScriptUtility.Trigger("SceneReady");
        }

        protected override void OnSceneUnload() {
            CurrentLevel = null;
        }
    }
}