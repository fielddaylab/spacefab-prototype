using FieldDay.Assets;
using System.Diagnostics;
using UnityEngine;

namespace FieldDay.UI {
    [CreateAssetMenu(menuName = "Field Day/Input/Input Group Name", fileName = "NewInputGroup")]
    public sealed class InputGroupName : EditorNameAsset {
    }

    public sealed class InputGroupNameAttribute : AssetNameAttribute {
        public InputGroupNameAttribute()
            : base(typeof(InputGroupName), true) {
        }
    }
}