using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scenes;
using FieldDay.SharedState;
using FieldDay.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceFab.Research {
    public sealed class ResearchCamera : SharedStateComponent {
        public Transform CameraPosition;
        public Transform FollowingObject;

        public Physics2DRaycaster RaycasterToDisable;
        [NonSerialized] public Routine ShiftRoutine;
    }

    static public partial class ResearchUtility {
        static public void MoveCameraTo(Transform position) {
            ResearchCamera camera = Find.State<ResearchCamera>();
            camera.RaycasterToDisable.enabled = false;
            camera.ShiftRoutine.Replace(camera, ShiftToX(camera, position.position.x));
        }

        static private IEnumerator ShiftToX(ResearchCamera camera, float x) {
            yield return Routine.Combine(
                camera.CameraPosition.MoveTo(x, 0.15f, Axis.X, Space.World).Ease(Curve.CubeOut),
                camera.FollowingObject.MoveTo(x, 0.24f, Axis.X, Space.World).Ease(Curve.CubeOut).DelayBy(0.07f),
                Routine.Delay(ReactivateRaycaster, 0.08f)
            );
        }

        static private void ReactivateRaycaster() {
            Find.State<ResearchCamera>().RaycasterToDisable.enabled = true;
        }
    }
}