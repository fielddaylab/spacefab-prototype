using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Streaming;
using FieldDay.Assets;
using FieldDay.Data;
using ScriptableBake;
using UnityEngine;

namespace FieldDay.Scripting {
    [CreateAssetMenu(menuName = "Field Day/Scripting/Script Configuration")]
    public sealed class ScriptingConfiguration : GlobalAsset, IBaked {
        [Tooltip("If set, all lines with custom line names will automatically be treated as voiceover mappings.")]
        public bool AutoLoadCustomLineNamesIntoVox = false;

        [Tooltip("If set, all lines will be assumed to have voiceover.")]
        public bool EnableVoxOnAllLinesByDefault = false;

        [Tooltip("If set, all lines will be assumed to have a dialog box.")]
        public bool EnableDialogBoxOnAllLinesByDefault = true;

        [Tooltip("Default id for dialog boxes")]
        public SerializedHash32 DefaultDialogBoxId;

        public override void Mount() {
            bool voxEnabled = EngineHints.GetHintBool("VOX_ENABLED", true);
            ScriptUtility.DB.AutoLoadCustomLineNamesIntoVox = voxEnabled && AutoLoadCustomLineNamesIntoVox;
            if (voxEnabled && EnableVoxOnAllLinesByDefault) {
                ScriptUtility.Runtime.Flags |= ScriptRuntimeConfigFlags.VoiceoverAllLinesByDefault;
            }
            if (EnableDialogBoxOnAllLinesByDefault) {
                ScriptUtility.Runtime.Flags |= ScriptRuntimeConfigFlags.UseDialogBoxByDefault;
            }
            ScriptUtility.DefaultDialoguePrinterId = DefaultDialogBoxId;
        }

        public override void Unmount() {
            Assert.True(Game.IsShuttingDown);
        }

#if UNITY_EDITOR

        int IBaked.Order { get { return 0; } }

        bool IBaked.Bake(BakeFlags flags, BakeContext context) {
            return false;
        }

#endif // UNITY_EDITOR
    }
}