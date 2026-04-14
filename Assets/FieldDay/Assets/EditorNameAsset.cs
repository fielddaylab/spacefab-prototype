using ScriptableBake;
using System.Diagnostics;
using UnityEngine;
using System;

namespace FieldDay.Assets {
    /// <summary>
    /// Establishes a selectable asset name.
    /// Should not be included in builds.
    /// </summary>
    public abstract class EditorNameAsset : ScriptableObject, IBaked {
#if !UNITY_EDITOR
        protected EditorNameAsset() {
            throw new Exception("EditorNameAsset should not be included in builds!");
        }
#else
        int IBaked.Order => -10000;

        bool IBaked.Bake(BakeFlags flags, BakeContext context) {
            const BakeFlags targetFlags = BakeFlags.IsBatchMode | BakeFlags.IsBuild;
            if ((flags & targetFlags) == targetFlags) {
                DestroyImmediate(this, true);
                return true;
            }

            return false;
        }
#endif // !UNITY_EDITOR
    }
}