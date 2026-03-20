#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define GENERATE_ENUM_INSPECTOR_NAMES 
#endif

using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FieldDay {
    /// <summary>
    /// Reflection cache.
    /// </summary>
    static public class ReflectionCache {
        /// <summary>
        /// Cached enum information.
        /// </summary>
        public struct EnumInfoCache {
            public object[] Values;
#if GENERATE_ENUM_INSPECTOR_NAMES
            public string[] InspectorNames;
#endif // GENERATE_ENUM_INSPECTOR_NAMES
        }

        static private readonly Dictionary<Type, EnumInfoCache> s_CachedEnumInfo = new Dictionary<Type, EnumInfoCache>(4);

        #region Assemblies

        /// <summary>
        /// Array of all user assemblies.
        /// </summary>
        static public IEnumerable<Assembly> UserAssemblies {
            get {
                return Reflect.FindAllUserAssemblies();
            }
        }

        #endregion // Assemblies

        #region Enums

        static public EnumInfoCache EnumInfo<T>() {
            return EnumInfo(typeof(T));
        }

        static public EnumInfoCache EnumInfo(Type enumType) {
            EnumInfoCache cache;
            if (!s_CachedEnumInfo.TryGetValue(enumType, out cache)) {
                List<object> values = new List<object>();
#if GENERATE_ENUM_INSPECTOR_NAMES
                List<string> names = new List<string>();
#endif // GENERATE_ENUM_INSPECTOR_NAMES
                foreach (var field in enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
                    if (field.IsDefined(typeof(HiddenAttribute)) || field.IsDefined(typeof(ObsoleteAttribute))) {
                        continue;
                    }

#if GENERATE_ENUM_INSPECTOR_NAMES
                    LabelAttribute label = (LabelAttribute) field.GetCustomAttribute(typeof(LabelAttribute));
                    string name;
                    if (label != null) {
                        name = label.Name;
                    } else {
                        name = InspectorName(field.Name);
                    }
#endif // GENERATE_ENUM_INSPECTOR_NAMES

                    object value = field.GetValue(null);

                    values.Add(value);

#if GENERATE_ENUM_INSPECTOR_NAMES
                    names.Add(name);
#endif // GENERATE_ENUM_INSPECTOR_NAMES
                }

                cache.Values = values.ToArray();
#if GENERATE_ENUM_INSPECTOR_NAMES
                cache.InspectorNames = names.ToArray();
#endif // GENERATE_ENUM_INSPECTOR_NAMES
                s_CachedEnumInfo.Add(enumType, cache);
            }
            return cache;
        }

        #endregion // Enums

        #region String

        /// <summary>
        /// Returns the nicified name for the given field/type name.
        /// </summary>
        static public unsafe string InspectorName(string name) {
            char* buff = stackalloc char[name.Length * 2];
            bool wasUpper = true, isUpper;
            int charsWritten = 0;

            int i = 0;
            if (name.Length > 1) {
                char first = name[0];
                if (first == '_') {
                    i = 1;
                } else if (first == 'm' || first == 's' || first == 'k') {
                    char second = name[1];
                    if (second == '_' || char.IsUpper(second)) {
                        i = 2;
                    }
                }
            }

            for (; i < name.Length; i++) {
                char c = name[i];
                isUpper = char.IsUpper(c);
                if (isUpper && !wasUpper && charsWritten > 0) {
                    buff[charsWritten++] = ' ';
                }
                buff[charsWritten++] = c;

                wasUpper = isUpper;
            }

            return new string(buff, 0, charsWritten);
        }

        /// <summary>
        /// Returns the analytics-style name for the given name.
        /// This makes all characters uppercase and places underscores
        /// where word breaks would occur in the original string.
        /// </summary>
        static public unsafe string AnalyticsNameUpper(string name) {
            char* buff = stackalloc char[name.Length * 2];
            bool wasUpper = true, isUpper;
            int charsWritten = 0;

            int i = 0;
            if (name.Length > 1) {
                char first = name[0];
                if (first == '_') {
                    i = 1;
                } else if (first == 'm' || first == 's' || first == 'k') {
                    char second = name[1];
                    if (second == '_' || char.IsUpper(second)) {
                        i = 2;
                    }
                }
            }

            for (; i < name.Length; i++) {
                char c = name[i];
                isUpper = char.IsUpper(c);
                if (char.IsWhiteSpace(c)) {
                    buff[charsWritten++] = '_';
                } else {
                    if (isUpper && !wasUpper && charsWritten > 0) {
                        buff[charsWritten++] = '_';
                    }
                    buff[charsWritten++] = StringUtils.ToUpperInvariant(c);
                }

                wasUpper = isUpper;
            }

            return new string(buff, 0, charsWritten);
        }

        /// <summary>
        /// Returns the analytics-style name for the given name.
        /// This formats similarly to InspectorName but without spaces
        /// </summary>
        static public unsafe string AnalyticsNamePascal(string name) {
            char* buff = stackalloc char[name.Length * 2];
            bool wasUpper = true, isUpper;
            int charsWritten = 0;

            int i = 0;
            if (name.Length > 1) {
                char first = name[0];
                if (first == '_') {
                    i = 1;
                } else if (first == 'm' || first == 's' || first == 'k') {
                    char second = name[1];
                    if (second == '_' || char.IsUpper(second)) {
                        i = 2;
                    }
                }
            }

            for (; i < name.Length; i++) {
                char c = name[i];
                isUpper = char.IsUpper(c);
                //if (isUpper && !wasUpper && charsWritten > 0) {
                //    buff[charsWritten++] = ' ';
                //}
                if (!char.IsWhiteSpace(c)) {
                    buff[charsWritten++] = c;
                }

                wasUpper = isUpper;
            }

            return new string(buff, 0, charsWritten);
        }

        #endregion // String
    }
}