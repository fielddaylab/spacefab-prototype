using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.UI;
using UnityEngine;

namespace SpaceFab {
    public sealed class SceneLoadButton : MonoBehaviour {
        public CursorHint Click;
        public SceneReference Scene;

        private void Awake() {
            Click.onClick.AddListener(() => {
                Game.Scenes.LoadMainScene(Scene);
            });
        }
    }
}