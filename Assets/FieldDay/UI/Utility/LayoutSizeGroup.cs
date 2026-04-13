using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay.UI {
#if UNITY_EDITOR
    [ExecuteAlways]
#endif // UNITY_EDITOR
    public sealed class LayoutSizeGroup : MonoBehaviour, ILayoutElement {
        public enum SyncMode : byte {
            Size,
            PreferredSize,
            PreferredSizeUpdateRoot,
        }

        [Flags]
        public enum Dimensions : byte {
            Horizontal = 0x1,
            Vertical = 0x02,

            Both = Horizontal | Vertical
        }
        
        [Required] public RectTransform Root;
        public SyncMode Mode;
        public Dimensions SyncDimensions = Dimensions.Both;
        [ShowIfField("ShouldDisplayUpdateRoot")] public Dimensions UpdateRootDimensions = Dimensions.Both;

        public Vector2 Padding;
        public Vector2 MinSize;
        [Required] public RectTransform[] Children;

        [NonSerialized] private Vector2 m_LastKnownSize;
        [NonSerialized] private Vector2 m_LastPaddedSize;
        [NonSerialized] private bool m_SyncQueued;

        /// <summary>
        /// Returns the last known size.
        /// </summary>
        public Vector2 LastSize {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_LastKnownSize; }
        }

        [ContextMenu("Force Sync")]
        public void Sync() {
            if ((Mode == SyncMode.PreferredSize || Mode == SyncMode.PreferredSizeUpdateRoot) && isActiveAndEnabled) {
                m_SyncQueued = true;
                return;
            }

            Sync(Root, Mode, Padding);
        }

        public void Sync(RectTransform root, SyncMode mode, Vector2 padding) {
            if (!root || SyncDimensions == 0) {
                return;
            }

            float width = m_LastKnownSize.x, height = m_LastKnownSize.y;
            switch (Mode) {
                case SyncMode.Size:
                default: {
                    Vector2 localSize = root.rect.size;
                    if ((SyncDimensions & Dimensions.Horizontal) != 0) {
                        width = localSize.x;
                    }
                    if ((SyncDimensions & Dimensions.Vertical) != 0) {
                        height = localSize.y;
                    }
                    break;
                }
                case SyncMode.PreferredSize:
                case SyncMode.PreferredSizeUpdateRoot: {
                    if ((SyncDimensions & Dimensions.Horizontal) != 0) {
                        width = LayoutUtility.GetPreferredWidth(root);
                    }
                    if ((SyncDimensions & Dimensions.Vertical) != 0) {
                        height = LayoutUtility.GetPreferredHeight(root);
                    }
                    break;
                }
            }

            SetSize(width, height);
        }

        public void SetSize(float width, float height) {
            width = (int)(Math.Max(width, MinSize.x) + 0.999f);
            height = (int)(Math.Max(height, MinSize.y) + 0.999f);

            if (m_LastKnownSize.x != width || m_LastKnownSize.y != height) {
                m_LastKnownSize.Set(width, height);

                bool horizontal = (SyncDimensions & Dimensions.Horizontal) != 0;
                bool vertical = (SyncDimensions & Dimensions.Vertical) != 0;

                if (Root && Mode == SyncMode.PreferredSizeUpdateRoot) {
                    if ((UpdateRootDimensions & Dimensions.Horizontal) != 0) {
                        Root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                    }
                    if ((UpdateRootDimensions & Dimensions.Vertical) != 0) {
                        Root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                    }
                }

                width = (int)(width + Padding.x + 0.999f);
                height = (int)(height + Padding.y + 0.999f);
                m_LastPaddedSize.Set(width, height);

                foreach (var child in Children) {
                    Assert.NotNullOrDestroyed(child, "LayoutSizeGroup sync child is null or destroyed!");

                    if (horizontal) {
                        child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                    }
                    if (vertical) {
                        child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                    }
                }
            }
        }

        private void OnEnable() {
            if (m_SyncQueued) {
                m_SyncQueued = false;
                Sync();
            }
        }

        #region ILayoutElement

        float ILayoutElement.minWidth {
            get { return MinSize.x; }
        }

        float ILayoutElement.preferredWidth {
            get { return m_LastPaddedSize.x; }
        }

        float ILayoutElement.flexibleWidth {
            get { return 0; }
        }

        float ILayoutElement.minHeight {
            get { return MinSize.y; }
        }

        float ILayoutElement.preferredHeight {
            get { return m_LastPaddedSize.y; }
        }

        float ILayoutElement.flexibleHeight {
            get { return 0; }
        }

        int ILayoutElement.layoutPriority {
            get { return 100; }
        }
        void ILayoutElement.CalculateLayoutInputHorizontal() {
        }

        void ILayoutElement.CalculateLayoutInputVertical() {
        }

        #endregion // ILayoutElement

#if UNITY_EDITOR
        private bool ShouldDisplayUpdateRoot() {
            return Mode == SyncMode.PreferredSizeUpdateRoot;
        }

        private void Update() {
            if (Application.IsPlaying(this) || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            m_LastKnownSize = default;
            Sync();
        }
#endif // UNITY_EDITOR
    }
}