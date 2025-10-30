#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Debugging;
using Unity.Profiling;
using UnityEngine;

namespace FieldDay.Perf {
    public sealed class PerformanceMgr {
        private const int BufferSize =
#if DEVELOPMENT
            120;
#else
            5;
#endif // UNITY_EDITOR
        private readonly RingBuffer<PhaseTimingData> m_TimingBuffer;

        internal PerformanceMgr() {
            m_TimingBuffer = new RingBuffer<PhaseTimingData>(BufferSize, RingBufferMode.Overwrite);
            PerfMetric.Initialize();

            GameLoop.OnDebugUpdate.Register(OnDebugUpdate);

            if (PerfUtility.IsSecureContext()) {
                UnityEngine.Debug.Log("[PerformanceMgr] Running in a secure context!");
            } else {
                UnityEngine.Debug.LogWarning("[PerformanceMgr] Not running in a secure context");
            }
        }

        internal void Shutdown() {
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
                double framePerf = desiredFrameDuration / frameDuration;

                if (totalTicks > 0) {
                    Color color = Color.white;
                    if (framePerf > 1.2) {
                        color = Color.green;
                    } else if (framePerf < 0.80) {
                        color = Color.red;
                    } else if (framePerf < 0.999) {
                        color = Color.yellow;
                    }

                    using (var psb = PooledStringBuilder.CreateLarge()) {
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
            DisplayLastFrameStats
        }

#if DEVELOPMENT

        static private int s_FrameSeek = 1;

        static private readonly int[] s_Framerates = new int[] {
            -1,
            20,
            30,
            40,
            60,
            90,
            120
        };

        static private readonly string[] s_FramerateStrings = new string[] {
            "Automatic",
            "20",
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
            info.AddSelector("Target Framerate", () => Application.targetFrameRate, (i) => GameLoop.SetTargetFramerate(i), s_Framerates, s_FramerateStrings);

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
            info.AddSlider("Frame Selection", () => s_FrameSeek, (f) => s_FrameSeek = (int) f, 1, BufferSize, 1, "{0}", () => DebugFlags.IsFlagSet(DebuggingFlags.DisplayLastFrameStats), 1);

            return info;
        }

#endif // DEVELOPMENT

#endregion // Debugging
    }

    public unsafe struct PhaseTimingData {
        public fixed uint Duration[PhaseBuckets.MaxBuckets];
        public uint TotalDuration;
    }
}