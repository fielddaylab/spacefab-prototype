using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Scenes;
using FieldDay.Scripting;
using FieldDay.SharedState;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    [PreloadOrder(100)]
    public sealed class ResearchGame : SceneController {
        [AssetName(typeof(ResearchMaterial))] public StringHash32[] Materials;
        public ResearchToolsMask Unlocks;

        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            ResearchInventory inventory = Find.State<ResearchInventory>();
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
    }
}