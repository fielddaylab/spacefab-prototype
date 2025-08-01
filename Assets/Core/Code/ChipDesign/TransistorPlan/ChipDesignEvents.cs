using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public static class GameEvents
    {
        public static StringHash32 OnLayerChanged = "on-layer-changed";
        public static StringHash32 OnToolChanged = "on-tool-changed";
        public static StringHash32 EvaluationStarted = "evaluation-started";
        public static StringHash32 OnLayoutChanged = "on-layout-changed";
        public static StringHash32 OnFloorLinksChanged = "on-floor-links-changed";
    }

    public static class GameConsts
    {
        public static float UNSTABLE_CODE = -29584;
        public static float DEFFERED_CODE = float.MinValue;
    }
}