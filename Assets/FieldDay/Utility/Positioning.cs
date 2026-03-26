using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Collections;
using ScriptableBake;
using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay {
    [Il2CppEagerStaticClassConstruction]
    static public class Positioning {

        #region Anchors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private float GetAnchorX(TextAnchor anchor) {
            return ((int)anchor % 3) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private float GetAnchorY(TextAnchor anchor) {
            return (2 - (int)anchor / 3) * 0.5f;
        }

        /// <summary>
        /// Sets the anchor for the given RectTransform on the x-axis.
        /// </summary>
        static public void SetAnchorX(RectTransform rect, float anchorX) {
            Vector2 min, max;
            min = rect.anchorMin;
            max = rect.anchorMax;
            min.x = max.x = anchorX;
            rect.anchorMin = min;
            rect.anchorMax = max;
        }

        /// <summary>
        /// Sets the anchor for the given RectTransform on the y-axis.
        /// </summary>
        static public void SetAnchorY(RectTransform rect, float anchorY) {
            Vector2 min, max;
            min = rect.anchorMin;
            max = rect.anchorMax;
            min.x = max.y = anchorY;
            rect.anchorMin = min;
            rect.anchorMax = max;
        }

        /// <summary>
        /// Sets the anchor for the given RectTransform.
        /// </summary>
        static public void SetAnchor(RectTransform rect, Vector2 anchorXY) {
            rect.anchorMin = anchorXY;
            rect.anchorMax = anchorXY;
        }

        /// <summary>
        /// Sets the anchor for the given RectTransform.
        /// </summary>
        static public void SetAnchor(RectTransform rect, TextAnchor anchor) {
            Vector2 anchorXY = new Vector2(GetAnchorX(anchor), GetAnchorY(anchor));
            rect.anchorMin = anchorXY;
            rect.anchorMax = anchorXY;
        }

        /// <summary>
        /// Sets the anchor and offset for the given RectTransform on the x-axis.
        /// </summary>
        static public void SetAnchorOffsetX(RectTransform rect, float anchorX, float offsetX) {
            Vector2 min, max, offset;
            min = rect.anchorMin;
            max = rect.anchorMax;
            offset = rect.anchoredPosition;
            min.x = max.x = anchorX;
            offset.x = offsetX;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = offset;
        }

        /// <summary>
        /// Sets the anchor and offset for the given RectTransform on the y-axis.
        /// </summary>
        static public void SetAnchorOffsetY(RectTransform rect, float anchorY, float offsetY) {
            Vector2 min, max, offset;
            min = rect.anchorMin;
            max = rect.anchorMax;
            offset = rect.anchoredPosition;
            min.x = max.y = anchorY;
            offset.y = offsetY;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = offset;
        }

        /// <summary>
        /// Sets the anchor and offset for the given RectTransform.
        /// </summary>
        static public void SetAnchorOffset(RectTransform rect, Vector2 anchorXY, Vector2 offset) {
            rect.anchorMin = anchorXY;
            rect.anchorMax = anchorXY;
            rect.anchoredPosition = offset;
        }

        /// <summary>
        /// Sets the anchor and offset for the given RectTransform.
        /// </summary>
        static public void SetAnchorOffset(RectTransform rect, TextAnchor anchor, Vector2 offset) {
            Vector2 anchorXY = new Vector2(GetAnchorX(anchor), GetAnchorY(anchor));
            rect.anchorMin = anchorXY;
            rect.anchorMax = anchorXY;
            rect.anchoredPosition = offset;
        }

        #endregion // Anchors

        #region Pivot

        /// <summary>
        /// Sets the pivot point for the given RectTransform.
        /// </summary>
        static public void SetPivot(RectTransform rect, TextAnchor pivot) {
            rect.pivot = new Vector2(GetAnchorX(pivot), GetAnchorY(pivot));
        }

        #endregion // Pivot

        #region Horizontal Layout

        /// <summary>
        /// Horizontally lays out given set of RectTransforms.
        /// </summary>
        static public float HorizontalLayout(TempReferenceBuffer<RectTransform> buffer, in LayoutOptions options, float basePosition = 0) {
            return DoHorizontalLayoutRect(buffer, options, basePosition);
        }

        static private unsafe float DoHorizontalLayoutRect(TempReferenceBuffer<RectTransform> buffer, in LayoutOptions options, float basePosition) {
            int len = buffer.Count;

            if (len == 0) {
                return 0;
            }

            float* offsets = stackalloc float[len];
            float totalSize = 0;
            float size = 0;
            float pivot;

            RectTransform rect;
            switch (options.Source) {
                case LayoutSource.PreferredSize: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        size = LayoutUtility.GetPreferredHeight(rect);
                        pivot = rect.pivot.y;
                        offsets[i] = totalSize + (1 - pivot) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
                case LayoutSource.Size: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        size = rect.rect.height;
                        pivot = rect.pivot.y;
                        offsets[i] = totalSize + (1 - pivot) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
                case LayoutSource.FixedSize: {
                    size = options.FixedSize;
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        pivot = rect.pivot.y;
                        offsets[i] = totalSize + (1 - pivot) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
            }

            basePosition = basePosition - (totalSize) * (1 - options.NormalizedAlignment);

            for (int i = 0; i < len; i++) {
                rect = buffer[i];
#if UNITY_EDITOR
                Baking.PrepareUndo(rect, "Horizontal alignment");
#endif // UNITY_EDITOR
                Vector2 anchoredPos = rect.anchoredPosition;
                anchoredPos.x = basePosition + offsets[i];
                rect.anchoredPosition = anchoredPos;
            }

            return totalSize;
        }

        #endregion // Horizontal Layout

        #region Vertical Layout

        /// <summary>
        /// Vertically lays out given set of RectTransforms.
        /// </summary>
        static public float VerticalLayout(TempReferenceBuffer<RectTransform> buffer, in LayoutOptions options, float basePosition = 0) {
            return DoVerticalLayoutRect(buffer, options, basePosition);
        }

        static private unsafe float DoVerticalLayoutRect(TempReferenceBuffer<RectTransform> buffer, in LayoutOptions options, float basePosition) {
            int len = buffer.Count;

            if (len == 0) {
                return 0;
            }

            float* offsets = stackalloc float[len];
            float totalSize = 0;
            float size = 0;
            float pivot;
            float direction = -1;
            bool flipPivot = false;
            if ((options.Flags & LayoutFlags.VerticalLayoutUp) != 0) {
                direction = 1;
                flipPivot = true;
            }
            RectTransform rect;
            switch(options.Source) {
                case LayoutSource.PreferredSize: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        size = LayoutUtility.GetPreferredHeight(rect);
                        pivot = rect.pivot.y;
                        offsets[i] = totalSize + (flipPivot ? pivot : (1 - pivot)) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
                case LayoutSource.Size: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        size = rect.rect.height;
                        pivot = rect.pivot.y;
                        offsets[i] = totalSize + (flipPivot ? pivot : (1 - pivot)) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
                case LayoutSource.FixedSize: {
                    size = options.FixedSize;
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        pivot = rect.pivot.y;
                        offsets[i] = totalSize + (flipPivot ? pivot : (1 - pivot)) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
            }

            basePosition = basePosition - (direction * totalSize) * (flipPivot ? options.NormalizedAlignment : (1 - options.NormalizedAlignment));

            for (int i = 0; i < len; i++) {
                rect = buffer[i];
#if UNITY_EDITOR
                Baking.PrepareUndo(rect, "Vertical alignment");
#endif // UNITY_EDITOR
                Vector2 anchoredPos = rect.anchoredPosition;
                anchoredPos.y = basePosition + direction * offsets[i];
                rect.anchoredPosition = anchoredPos;
            }

            return totalSize;
        }

        #endregion // Vertical Layout

        #region Axis Layout

        /// <summary>
        /// Lays out given set of Transforms along the given axis.
        /// </summary>
        static public float AxisLayout(TempReferenceBuffer<Transform> buffer, in LayoutOptions options, float basePosition, Axis axis) {
            return DoAxisLayout(buffer, options, basePosition, axis);
        }

        static private unsafe float DoAxisLayout(TempReferenceBuffer<Transform> buffer, in LayoutOptions options, float basePosition, Axis axis) {
            int len = buffer.Count;

            if (len == 0) {
                return 0;
            }

            Assert.True(axis == Axis.X || axis == Axis.Y || axis == Axis.Z, "Invalid axis");
            int axisIndex = Bits.IndexOf(axis);

            float* offsets = stackalloc float[len];
            float totalSize = 0;
            float size = 0;
            float pivot;

            Transform transform;
            LayoutSizeInfo sizeInfo;
            switch (options.Source) {
                case LayoutSource.PreferredSize:
                case LayoutSource.Size: {
                    for (int i = 0; i < len; i++) {
                        transform = buffer[i];
                        if (transform.TryGetComponent(out sizeInfo)) {
                            pivot = sizeInfo.Pivot[axisIndex];
                            size = sizeInfo.Size[axisIndex];
                        } else {
                            pivot = 0.5f;
                            size = transform.localScale[axisIndex];
                        }
                        offsets[i] = totalSize + (1 - pivot) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
                case LayoutSource.FixedSize: {
                    size = options.FixedSize;
                    for (int i = 0; i < len; i++) {
                        transform = buffer[i];
                        if (transform.TryGetComponent(out sizeInfo)) {
                            pivot = sizeInfo.Pivot[axisIndex];
                        } else {
                            pivot = 0.5f;
                        }
                        offsets[i] = totalSize + (1 - pivot) * size;
                        totalSize += options.Spacing + size;
                    }
                    totalSize -= options.Spacing;
                    break;
                }
            }

            basePosition = basePosition - (totalSize) * (1 - options.NormalizedAlignment);

            for (int i = 0; i < len; i++) {
                transform = buffer[i];
#if UNITY_EDITOR
                Baking.PrepareUndo(transform, "Axis alignment");
#endif // UNITY_EDITOR
                Vector3 localPos = transform.localPosition;
                localPos[axisIndex] = basePosition + offsets[i];
                transform.localPosition = localPos;
            }

            return totalSize;
        }

        #endregion // Axis Layout
    }

    public enum LayoutSource : byte {
        FixedSize,
        PreferredSize,
        Size,
    }

    [Flags]
    public enum LayoutFlags : ushort {
        VerticalLayoutUp = 0x01,
    }

    [Serializable]
    public struct LayoutOptions {
        public LayoutSource Source;
        public LayoutFlags Flags;
        public float NormalizedAlignment;
        public float Spacing;
        public float FixedSize;
    }
}