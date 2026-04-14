using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Collections;
using ScriptableBake;
using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay {
    [Il2CppEagerStaticClassConstruction]
    static public class Positioning {

        #region Queries

        /// <summary>
        /// Returns a temporary buffer containing all active immediate children of the given root.
        /// </summary>
        static public TempReferenceBuffer<Transform> QueryActiveChildren(this Transform root) {
            int count = root.childCount;
            if (count <= 0) {
                return default;
            }

            TempReferenceBuffer<Transform> temp = TempReferenceBuffer<Transform>.Create(count);
            QueryActiveChildren(root, temp);
            return temp;
        }

        /// <summary>
        /// Fills a temporary buffer containing all active immediate children of the given root.
        /// </summary>
        static public int QueryActiveChildren(this Transform root, TempReferenceBuffer<Transform> buffer) {
            int count = root.childCount;
            for (int i = 0; i < count; i++) {
                Transform t = root.GetChild(i);
                if (t.gameObject.activeSelf) {
                    buffer.Add(t);
                }
            }
            return count;
        }

        /// <summary>
        /// Returns a temporary buffer containing all active immediate children of the given root.
        /// </summary>
        static public TempReferenceBuffer<RectTransform> QueryActiveChildren(this RectTransform root) {
            int count = root.childCount;
            if (count <= 0) {
                return default;
            }

            TempReferenceBuffer<RectTransform> temp = TempReferenceBuffer<RectTransform>.Create(count);
            QueryActiveChildren(root, temp);
            return temp;
        }

        /// <summary>
        /// Fills a temporary buffer containing all active immediate children of the given root.
        /// </summary>
        static public int QueryActiveChildren(this RectTransform root, TempReferenceBuffer<RectTransform> buffer) {
            int count = root.childCount;
            for(int i = 0; i < count; i++) {
                Transform t = root.GetChild(i);
                if (t.gameObject.activeSelf) {
                    buffer.Add(Unsafe.FastCast<RectTransform>(t));
                }
            }
            return count;
        }

        #endregion // Queries

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
            float* pivots = stackalloc float[len];
            float* sizes = stackalloc float[len];

            float totalSize = 0;

            RectTransform rect;
            switch (options.Source) {
                case LayoutSource.PreferredSize: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        sizes[i] = LayoutUtility.GetPreferredHeight(rect);
                        pivots[i] = rect.pivot.x;
                    }
                    totalSize = ProcessPositionsDynamicSize(len, sizes, pivots, options.Spacing, offsets);
                    break;
                }
                case LayoutSource.Size: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        sizes[i] = rect.rect.height;
                        pivots[i] = rect.pivot.x;
                    }
                    totalSize = ProcessPositionsDynamicSize(len, sizes, pivots, options.Spacing, offsets);
                    break;
                }
                case LayoutSource.FixedSize: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        pivots[i] = rect.pivot.y;
                    }
                    totalSize = ProcessPositionsFixedSize(len, options.FixedSize, pivots, options.Spacing, offsets);
                    break;
                }
            }

            basePosition = ComputeBasePosition(basePosition, totalSize, options.NormalizedAlignment);

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

        /// <summary>
        /// Horizontally aligns the given set of RectTransforms.
        /// </summary>
        static public void HorizontalAlign(TempReferenceBuffer<RectTransform> buffer, float basePosition) {
            int len = buffer.Count;

            if (len == 0) {
                return;
            }

            RectTransform rect;
            Vector3 anchorPos;
            for(int i = 0; i < len; i++) {
                rect = buffer[i];
                anchorPos = rect.anchoredPosition3D;
                anchorPos.x = basePosition;
                rect.anchoredPosition3D = anchorPos;
            }
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
            float* pivots = stackalloc float[len];
            float* sizes = stackalloc float[len];
            float totalSize = 0;

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
                        sizes[i] = LayoutUtility.GetPreferredHeight(rect);
                        pivots[i] = ConditionalFlipPivot(rect.pivot.y, flipPivot);
                    }
                    totalSize = ProcessPositionsDynamicSize(len, sizes, pivots, options.Spacing, offsets);
                    break;
                }
                case LayoutSource.Size: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        sizes[i] = rect.rect.height;
                        pivots[i] = ConditionalFlipPivot(rect.pivot.y, flipPivot);
                    }
                    totalSize = ProcessPositionsDynamicSize(len, sizes, pivots, options.Spacing, offsets);
                    break;
                }
                case LayoutSource.FixedSize: {
                    for (int i = 0; i < len; i++) {
                        rect = buffer[i];
                        pivots[i] = ConditionalFlipPivot(rect.pivot.y, flipPivot);
                    }
                    totalSize = ProcessPositionsFixedSize(len, options.FixedSize, pivots, options.Spacing, offsets);
                    break;
                }
            }

            basePosition = ComputeBasePosition(basePosition, direction * totalSize, ConditionalFlipPivot(options.NormalizedAlignment, flipPivot));

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

        /// <summary>
        /// Vertically aligns the given set of RectTransforms.
        /// </summary>
        static public void VerticalAlign(TempReferenceBuffer<RectTransform> buffer, float basePosition) {
            int len = buffer.Count;

            if (len == 0) {
                return;
            }

            RectTransform rect;
            Vector3 anchorPos;
            for (int i = 0; i < len; i++) {
                rect = buffer[i];
                anchorPos = rect.anchoredPosition3D;
                anchorPos.y = basePosition;
                rect.anchoredPosition3D = anchorPos;
            }
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
            float* pivots = stackalloc float[len];
            float* sizes = stackalloc float[len];
            float totalSize = 0;

            Transform transform;
            LayoutSizeInfo sizeInfo;
            switch (options.Source) {
                case LayoutSource.PreferredSize:
                case LayoutSource.Size: {
                    for (int i = 0; i < len; i++) {
                        transform = buffer[i];
                        if (transform.TryGetComponent(out sizeInfo)) {
                            pivots[i] = sizeInfo.Pivot[axisIndex];
                            sizes[i] = sizeInfo.Size[axisIndex];
                        } else {
                            pivots[i] = 0.5f;
                            sizes[i] = transform.localScale[axisIndex];
                        }
                    }
                    totalSize = ProcessPositionsDynamicSize(len, sizes, pivots, options.Spacing, offsets);
                    break;
                }
                case LayoutSource.FixedSize: {
                    for (int i = 0; i < len; i++) {
                        transform = buffer[i];
                        if (transform.TryGetComponent(out sizeInfo)) {
                            pivots[i] = sizeInfo.Pivot[axisIndex];
                        } else {
                            pivots[i] = 0.5f;
                        }
                    }
                    totalSize = ProcessPositionsFixedSize(len, options.FixedSize, pivots, options.Spacing, offsets);
                    break;
                }
            }

            basePosition = ComputeBasePosition(basePosition, totalSize, options.NormalizedAlignment);

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

        #region Layout Math

        [Il2CppSetOption(Option.NullChecks, false)]
        static private unsafe float ProcessPositionsDynamicSize(int entryCount, float* sizes, float* pivots, float spacing, float* results) {
            float total = 0;
            float size;
            for(int i = 0; i < entryCount; i++) {
                size = sizes[i];
                results[i] = total + (1 - pivots[i]) * size;
                total += spacing + size;
            }
            total -= spacing;
            return total;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        static private unsafe float ProcessPositionsFixedSize(int entryCount, float size, float* pivots, float spacing, float* results) {
            float total = 0;
            for (int i = 0; i < entryCount; i++) {
                results[i] = total + (1 - pivots[i]) * size;
                total += spacing + size;
            }
            total -= spacing;
            return total;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private float ComputeBasePosition(float originalBasePosition, float totalSize, float normalizedAlignment) {
            return originalBasePosition - (totalSize * (1 - normalizedAlignment));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private float ConditionalFlipPivot(float pivot, bool flip) {
            return flip ? (1 - pivot) : pivot;
        }

        #endregion // Layout Math
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
        [ShowIfField("DisplayFixedSize")] public float FixedSize;

#if UNITY_EDITOR
        private bool DisplayFixedSize() {
            return Source == LayoutSource.FixedSize;
        }
#endif // UNITY_EDITOR
    }
}