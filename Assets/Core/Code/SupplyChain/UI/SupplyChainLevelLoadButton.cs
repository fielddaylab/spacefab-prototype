using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Assets;
using FieldDay.Scenes;
using FieldDay.UI;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class SupplyChainLevelLoadButton : MonoBehaviour {
        public SceneReference Scene;
        [AssetName(typeof(SupplyChainLevel))] public StringHash32 LevelId;
        public CursorHint Click;

        private void Awake() {
            Click.onClick.AddListener(() => {
                Game.Scenes.LoadMainScene(Scene);

                SceneRequestContext context = default;
                context.Task = LevelId;
                Game.Scenes.QueueMainLoadContext(context);
            });
        }
    }
}