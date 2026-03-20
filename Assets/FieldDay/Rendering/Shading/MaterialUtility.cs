using BeauUtil.Debugger;
using System.Reflection;
using UnityEngine;
using FieldDay.Assets;


#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace FieldDay.Rendering {
    static public class MaterialUtility {
        static public void SetDefaultUIGraphicMaterial(Material material) {
            Assert.NotNullOrDestroyed(material);
            typeof(UnityEngine.UI.Graphic).GetField("s_DefaultUI", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, material);
            Log.Msg("[MaterialUtility] Replaced default UI material with {0}", material.name);
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static private void EditorInitialize() {
            EditorApplication.delayCall += () => {
                Material attemptedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/FieldDay/_Assets/Materials/UI/UI-Premultiplied.mat");
                if (attemptedMaterial != null) {
                    SetDefaultUIGraphicMaterial(attemptedMaterial);
                }
            };
        }
#endif // UNITY_EDITOR
    }
}