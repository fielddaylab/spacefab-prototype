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
        public struct HintValue : IEquatable<HintValue> {
            public readonly string StringValue;

            public HintValue(string value) {
                StringValue = value;
            }

            public HintValue(bool value) {
                StringValue = value ? "true" : null;
            }

            public readonly bool AsBool() {
                return !string.IsNullOrEmpty(StringValue) && !string.Equals(StringValue, "false", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(StringValue, "0", StringComparison.Ordinal);
            }

            public bool Equals(HintValue other) {
                return string.Equals(StringValue, other.StringValue, StringComparison.Ordinal);
            }

            public override int GetHashCode() {
                return HashCode.Combine(StringValue);
            }

            public override string ToString() {
                return StringValue ?? "[null]";
            }

            public override bool Equals(object obj) {
                if (obj is HintValue) {
                    return Equals((HintValue)obj);
                }
                return false;
            }

            static public bool operator==(HintValue left, HintValue right) {
                return left.Equals(right);
            }

            static public bool operator !=(HintValue left, HintValue right) {
                return !left.Equals(right);
            }

            static public implicit operator bool(HintValue value) {
                return value.AsBool();
            }
        }

        private class HintEntry {
            public HintValue Value;
            public CastableEvent<HintValue> OnUpdated;
            public bool Locked;
        }

        static private Dictionary<StringHash32, HintEntry> s_HintMap;

        /// <summary>
        /// Initializes the engine hint system.
        /// </summary>
        static public void Initialize() {
            if (s_HintMap != null) {
                return;
            }

            s_HintMap = MapUtils.Create<StringHash32, HintEntry>(32);
            Log.Msg("[EngineHints] Initialized engine hint map");
        }

        /// <summary>
        /// Shuts down the engine hint system.
        /// </summary>
        static public void Shutdown() {
            if (s_HintMap != null) {
                foreach (var entry in s_HintMap.Values) {
                    entry.OnUpdated?.Clear();
                }
                s_HintMap.Clear();
                s_HintMap = null;
                Log.Msg("[EngineHints] Shut down engine hint map");
            }
        }

        static private HintEntry GetEntry(string name, bool create) {
            Assert.True(!string.IsNullOrEmpty(name));

            StringHash32 key = StringHash32.FastCaseInsensitive(name);
            if (!s_HintMap.TryGetValue(key, out HintEntry entry) && create) {
                entry = new HintEntry();
                s_HintMap.Add(key, entry);
            }
            return entry;
        }

        /// <summary>
        /// Sets a hint as a string.
        /// </summary>
        static public void SetHint(string name, string value) {
            HintEntry entry = GetEntry(name, true);
            if (entry.Locked) {
                Log.Warn("[EngineHints] Hint '{0}' is locked!", name);
                return;
            }

            HintValue hintVal = new HintValue(value);
            if (!entry.Value.Equals(hintVal)) {
                entry.Value = hintVal;
                Log.Msg("[EngineHints] Set hint '{0}'='{1}'", name, value);
                entry.OnUpdated?.Invoke(hintVal);
            }
        }

        /// <summary>
        /// Sets a hint as a boolean.
        /// </summary>
        static public void SetHint(string name, bool value) {
            HintEntry entry = GetEntry(name, true);
            if (entry.Locked) {
                Log.Warn("[EngineHints] Hint '{0}' is locked!", name);
                return;
            }

            HintValue hintVal = new HintValue(value);
            if (!entry.Value.AsBool() != value) {
                entry.Value = hintVal;
                Log.Msg("[EngineHints] Set hint '{0}'={1}", name, value ? "true" : "false");
                entry.OnUpdated?.Invoke(hintVal);
            }
        }

        /// <summary>
        /// Retrieves the hint.
        /// </summary>
        static public HintValue GetHint(string name, string defaultValue) {
            HintEntry entry = GetEntry(name, false);
            if (entry != null) {
                return entry.Value;
            }
            return new HintValue(defaultValue);
        }

        /// <summary>
        /// Retrieves the hint, as a boolean.
        /// </summary>
        static public bool GetHintBool(string name, bool defaultValue) {
            HintEntry entry = GetEntry(name, false);
            if (entry != null) {
                return entry.Value.AsBool();
            }
            return defaultValue;
        }

        /// <summary>
        /// Locks the hint, preventing further changes.
        /// </summary>
        static public void LockHint(string name) {
            HintEntry entry = GetEntry(name, true);
            entry.Locked = true;
        }

        /// <summary>
        /// Unlocks the hint.
        /// </summary>
        static public void UnlockHint(string name) {
            HintEntry entry = GetEntry(name, false);
            if (entry != null) {
                entry.Locked = false;
            }
        }

        /// <summary>
        /// Adds a callback for when this value changes.
        /// Will invoke the callback immediately.
        /// </summary>
        static public void Watch(string name, Action<HintValue> handler) {
            Assert.NotNull(handler);

            HintEntry entry = GetEntry(name, true);
            if (entry.OnUpdated == null) {
                entry.OnUpdated = new CastableEvent<HintValue>();
            }
            entry.OnUpdated.Deregister(handler);
            entry.OnUpdated.Register(handler);
            handler.Invoke(entry.Value);
        }

        /// <summary>
        /// Removes a callback for when this value changes.
        /// </summary>
        static public void Unwatch(string name, Action<HintValue> handler) {
            Assert.NotNull(handler);

            HintEntry entry = GetEntry(name, false);
            if (entry != null && entry.OnUpdated != null) {
                entry.OnUpdated.Deregister(handler);
            }
        }
    }
}