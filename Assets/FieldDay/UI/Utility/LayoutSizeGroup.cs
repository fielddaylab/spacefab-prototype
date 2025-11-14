using BeauUtil;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay.UI {
#if UNITY_EDITOR
    [ExecuteAlways]
#endif // UNITY_EDITOR
    public sealed class LayoutSizeGroup : MonoBehaviour {
        public enum SyncMode {
            Size,
            PreferredSize
        }
        
        [Required] public RectTransform Root;
        public SyncMode Mode;
        public Vector2 Padding;

        [Required] public RectTransform[] Children;

        [NonSerialized] private Vector2 m_LastKnownSize;

        public void Sync() {
            Sync(Root, Mode, Padding);
        }

        public void Sync(RectTransform root, SyncMode mode, Vector2 padding) {
            if (!root) {
                return;
            }

            float width, height;
            switch (Mode) {
                case SyncMode.Size:
                default: {
                    Vector2 localSize = root.rect.size;
                    width = localSize.x;
                    height = localSize.y;
                    break;
                }
                case SyncMode.PreferredSize: {
                    width = LayoutUtility.GetPreferredWidth(root);
                    height = LayoutUtility.GetPreferredHeight(root);
                    break;
                }
            }

            width += Padding.x;
            height += Padding.y;

            SetSize(new Vector2(width, height));
        }

        public void SetSize(Vector2 size) {
            if (m_LastKnownSize != size) {
                m_LastKnownSize = size;

                foreach (var child in Children) {
                    if (!child) {
                        continue;
                    }

                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                }
            }
        }

#if UNITY_EDITOR
        private void LateUpdate() {
            if (Application.IsPlaying(this))
                return;

            Sync();
        }
#endif // UNITY_EDITOR
    }
}