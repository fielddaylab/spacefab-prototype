#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Debugging;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace FieldDay.Perf {
    public struct PerfMetric {
        public readonly ProfilerCategory Category;
        public readonly string Name;
        public readonly bool IsValid;
    }
}