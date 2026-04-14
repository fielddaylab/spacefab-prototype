using BeauUtil;
using UnityEngine;
using UnityEngine.UI;

namespace FieldDay {
    public sealed class LayoutSizeInfo : MonoBehaviour, ILayoutElement {
        public Vector3 Pivot = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 Size = Vector3.one;

        #region ILayoutElement

        float ILayoutElement.minWidth { get { return Size.x; } }

        float ILayoutElement.preferredWidth { get { return Size.x; } }

        float ILayoutElement.flexibleWidth { get { return 0; } }

        float ILayoutElement.minHeight { get { return Size.y; } }

        float ILayoutElement.preferredHeight { get { return Size.y; } }

        float ILayoutElement.flexibleHeight { get { return 0; } }

        int ILayoutElement.layoutPriority { get { return -1; } }

        void ILayoutElement.CalculateLayoutInputHorizontal() {
        }

        void ILayoutElement.CalculateLayoutInputVertical() {
        }

        #endregion // ILayoutElement

        #region Editor

#if UNITY_EDITOR

        private void OnDrawGizmosSelected() {
            Matrix4x4 prevMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector3 center = new Vector3(
                Size.x * (Pivot.x - 0.5f),
                Size.y * (Pivot.y - 0.5f),
                Size.z * (Pivot.z - 0.5f)
                );

            Gizmos.color = ColorBank.Wheat.WithAlpha(0.2f);
            Gizmos.DrawCube(center, Size);
            Gizmos.color = ColorBank.Wheat;
            Gizmos.DrawWireCube(center, Size);

            Gizmos.matrix = prevMatrix;
        }

#endif // UNITY_EDITOR

        #endregion // Editor
    }
}