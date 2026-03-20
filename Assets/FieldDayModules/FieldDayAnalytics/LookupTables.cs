using BeauUtil;
using System;
using System.Diagnostics;
using UnityEngine;

namespace FieldDay.Analytics {
    static public class AnalyticsLookup {
    }

    public struct EnumLookupTable<T> where T : unmanaged, Enum {
        public string[] Names;
    }

    public struct FlagEnumLookupTable<T> where T : unmanaged, Enum {
        public string[] Names;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class PreserveLookupNameAttribute : Attribute {
        public string NameField;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class PreserveLookupFieldAttribute : Attribute {
    }
}