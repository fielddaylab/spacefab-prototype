#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

using BeauUtil;
using FieldDay.Collections;
using FieldDay.Components;
using FieldDay.SharedState;
using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Scripting;

namespace FieldDay {
    /// <summary>
    /// Static ECS api.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    static public class ECS {
        #region SharedState

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void GetState<TShared>(out TShared a)
            where TShared : class, ISharedState {
            a = Find.State<TShared>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void GetState<TSharedA, TSharedB>(out TSharedA a, out TSharedB b)
            where TSharedA : class, ISharedState
            where TSharedB : class, ISharedState {
            a = Find.State<TSharedA>();
            b = Find.State<TSharedB>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void GetState<TSharedA, TSharedB, TSharedC>(out TSharedA a, out TSharedB b, out TSharedC c)
            where TSharedA : class, ISharedState
            where TSharedB : class, ISharedState
            where TSharedC : class, ISharedState {
            a = Find.State<TSharedA>();
            b = Find.State<TSharedB>();
            c = Find.State<TSharedC>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void GetState<TSharedA, TSharedB, TSharedC, TSharedD>(out TSharedA a, out TSharedB b, out TSharedC c, out TSharedD d)
            where TSharedA : class, ISharedState
            where TSharedB : class, ISharedState
            where TSharedC : class, ISharedState
            where TSharedD : class, ISharedState {
            a = Find.State<TSharedA>();
            b = Find.State<TSharedB>();
            c = Find.State<TSharedC>();
            d = Find.State<TSharedD>();
        }

        #endregion // SharedState

        #region Components

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public ComponentIterator<TComponent> GetComponents<TComponent>()
            where TComponent : class, IComponentData {
            return Find.Components<TComponent>();
        }

        #endregion // Components
    }
}