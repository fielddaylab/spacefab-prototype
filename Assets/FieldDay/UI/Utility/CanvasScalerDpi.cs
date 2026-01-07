using BeauUtil;
using FieldDay.Rendering;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay.UI {
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class CanvasScalerDpi : MonoBehaviour {
        [Header("Dpi Scaling")]
        [SerializeField] private float m_DefaultScale = 1;
        [SerializeField] private float m_HighScale = 2;
        [SerializeField] private float m_ExtraHighScale = 3;

        [NonSerialized] private CanvasScaler m_Scaler;
        [NonSerialized] private float m_UserScale = 1;

        public float UserScale {
            get { return m_UserScale; }
            set {
                m_UserScale = value;
                if (isActiveAndEnabled) {
                    OnDpiUpdated(Game.Rendering.CurrentDpiType);
                }
            }
        }

        private void OnEnable() {
            this.CacheComponent(ref m_Scaler);
            Game.Rendering.OnScreenDpiChanged.Register(OnDpiUpdated);
            OnDpiUpdated(Game.Rendering.CurrentDpiType);
        }

        private void OnDisable() {
            Game.Rendering?.OnScreenDpiChanged.Deregister(OnDpiUpdated);
        }

        private void OnDpiUpdated(ScreenDpiType dpi) {
            float scale = m_DefaultScale;
            switch(dpi) {
                case ScreenDpiType.High: {
                    scale = m_HighScale;
                    break;
                }
                case ScreenDpiType.ExtraHigh: {
                    scale = m_ExtraHighScale;
                    break;
                }
            }
            m_Scaler.scaleFactor = scale * m_UserScale;
        }
    }
}