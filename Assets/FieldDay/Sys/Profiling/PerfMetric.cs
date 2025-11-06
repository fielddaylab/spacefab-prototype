#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

#if !USING_TINYIL || (!UNITY_EDITOR && !ENABLE_IL2CPP)
#define RESTRICT_INTERNAL_CALLS
#endif // !USING_TINYIL || (!UNITY_EDITOR && !ENABLE_IL2CPP)

using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using Unity.Profiling.LowLevel;
using Unity.Profiling.LowLevel.Unsafe;
using System.Reflection;
using Unity.IL2CPP.CompilerServices;
using System.Runtime.CompilerServices;

#if !RESTRICT_INTERNAL_CALLS
using TinyIL;
#endif // !RESTRICT_INTERNAL_CALLS

namespace FieldDay.Perf {
    /// <summary>
    /// Performance metric identifier.
    /// </summary>
    public readonly partial struct PerfMetric {
        public readonly ulong HashId;
        public readonly ProfilerCategory Category;
        public readonly string Name;
        public readonly string DisplayName;

        public PerfMetric(ProfilerCategory category, string name) {
            Category = category;
            Name = name;
            DisplayName = name;
            HashId = CalculateHash(category, name);
        }

        public PerfMetric(Categories category, string name) {
            Category = CategoryMap[(int) category];
            Name = name;
            DisplayName = name;
            HashId = CalculateHash(Category, name);
        }

        public PerfMetric(string category, string name) {
            Category = new ProfilerCategory(category);
            Name = name;
            DisplayName = name;
            HashId = CalculateHash(Category, name);
        }

        public PerfMetric(ProfilerCategory category, string name, string displayName) {
            Category = category;
            Name = name;
            DisplayName = displayName;
            HashId = CalculateHash(category, name);
        }

        public PerfMetric(Categories category, string name, string displayName) {
            Category = CategoryMap[(int) category];
            Name = name;
            DisplayName = displayName;
            HashId = CalculateHash(Category, name);
        }

        public PerfMetric(string category, string name, string displayName) {
            Category = new ProfilerCategory(category);
            Name = name;
            DisplayName = displayName;
            HashId = CalculateHash(Category, name);
        }

        /// <summary>
        /// Returns if this metric is currently available.
        /// </summary>
        public bool IsAvailable {
            get { return HashId != 0UL && GetHandle(this).Valid; }
        }

        static private ulong CalculateHash(ProfilerCategory category, string name) {
            return ((ulong) Unsafe.FastReinterpret<ProfilerCategory, ushort>(category) << 32)
                | (ulong) (StringHash32.FastCaseInsensitive(name).HashValue);
        }

        /// <summary>
        /// Null performance metric.
        /// </summary>
        static public readonly PerfMetric Null = default(PerfMetric);

        #region Categories

        /// <summary>
        /// Categories
        /// </summary>
        public enum Categories {
            Animation,
            Audio,
            Internal,
            Lighting,
            Loading,
            Memory,
            Particles,
            Physics,
            Physics2D,
            Render,
            Scripts,
            VR,
            Video,
            Input,
            FileIO,
            Network,
            Gui,
        }

        static private readonly ProfilerCategory[] CategoryMap = new ProfilerCategory[] {
            ProfilerCategory.Animation,
            ProfilerCategory.Audio,
            ProfilerCategory.Internal,
            ProfilerCategory.Lighting,
            ProfilerCategory.Loading,
            ProfilerCategory.Memory,
            ProfilerCategory.Particles,
            ProfilerCategory.Physics,
            ProfilerCategory.Physics2D,
            ProfilerCategory.Render,
            ProfilerCategory.Scripts,
            ProfilerCategory.Vr,
            ProfilerCategory.Video,
            ProfilerCategory.Input,
            ProfilerCategory.FileIO,
            ProfilerCategory.Network,
            ProfilerCategory.Gui
        };

        #endregion // Categories

        #region Module

        static public void Initialize() {
#if RESTRICT_INTERNAL_CALLS
            s_GetHandle = (GetHandleDelegate) typeof(ProfilerRecorderHandle).GetMethod("GetByName", BindingFlags.Static | BindingFlags.NonPublic)?.CreateDelegate(typeof(GetHandleDelegate));
            s_CreateRecorder = (CreateRecorderFromHandleDelegate)typeof(ProfilerRecorder).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic)?.CreateDelegate(typeof(CreateRecorderFromHandleDelegate));
#endif // RESTRICT_INTERNAL_CALLS
        }

        static public void Shutdown() {
#if RESTRICT_INTERNAL_CALLS
            s_GetHandle = null;
            s_CreateRecorder = null;
#endif // RESTRICT_INTERNAL_CALLS
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
                case ProfilerMarkerDataUnit.FrequencyHz:
                    output.AppendNoAlloc(value).Append("hz");
                    break;
                case ProfilerMarkerDataUnit.Bytes:
                    Unsafe.FormatBytes(value, output);
                    break;
                case ProfilerMarkerDataUnit.Count:
                default:
                    output.AppendNoAlloc(value);
                    break;
            }
        }

        static private void WriteDouble(StringBuilder output, double value, ProfilerMarkerDataUnit unit) {
            switch(unit) {
                case ProfilerMarkerDataUnit.TimeNanoseconds:
                    output.AppendNoAlloc(value, 2).Append("ns");
                    break;
                default:
                    output.AppendNoAlloc(value, 2);
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

        #region Internal Calls

#if RESTRICT_INTERNAL_CALLS
        private delegate ProfilerRecorderHandle GetHandleDelegate(ProfilerCategory category, string name);
        private delegate ProfilerRecorder CreateRecorderFromHandleDelegate(ProfilerRecorderHandle statHandle, int maxSampleCount, ProfilerRecorderOptions options);

        static private GetHandleDelegate s_GetHandle;
        static private CreateRecorderFromHandleDelegate s_CreateRecorder;
#endif // RESTRICT_INTERNAL_CALLS

        /// <summary>
        /// Gets the ProfilerRecorderHandle for the given metric description.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
#if !RESTRICT_INTERNAL_CALLS
        [IntrinsicIL("ldarg.0; ldfld [arg metric]::Category; ldarg.0; ldfld [arg metric]::Name; call Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle::GetByName(Unity.Profiling.ProfilerCategory,string); ret")]
#endif // !RESTRICT_INTERNAL_CALLS
        static public ProfilerRecorderHandle GetHandle(PerfMetric metric) { 
#if RESTRICT_INTERNAL_CALLS
            return s_GetHandle?.Invoke(metric.Category, metric.Name) ?? default;
#else
            throw new NotImplementedException();
#endif // RESTRICT_INTERNAL_CALLS
        }

        /// <summary>
        /// Creates a new ProfilerRecorder from the given metric.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public ProfilerRecorder CreateRecorder(PerfMetric metric, int capacity = 1, ProfilerRecorderOptions options = ProfilerRecorderOptions.Default) {
            return new ProfilerRecorder(metric.Category, metric.Name, capacity, options);
        }

        /// <summary>
        /// Creates a new ProfilerRecorder from the given handle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
#if !RESTRICT_INTERNAL_CALLS
        [IntrinsicIL("ldarg.0; ldarg.1; ldarg.2; call Unity.Profiling.ProfilerRecorder::Create(Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle,int32,Unity.Profiling.ProfilerRecorderOptions); ret")]
#endif // !RESTRICT_INTERNAL_CALLS
        static public ProfilerRecorder CreateRecorder(ProfilerRecorderHandle handle, int capacity = 1, ProfilerRecorderOptions options = ProfilerRecorderOptions.Default) {
#if RESTRICT_INTERNAL_CALLS
            return s_CreateRecorder?.Invoke(handle, capacity, options) ?? default;
#else
            throw new NotImplementedException();
#endif // RESTRICT_INTERNAL_CALLS
        }

        #endregion // Internal Calls
    }
}