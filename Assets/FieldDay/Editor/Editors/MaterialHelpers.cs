using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Editor;
using FieldDay.Rendering;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FieldDay.Editor {
    static public class MaterialHelpers {
        [MenuItem("CONTEXT/Material/Clean Up Unused Properties")]
        static private void ContextCleanUp(MenuCommand cmd) {
            Material material = (Material) cmd.context;
            RemoveExtraProperties(material);
        }

        [MenuItem("Field Day/Maintenance/Clean Up Unused Properties on ALL Materials")]
        static private void CleanUpAll() {
            Material[] materials = AssetDBUtils.FindAssets<Material>(null, new string[] {"Assets"});
            if (materials.Length > 0) {
                if (!EditorUtility.DisplayDialog("WARNING", "You're about to potentially modify " + materials.Length + " material files. Proceed?", "Yes", "Wait no")) {
                    return;
                }

                int materialCount = 0;
                int propertyCount = 0;
                foreach(var material in materials) {
                    int removed = RemoveExtraProperties(material);
                    if (removed > 0) {
                        materialCount++;
                        propertyCount += removed;
                    }
                }

                if (materialCount > 0) {
                    Debug.LogFormat("[MaterialHelpers] Removed {0} properties from {1} materials", propertyCount, materialCount);
                }
            }
        }

        [MenuItem("Field Day/Maintenance/Clean Up Unused Properties on Selected Materials")]
        static private void CleanUpAllSelected() {
            foreach (var obj in Selection.objects) {
                if (obj is Material) {
                    RemoveExtraProperties(obj as Material);
                }
            }
        }

        [MenuItem("Field Day/Maintenance/Clean Up Unused Properties on Selected Materials", validate = true)]
        static private bool CleanUpAllSelected_Validate() {
            foreach(var obj in Selection.objects) {
                if (obj is Material) {
                    return true;
                }
            }
            return false;
        }

        static public int RemoveExtraProperties(Material material) {
            Assert.NotNullOrDestroyed(material);
            return RemoveExtraProperties(material, material.shader);
        }

        static public int RemoveExtraProperties(Material material, Shader sourceShader) {
            Assert.NotNullOrDestroyed(material);
            if (!sourceShader) {
                return 0;
            }

            SerializedObject serializedObj = new SerializedObject(material);
            SerializedProperty savedPropertiesStruct = serializedObj.FindProperty("m_SavedProperties");
            List<string> removedProperties = new List<string>(32);
            int removedCount = StripMissingPropertiesFromArray(savedPropertiesStruct.FindPropertyRelative("m_TexEnvs"), sourceShader, removedProperties);
            removedCount += StripMissingPropertiesFromArray(savedPropertiesStruct.FindPropertyRelative("m_Ints"), sourceShader, removedProperties);
            removedCount += StripMissingPropertiesFromArray(savedPropertiesStruct.FindPropertyRelative("m_Floats"), sourceShader, removedProperties);
            removedCount += StripMissingPropertiesFromArray(savedPropertiesStruct.FindPropertyRelative("m_Colors"), sourceShader, removedProperties);
            removedCount += StripKeywordsFromArray(serializedObj.FindProperty("m_InvalidKeywords"), sourceShader, removedProperties);

            serializedObj.ApplyModifiedProperties();

            if (removedCount > 0) {
                StringBuilder sb = new StringBuilder(2048);
                sb.Append("[MaterialHelpers] Deleted ").AppendNoAlloc(removedCount).Append(" unused properties from material '").Append(AssetDatabase.GetAssetPath(material))
                    .Append("' with shader '").Append(sourceShader.name);
                foreach(var propName in removedProperties) {
                    sb.Append("\n - ").Append(propName);
                }
                Debug.LogFormat(material, sb.ToString());
            }

            return removedCount;
        }

        static private int StripMissingPropertiesFromArray(SerializedProperty prop, Shader source, List<string> removedTracker) {
            int count = prop.arraySize;
            int removedCount = 0;
            
            for(int i = count; i-- > 0;) {
                SerializedProperty element = prop.GetArrayElementAtIndex(i);
                //Debug.LogFormat("element at index {0} is {1} of type {2}", i, element.name, element.type);
                element.Next(true);
                //Debug.LogFormat("inner is {0} with type {1}", element.name, element.type);
                string propertyName = element.stringValue;
                int shaderPropIndex = source.FindPropertyIndex(propertyName);
                if (shaderPropIndex < 0) {
                    if (removedTracker == null) {
                        Debug.LogWarningFormat("property {0} is not found in shader {1}", propertyName, source.name);
                    } else {
                        removedTracker.Add(propertyName);
                    }
                    prop.DeleteArrayElementAtIndex(i);
                    removedCount++;
                }
            }

            return removedCount;
        }

        static private int StripKeywordsFromArray(SerializedProperty prop, Shader source, List<string> removedTracker) {
            int count = prop.arraySize;

            for (int i = count; i-- > 0;) {
                SerializedProperty element = prop.GetArrayElementAtIndex(i);
                //Debug.LogFormat("element at index {0} is {1} of type {2}", i, element.name, element.type);
                //Debug.LogFormat("inner is {0} with type {1}", element.name, element.type);
                string propertyName = element.stringValue;
                if (removedTracker == null) {
                    Debug.LogWarningFormat("keyword {0} is not found in shader {1}", propertyName, source.name);
                } else {
                    removedTracker.Add(propertyName);
                }
            }

            prop.ClearArray();
            return count;
        }
    }
}