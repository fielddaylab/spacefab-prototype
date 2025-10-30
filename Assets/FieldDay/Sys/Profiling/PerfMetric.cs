#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using Unity.Profiling.LowLevel;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEditor;

namespace FieldDay.Perf {
    public readonly struct PerfMetric {
        public readonly ProfilerCategory Category;
        public readonly string Name;

        public PerfMetric(ProfilerCategory category, string name) {
            Category = category;
            Name = name;
        }

        #region Module

        static public void Initialize() {
            List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>(EstimatedAvailableCount);
            ProfilerRecorderHandle.GetAvailable(handles);

            int count = 0;
            foreach (var handle in handles) {
                if (!handle.Valid) {
                    continue;
                }
                count++;
            }

            Log.Msg("[PerfMetric] {0} metrics found", count);
        }

        static public void Shutdown() {
            
        }

        #endregion // Module

        #region Strings

        /// <summary>
        /// Writes the last value of a ProfilerRecorder, formatted with the appropriate units.
        /// </summary>
        static public void FormatValue(StringBuilder output, ProfilerRecorder recorder) {
            if (recorder.Valid && recorder.IsRunning && recorder.Count > 0) {
                int prevSize = output.Length;
                switch(recorder.DataType) {
                    case ProfilerMarkerDataType.Float:
                    case ProfilerMarkerDataType.Double:
                        WriteDouble(output, recorder.LastValueAsDouble, recorder.UnitType);
                        break;
                    case ProfilerMarkerDataType.Int64:
                    case ProfilerMarkerDataType.UInt64:
                    case ProfilerMarkerDataType.Int32:
                    case ProfilerMarkerDataType.UInt32:
                        WriteInt(output, recorder.LastValue, recorder.UnitType);
                        break;
                }
                if (output.Length == prevSize) {
                    output.Append("[unknown]");
                }
            } else {
                output.Append("---");
            }
        }

        static private void WriteInt(StringBuilder output, long value, ProfilerMarkerDataUnit unit) {
            switch (unit) {
                case ProfilerMarkerDataUnit.TimeNanoseconds:
                    output.AppendNoAlloc(value).Append("ns");
                    break;
                case ProfilerMarkerDataUnit.Percent:
                    output.AppendNoAlloc(value).Append("%");
                    break;
                case ProfilerMarkerDataUnit.Count:
                    output.AppendNoAlloc(value);
                    break;
                case ProfilerMarkerDataUnit.FrequencyHz:
                    output.AppendNoAlloc(value).Append("hz");
                    break;
                case ProfilerMarkerDataUnit.Bytes:
                    Unsafe.FormatBytes(value, output);
                    break;
            }
        }

        static private void WriteDouble(StringBuilder output, double value, ProfilerMarkerDataUnit unit) {
            switch(unit) {
                case ProfilerMarkerDataUnit.TimeNanoseconds:
                    output.AppendNoAlloc(value, 2).Append("ns");
                    break;
            }
        }

        #endregion // Strings

        #region Enumerating

        private const int EstimatedAvailableCount = Game.IsEditor ? 4096 : (Game.IsDevBuild ? 1500 : 128);

        static public IEnumerable<ProfilerRecorderDescription> EnumerateAvailableMetrics() {
            List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>(EstimatedAvailableCount);
            ProfilerRecorderHandle.GetAvailable(handles);
            foreach(var handle in handles) {
                if (handle.Valid) {
                    yield return ProfilerRecorderHandle.GetDescription(handle);
                }
            }
        }

        #endregion // Enumerating
    }
}