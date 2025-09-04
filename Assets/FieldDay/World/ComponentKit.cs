using FieldDay.Scenes;
using System;
using UnityEngine;

namespace FieldDay {
    /// <summary>
    /// Base class for a component used to coordinate the setup of
    /// several components. It will be deleted in builds.
    /// </summary>
    public abstract class ComponentKit : MonoBehaviour, IEditorOnly {

        /// <summary>
        /// Called during OnValidate.
        /// </summary>
        protected virtual void ApplyChanges() {
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (Frame.IsActive(this)) {
                ApplyChanges();
            }
        }
#else
        private ComponentKit() {
            throw new Exception("ComponentKits cannot exist outside of editor.");
        }
#endif // UNITY_EDITOR
    }
}