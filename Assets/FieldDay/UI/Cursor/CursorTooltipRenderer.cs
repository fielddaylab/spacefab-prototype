using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.HID;
using FieldDay.Localization;
using FieldDay.Rendering;
using FieldDay.UI.Animation;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay.UI {
    public sealed class CursorTooltipRenderer : MonoBehaviour, IOnGuiUpdate {
        private enum Direction {
            Left,
            Right,
            Up,
            Down
        }

        #region Inspector

        [Header("Positioning")]
        [SerializeField] private RectTransform m_Position;
        [SerializeField] private RectTransform m_BoxGroup;
        [SerializeField] private float m_CursorOffset = 16;
        [SerializeField] private float m_ScreenEdgeOffset = 16;

        [Header("Contents")]
        [SerializeField] private LayoutGroup m_Layout;
        [SerializeField] private TMP_Text m_Text;
        [SerializeField] private TMP_Text m_Header;

        [Header("Defaults")]
        [SerializeField] private float m_DefaultHoverDelay = 0.6f;

        #endregion // Inspector

        [NonSerialized] private Camera m_CanvasCamera;
        [NonSerialized] private RectTransform m_ParentRect;

        [NonSerialized] private CursorHint m_LastKnown;
        [NonSerialized] private float m_Cooldown;
        [NonSerialized] private bool m_Visible;
        [NonSerialized] private long m_LastVersionKey;

        private void Awake() {
            m_ParentRect = (RectTransform) m_Position.parent;
            CanvasHelper.TryGetCamera(m_Position.GetCanvas().rootCanvas, out m_CanvasCamera);
            m_Position.anchorMin = m_Position.anchorMax = new Vector2(0.5f, 0.5f);
        }

        private void OnEnable() {
            Game.Gui.RegisterUpdate(this);
        }

        private void OnDisable() {
            m_LastKnown = null;
            m_Cooldown = 0;
            m_Visible = false;
            m_LastVersionKey = 0;
            m_BoxGroup.gameObject.SetActive(false);
            Game.Gui?.DeregisterUpdate(this);
        }

        private void UpdateCursorPositionFromScreenCursor(Vector2 screenPosition) {
            bool found = RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRect, screenPosition, m_CanvasCamera, out Vector2 local);
            if (found) {
                UpdateCursorPositionFromLocalPosition(local);
            }
        }

        private void UpdateCursorPositionFromLocalPosition(Vector2 localPosition) {
            Direction direction = FindBestDirection(ref localPosition);

            Vector2 offset = default;
            Vector2 pivot = default;
            switch(direction) {
                case Direction.Left: {
                    pivot = new Vector2(1, 0.5f);
                    offset.x = -m_CursorOffset;
                    break;
                }

                case Direction.Right: {
                    pivot = new Vector2(0, 0.5f);
                    offset.x = m_CursorOffset;
                    break;
                }

                case Direction.Up: {
                    pivot = new Vector2(0.5f, 0);
                    offset.y = m_CursorOffset;
                    break;
                }

                case Direction.Down: {
                    pivot = new Vector2(0.5f, 1);
                    offset.y = -m_CursorOffset;
                    break;
                }
            }

            m_BoxGroup.anchoredPosition = offset;
            m_BoxGroup.pivot = pivot;

            localPosition.x = Mathf.Round(localPosition.x);
            localPosition.y = Mathf.Round(localPosition.y);
            m_Position.localPosition = localPosition;
        }

        private Direction FindBestDirection(ref Vector2 localPosition) {
            Vector2 boundsSize = m_ParentRect.rect.size / 2;
            Vector2 boxSize = m_BoxGroup.sizeDelta / 2;

            boundsSize.x -= m_ScreenEdgeOffset;
            boundsSize.y -= m_ScreenEdgeOffset;

            localPosition.x = Math.Clamp(localPosition.x, -boundsSize.x, boundsSize.x);
            localPosition.y = Math.Clamp(localPosition.y, -boundsSize.y, boundsSize.y);

            if (localPosition.x - boxSize.x >= -boundsSize.x
                && localPosition.x + boxSize.x <= boundsSize.x) {
                return localPosition.y > 0 ? Direction.Down : Direction.Up;
            }

            boundsSize.y -= boxSize.y;
            localPosition.y = Math.Clamp(localPosition.y, -boundsSize.y, boundsSize.y);

            return localPosition.x < 0 ? Direction.Right : Direction.Left;
        }

        private void Clear() {
            if (m_Visible) {
                m_BoxGroup.gameObject.SetActive(false);
                m_Visible = false;
            }
        }

        private void Display() {
            if (!m_Visible) {
                m_BoxGroup.gameObject.SetActive(true);
                m_Visible = true;
            }
        }

        private void UpdateContents(in CursorTooltipContents contents) {
            bool headerActive = false;
            if (headerActive = (contents.DynamicHeader != null && contents.DynamicHeader.Length > 0)) {
                m_Header.SetText(contents.DynamicHeader);
            } else if (headerActive = !string.IsNullOrEmpty(contents.Header)) {
                m_Header.SetText(contents.Header);
            } else if (headerActive = !contents.LocHeader.IsEmpty) {
                // TODO: handle localization key
            }
            
            m_Header.gameObject.SetActive(headerActive);

            bool textActive = false;
            if (textActive = (contents.DynamicContents != null && contents.DynamicContents.Length > 0)) {
                m_Text.SetText(contents.DynamicContents);
            } else if (textActive = !string.IsNullOrEmpty(contents.Contents)) {
                m_Text.SetText(contents.Contents);
            } else if (textActive = !contents.LocContents.IsEmpty) {
                // TODO: handle localization key
            }

            m_Text.gameObject.SetActive(textActive);
        }

        void IOnGuiUpdate.OnGuiUpdate() {
            // TODO: don't show if localization is loading

            CursorHint currentHint = CursorHint.Current;
            if (!CursorHint.HasTooltip(currentHint)) {
                currentHint = null;
            }

            bool hasHint = currentHint;
            CursorTooltipContents tooltipContents;

            if (m_LastKnown != currentHint) {
                m_LastKnown = currentHint;
                if (!currentHint) {
                    Clear();
                    m_Cooldown = 0;
                } else {
                    if (!m_Visible) {
                        float cooldown = m_DefaultHoverDelay;
                        if ((currentHint.Flags & CursorHint.BehaviorFlags.ExtendedTooltipDelay) != 0) {
                            cooldown *= 2;
                        }
                        m_Cooldown = Math.Max(m_Cooldown, cooldown);
                    } else {
                        m_LastVersionKey = currentHint.LastUpdatedTimestamp;
                        CursorHint.GetTooltipContents(currentHint, out tooltipContents);
                        UpdateContents(tooltipContents);
                        m_Layout.ForceRebuild();
                    }
                }
            }

            if (hasHint) {
                if (!m_Visible) {
                    m_Cooldown -= Frame.UnscaledDeltaTime;
                    if (m_Cooldown <= 0) {
                        m_LastVersionKey = currentHint.LastUpdatedTimestamp;
                        CursorHint.GetTooltipContents(currentHint, out tooltipContents);
                        UpdateContents(tooltipContents);
                        Display();
                        m_Layout.ForceRebuild();
                    }
                }
            }

            if (hasHint && m_Visible) {
                if (m_LastVersionKey != currentHint.LastUpdatedTimestamp) {
                    m_LastVersionKey = currentHint.LastUpdatedTimestamp;
                    CursorHint.GetTooltipContents(currentHint, out tooltipContents);
                    UpdateContents(tooltipContents);
                    m_Layout.ForceRebuild();
                }
                UpdateCursorPositionFromScreenCursor(Input.mousePosition);
            }
        }
    }
}