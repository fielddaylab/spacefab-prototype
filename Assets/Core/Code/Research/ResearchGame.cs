using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
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

        static public ResearchLevel CurrentLevel { get; private set; }

        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            ResearchInventory inventory = Find.State<ResearchInventory>();
            
            Game.Scenes.GetLoadContext(out SceneRequestContext context);
            StringHash32 levelName = context.Task.Name;
            if (!levelName.IsEmpty) {
                CurrentLevel = Find.NamedAsset<ResearchLevel>(levelName);
                Materials = CurrentLevel.AvailableMaterials;
                Unlocks = CurrentLevel.AvailableTools;

                foreach(var prepopulate in CurrentLevel.PrePopulate) {
                    inventory.MaterialKnowledge.Add(prepopulate.MaterialId, prepopulate.Knowledge);
                }
            }

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