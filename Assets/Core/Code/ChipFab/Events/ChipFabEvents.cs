using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public static class GameEvents
    {
        public static StringHash32 NewWaferCreated = "new-wafer-created";
        public static StringHash32 WaferPickedUp = "wafer-picked-up";

        public static StringHash32 WaferStateUpdated = "wafer-state-updated";
        public static StringHash32 WaferStateUndone = "wafer-state-undone";

        public static StringHash32 NewDopantCreated = "new-dopant-created";

        public static StringHash32 TimerBegin = "timer-begin";
        public static StringHash32 StationStarted = "station-started";
        public static StringHash32 StationCompleted = "station-completed";

        public static StringHash32 WaferSubmitted = "wafer-submitted";
        public static StringHash32 AutomationStarted = "automation-started";
        public static StringHash32 AutomationCompleted = "automation-completed";

        public static StringHash32 IncorrectStationAttempted = "incorrect-station-attempted";
    }

    public static class GameConsts
    {

    }
}