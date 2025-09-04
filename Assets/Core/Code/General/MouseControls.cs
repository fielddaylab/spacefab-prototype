using FieldDay;
using UnityEngine;

namespace SpaceFab {
    static public class MouseControls {
        static public bool TryGetWorldPosition2D(out Vector2 position) {
            if (Game.Input.HasPointer() && !Game.Input.AreRaycastsPaused()) {
                position = Game.Rendering.PrimaryCamera.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
                return true;
            }

            position = default;
            return false;
        }
    } 
}