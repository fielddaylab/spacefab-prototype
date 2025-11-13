using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Variants;
using System;
using System.Collections.Generic;

namespace FieldDay.Data {
    /// <summary>
    /// Engine configuration hints.
    /// </summary>
    static public class EngineHints {
        [Serializable]
        public struct SerializedData {
            public string Name;
            public string Value;
        }

        public struct HintValue {
            public string StringValue;
            public Variant VariantValue;
        }

        private class HintEntry {
            public HintValue Value;
            public CastableEvent<HintValue> OnUpdated;
        }

        static private Dictionary<StringHash32, HintEntry> s_HintMap;

        static public void Initialize() {
            if (s_HintMap != null) {
                return;
            }

            s_HintMap = MapUtils.Create<StringHash32, HintEntry>(32);
            Log.Msg("[EngineHints] Initialized engine hint map");
        }

        static public void Shutdown() {
            if (s_HintMap != null) {
                foreach(var entry in s_HintMap.Values) {
                    entry.OnUpdated?.Clear();
                }
                s_HintMap.Clear();
                s_HintMap = null;
                Log.Msg("[EngineHints] Shut down engine hint map");
            }
        }
    }
}