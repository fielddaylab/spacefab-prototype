using FieldDay.Assets;
using System.Diagnostics;
using UnityEngine;

namespace FieldDay.UI {
    [CreateAssetMenu(menuName = "Field Day/Gui/Panel Group Name", fileName = "NewPanelGroup")]
    public sealed class PanelGroupName : EditorNameAsset {
    }

    public sealed class PanelGroupNameAttribute : AssetNameAttribute {
        public PanelGroupNameAttribute()
            : base(typeof(PanelGroupName), true) {
        }
    }
}