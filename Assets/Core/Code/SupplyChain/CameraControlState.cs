using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class CameraControlState : SharedStateComponent, IRegistrationCallbacks {
        [Header("Components")]
        [Required] public Camera Camera;
        [Required] public Transform CameraPosition;

        [Header("Configuration")]
        [Required] public BoxCollider2D Region;
        public float InterpolationStrength;
        public float MovementSpeed;

        [NonSerialized] public Vector2 TargetPosition;

        void IRegistrationCallbacks.OnDeregister() {
        }

        void IRegistrationCallbacks.OnRegister() {
            TargetPosition = CameraPosition.position;
        }
    }
}