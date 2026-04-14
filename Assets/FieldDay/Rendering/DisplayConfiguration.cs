using FieldDay.Assets;
using UnityEngine;

namespace FieldDay.Rendering {
    [CreateAssetMenu(menuName = "Field Day/Display Configuration", order =-259)]
    public sealed class DisplayConfiguration : ScriptableObject {
        public enum Axis {
            Width,
            Height
        }

        [Header("Reference Resolution")]
        public Vector2Int ReferenceResolution = new Vector2Int(1024, 768);
        public Axis ReferenceAxis = Axis.Height;

        [Header("Aspect Clamping")]
        public Vector2Int MinimumAspectRatio = new Vector2Int(4, 3);
        public Vector2Int MaximumAspectRatio = new Vector2Int(4, 3);
    }
}