using FieldDay.Collections;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

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

        #endregion // Horizontal Layout
    }
}