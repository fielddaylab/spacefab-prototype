#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Debugging;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

namespace FieldDay.Perf {
    public sealed class PerformanceMgr {
        private const int TimingBufferSize =
#if DEVELOPMENT
            60;
#else
            5;
#endif // UNITY_EDITOR

        private const int MetricBufferSize =
#if DEVELOPMENT
            16;
#else
            8;
#endif // UNITY_EDITOR

        private struct ActiveMetric {
            public PerfMetric Metric;
            public ProfilerRecorder Recorder;
            public ushort LastAccess;
        }

        private readonly RingBuffer<PhaseTimingData> m_TimingBuffer;
        private readonly RingBuffer<ActiveMetric> m_TemporaryMetrics;
        private readonly RingBuffer<ActiveMetric> m_ResidentMetrics;

        internal PerformanceMgr() {
            PerfMetric.Initialize();

            m_TimingBuffer = new RingBuffer<PhaseTimingData>(TimingBufferSize, RingBufferMode.Overwrite);
            m_TemporaryMetrics = new RingBuffer<ActiveMetric>(MetricBufferSize, RingBufferMode.Expand);
            m_ResidentMetrics = new RingBuffer<ActiveMetric>(MetricBufferSize, RingBufferMode.Expand);

#if DEVELOPMENT
            GameLoop.OnDebugUpdate.Register(OnDebugUpdate);
#endif // DEVELOPMENT

            if (PerfUtility.IsSecureContext()) {
                UnityEngine.Debug.Log("[PerformanceMgr] Running in a secure context!");
            } else {
                UnityEngine.Debug.LogWarning("[PerformanceMgr] Not running in a secure context");
            }
        }

        internal void Shutdown() {
            for(int i = m_TemporaryMetrics.Count; i-- > 0;) {
                m_TemporaryMetrics[i].Recorder.Dispose();
            }
            m_TemporaryMetrics.Clear();

            for (int i = m_ResidentMetrics.Count; i-- > 0;) {
                m_ResidentMetrics[i].Recorder.Dispose();
            }
            m_ResidentMetrics.Clear();

            m_TimingBuffer.Clear();

            PerfMetric.Shutdown();
        }

        private unsafe void OnDebugUpdate() {
#if DEVELOPMENT

            if (DebugFlags.IsFlagSet(DebuggingFlags.DisplayLastFrameStats) && m_TimingBuffer.Count > 0) {
                int frameIndex = Math.Max(0, m_TimingBuffer.Count - s_FrameSeek);
                PhaseTimingData timingData = m_TimingBuffer[frameIndex];

                uint totalTicks = timingData.TotalDuration;
                double desiredFrameDuration = PerfUtility.TargetFrameDurationMS();
                double frameDuration = Profiling.TicksToMillisecs(totalTicks);
                double framePerf = frameDuration / desiredFrameDuration;

                if (totalTicks > 0) {
                    Color color = Color.white;
                    if (framePerf <= 0.85) {
                        color = Color.green;
                    } else if (framePerf >= 1.25) {
                        color = Color.red;
                    } else if (framePerf >= 1.01) {
                        color = Color.yellow;
                    }

                    using (var psb = PooledStringBuilder.CreateLarge()) {

#if UNITY_WEBGL
                        if (!PerfUtility.IsSecureContext()) {
                            psb.Builder.Append("<color=#FFF600>!! CONTEXT IS NOT SECURE\n!! TIMINGS MAY BE INACCURATE</color>\n\n");
                        }
#endif // UNITY_WEBGL

                        psb.Builder.Append("Frame -").AppendNoAlloc(m_TimingBuffer.Count - frameIndex).Append(":\t")
                            .AppendNoAlloc(frameDuration, 2).Append("ms\t");
                        psb.Builder.AppendNoAlloc(100.0 * framePerf, 1).Append("%") 
                            .Append("\n");

                        for (int i = 0; i < PhaseBuckets.MaxBuckets; i++) {
                            double microsecs = Profiling.TicksToMicrosecs(timingData.Duration[i]);
                            double percent = 100 * timingData.Duration[i] / (double)totalTicks;
                            psb.Builder.Append("  ").Append(s_PhaseStrings[i]).Append(":\t").AppendNoAlloc(microsecs, 1).Append("us\t")
                                .AppendNoAlloc(percent, 1).Append("%\n");
                        }
                        psb.Builder.Length -= 1;

                        DebugDraw.AddLogText(psb.Builder, color);
                    }
                } else {
                    DebugDraw.AddLogText("Frame INVALID", Color.red);
                }
            }
#endif // DEVELOPMENT
        }

        #region Timing

        internal unsafe void RecordTiming(in PhaseTiming timing) {
            PhaseTimingData timingData;
            uint totalAccum = 0;
            for(int i = 0; i < PhaseBuckets.MaxBuckets; i++) {
                totalAccum += (timingData.Duration[i] = (uint) Math.Min(timing.Duration[i], uint.MaxValue));
            }
            timingData.TotalDuration = totalAccum;
            m_TimingBuffer.PushBack(timingData);
        }

        #endregion // Timing

        #region Debugging

        public enum DebuggingFlags {
            DisplayLastFrameStats,
        }

#if DEVELOPMENT

        static private int s_FrameSeek = 1;

        static private readonly int[] s_Framerates = new int[] {
            -1,
            20,
            24,
            30,
            40,
            60,
            90,
            120
        };

        static private readonly string[] s_FramerateStrings = new string[] {
            "Automatic",
            "20",
            "24",
            "30",
            "40",
            "60",
            "90",
            "120"
        };

        static private readonly string[] s_PhaseStrings = new string[] {
            "DbgUpd", "PreUpd", "FixedUpd", "LateFixedUpd",
            "Upd", "UnscUpd", "LateUpd", "UnscLateUpd",
            "CanvasPre", "RenderPre", "PreCull", "PreRender", "PostRender", "FrameAdv"
        };

        [EngineMenuFactory]
        static private DMInfo CreateDebugInfo() {
            DMInfo info = new DMInfo("Performance");
            info.AddSelector("Target Framerate",
                () => Application.targetFrameRate,
                (i) => GameLoop.SetTargetFramerate(i),
                s_Framerates, s_FramerateStrings);

            info.AddDivider();

            DMInfo metrics = new DMInfo("Metrics");

            metrics.AddButton("Dump Available Metrics", () => {
                using (Log.DisableMsgStackTrace()) {
                    int count = 0;
                    Log.Msg("[PerformanceMgr] Enumerating metrics...");
                    foreach (var metric in PerfMetric.EnumerateAvailableMetrics()) {
                        Log.Msg("{0} | {1} ({2}, {3}) [{4}]", metric.Category.Name, metric.Name, metric.DataType.ToString(), metric.UnitType.ToString(), metric.Flags.ToString());
                        count++;
                    }
                    Log.Msg("[PerformanceMgr] Found {0} metrics", count);
                }
            });

            info.AddSubmenu(metrics);

            info.AddDivider();

            DebugFlags.Menu.AddFlagToggle(info, "Display Frame Profiling Time", DebuggingFlags.DisplayLastFrameStats);
            info.AddSlider("Frame Selection",
                () => s_FrameSeek,
                (f) => s_FrameSeek = (int) f, 1, TimingBufferSize, 1, "{0}", () => DebugFlags.IsFlagSet(DebuggingFlags.DisplayLastFrameStats), 1);

            return info;
        }

#endif // DEVELOPMENT

        #endregion // Debugging

        #region Metrics

        internal ProfilerRecorder GetRecorder(PerfMetric metric) {
#if DEVELOPMENT
            for (int i = 0; i < m_TemporaryMetrics.Count; i++) {
                ref var metricTracker = ref m_TemporaryMetrics[i];
                if (metricTracker.Metric.HashId == metric.HashId) {
                    metricTracker.LastAccess = Frame.Index;
                    return metricTracker.Recorder;
                }
            }
            ProfilerRecorderHandle handle = PerfMetric.GetHandle(metric);
            if (handle.Valid) {
                ActiveMetric newMetric;
                newMetric.Metric = metric;
                newMetric.LastAccess = Frame.Index;
                newMetric.Recorder = PerfMetric.CreateRecorder(handle);
                newMetric.Recorder.Start();
                m_TemporaryMetrics.PushBack(newMetric);
                return newMetric.Recorder;
            }
#endif // DEVELOPMENT
            return default;
        }

        internal void CleanUpUnusedMetrics() {
#if DEVELOPMENT
            for(int i = m_TemporaryMetrics.Count; i-- > 0;) {
                if (Frame.Age(m_TemporaryMetrics[i].LastAccess) >= 3) {
                    m_TemporaryMetrics[i].Recorder.Dispose();
                    m_TemporaryMetrics.FastRemoveAt(i);
                }
            }
#endif // DEVELOPMENT
        }

        #endregion // Metrics
    }

    public unsafe struct PhaseTimingData {
        public fixed uint Duration[PhaseBuckets.MaxBuckets];
        public uint TotalDuration;
    }

    public partial struct PerfMetric {
        /// <summary>
        /// Requests a value be recorded by the Profiler.
        /// </summary>
        static public ProfilerRecorder Read(PerfMetric metric) {
            return Game.Perf.GetRecorder(metric);
        }

        /// <summary>
        /// Writes a metric and its current value to the given StringBuilder.
        /// </summary>
        static public void WriteMetric(StringBuilder output, PerfMetric metric) {
            output.Append(metric.DisplayName).Append(": ");
            FormatValue(output, Read(metric));
        }

        /// <summary>
        /// Writes a metric and its current value to the given StringBuilder.
        /// Also writes a newline character.
        /// </summary>
        static public void WriteMetricLine(StringBuilder output, PerfMetric metric) {
            WriteMetric(output, metric);
            output.Append('\n');
        }

        /// <summary>
        /// Writes a metric's current value to the given StringBuilder.
        /// </summary>
        static public void WriteMetricValue(StringBuilder output, PerfMetric metric) {
            FormatValue(output, Read(metric));
        }
    }
}