using BeauRoutine;
using BeauRoutine.Extensions;
using BeauUtil;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI.Animation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SpaceFab {
    public sealed class TransitionHandler : SharedStateComponent {
        public Canvas Canvas;
        public DissolveTransitionRenderer Renderer;

        private void Awake() {
            Game.Scenes.RegisterTransitionHandlers(UnloadHandler, LoadHandler);
            Game.Scenes.OnMainSceneUnloading.Register(OnMainSceneUnloading);
            Game.Scenes.OnMainSceneReady.Register(OnMainSceneReady);

            if (!GameLoop.IsBooted() && SceneManager.GetActiveScene().buildIndex != 0) {
                Renderer.RandomizeBackground();
                Renderer.SetCutoff(1);
                Canvas.enabled = true;
            } else {
                Canvas.enabled = false;
            }
        }

        private void OnMainSceneUnloading() {
            Game.Input.PauseAll();
        }

        private void OnMainSceneReady() {
            Game.Input.ResumeAll();
        }

        private IEnumerator UnloadHandler(Scene scene, StringHash32 tag, MainSceneTransitionArgs transition) {
            Canvas.enabled = true;
            Renderer.SetCutoff(0);
            Renderer.RandomizeBackground();
            Renderer.RandomizeMasks();
            Renderer.SetInvertedBlend(false);

            yield return Tween.ZeroToOne(Renderer.SetCutoff, 1f);
            yield return 0.05f;
        }

        private IEnumerator LoadHandler(Scene scene, StringHash32 tag, MainSceneTransitionArgs transition) {
            Renderer.SetInvertedBlend(true);
            Renderer.RandomizeMasks();

            yield return Tween.OneToZero(Renderer.SetCutoff, 1f);
            Canvas.enabled = false;
        }
    }
}