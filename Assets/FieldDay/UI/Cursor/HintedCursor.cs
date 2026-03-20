using System;
using BeauUtil;
using FieldDay.Assets;
using FieldDay.Debugging;
using FieldDay.HID;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay.UI {
    public class HintedCursor : MonoBehaviour, IOnGuiUpdate {
        public enum VisiblityMode {
            Invisible,
            Interactable,
            Always,
        }

        #region Inspector

        [Header("Components")]
        [SerializeField, Required] private RectTransform m_Position;
        [SerializeField, Required] private Image m_Sprite;

        [Header("Configuration")]
        [SerializeField] private Sprite m_DefaultHoverSprite;
        [SerializeField] private float m_DefaultHeldScale = 0.75f;

        #endregion // Inspector

        [NonSerialized] private Sprite m_DefaultSprite;
        [NonSerialized] private Sprite m_CurrentSprite;
        [NonSerialized] private Vector2 m_DefaultSpriteSize;
        [NonSerialized] private Vector2 m_OriginalSizeDelta;

        private void Awake() {
            m_DefaultSprite = m_Sprite.sprite;
            m_DefaultSpriteSize = m_DefaultSprite.rect.size;
            m_OriginalSizeDelta = m_Position.sizeDelta;
        }

        private void OnEnable() {
            Game.Gui.RegisterUpdate(this);
            CursorUtility.HideCursor();
        }

        private void OnDisable() {
            Game.Gui?.DeregisterUpdate(this);
            CursorUtility.ShowCursor();
        }

        void IOnGuiUpdate.OnGuiUpdate() {
            bool cursorIsFocused = GameLoop.IsFocused() && CursorUtility.IsCursorWithinGameWindow();
#if UNITY_EDITOR
            if (!cursorIsFocused) {
                CursorUtility.ShowCursor();
            } else {
                CursorUtility.HideCursor();
            }
#elif UNITY_WEBGL // necessary in case htmlcanvas is not fully initialized yet
            if (Time.frameCount < 5) {
                CursorUtility.HideCursor();
            }
#endif // UNITY_EDITOR

            m_Position.position = Input.mousePosition;
            
            // retrieve state

            CursorHint hint = CursorHint.Current;
            bool isButtonHeld = Game.Input.IsMouseDown(MouseButton.Left);
            
            bool hintIsInteractable;
            bool hintIsLocked;
            CursorType type;

            if (hint) {
                hintIsInteractable = hint.IsInteractable();
                hintIsLocked = CursorHint.IsLocked(hint);
                type = hint.CursorType.IsEmpty ? null : Find.NamedAsset<CursorType>(hint.CursorType);
            } else {
                hintIsInteractable = hintIsLocked = false;
                type = null;
            }

            // determine output

            Sprite defaultSprite = m_DefaultSprite;
            Sprite hoverSprite = m_DefaultHoverSprite;

            if (!hintIsInteractable) {
                StringHash32 overrideType = CursorHint.DefaultCursor;
                if (!overrideType.IsEmpty) {
                    type = Find.NamedAsset<CursorType>(overrideType);
                    defaultSprite = type.DefaultImage;
                }
            }

            bool showCursor = DebugFlags.IsConsoleOpen || (s_Visibility != VisiblityMode.Invisible && cursorIsFocused && (s_Visibility == VisiblityMode.Always || hintIsInteractable));
            m_Sprite.enabled = showCursor;

            if (!showCursor) {
                return;
            }

            Sprite icon = defaultSprite;
            bool scaleDown = isButtonHeld;

            if (hintIsInteractable) {
                if (type == null) {
                    icon = hoverSprite;
                } else {
                    if ((isButtonHeld || hintIsLocked) && type.HeldImage != null) {
                        scaleDown = type.HeldScaleOverride > 0;
                        icon = type.HeldImage;
                    } else {
                        icon = type.DefaultImage;
                    }
                }
            }

            // Final assignments

            if (m_CurrentSprite != icon) {
                m_Sprite.sprite = icon;
                m_CurrentSprite = icon;
                Vector2 pivot = icon.pivot;
                Vector2 size = icon.rect.size;
                pivot.x /= size.x;
                pivot.y /= size.y;
                m_Position.pivot = pivot;

                Vector2 newSize;
                newSize.x = m_OriginalSizeDelta.x * size.x / m_DefaultSpriteSize.x;
                newSize.y = m_OriginalSizeDelta.y * size.y / m_DefaultSpriteSize.y;
                m_Position.sizeDelta = newSize;
            }

            float scale;
            if (type) {
                if (scaleDown) {
                    scale = type.HeldScaleOverride > 0 ? type.HeldScaleOverride : m_DefaultHeldScale * type.DefaultScale;
                } else {
                    scale = type.DefaultScale;
                }
            } else {
                scale = scaleDown ? m_DefaultHeldScale : 1;
            }

            m_Position.localScale = new Vector3(scale, scale, scale);
        }

        #region Statics

        static private VisiblityMode s_Visibility = VisiblityMode.Always;

        /// <summary>
        /// When the HintedCursor should be displayed.
        /// </summary>
        static public VisiblityMode Visibility {
            get { return s_Visibility; }
            set { s_Visibility = value; }
        }

        #endregion // Statics
    }
}