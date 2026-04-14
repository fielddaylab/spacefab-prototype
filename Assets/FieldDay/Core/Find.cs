using System.Runtime.CompilerServices;
using BeauUtil;
using FieldDay.Assets;
using FieldDay.Components;
using FieldDay.SharedState;
using FieldDay.UI;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace FieldDay {
    /// <summary>
    /// Object lookup shortcuts.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    public class Find {
        #region Assets

        /// <summary>
        /// Looks up the global asset of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public T GlobalAsset<T>() where T : class, IGlobalAsset {
            return Game.Assets.GetGlobal<T>();
        }

        /// <summary>
        /// Looks up the global asset of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void GlobalAsset<T0>(out T0 assetA)
            where T0 : class, IGlobalAsset {
            assetA = Game.Assets.GetGlobal<T0>();
        }

        /// <summary>
        /// Looks up the global assets of the given types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void GlobalAsset<T0, T1>(out T0 assetA, out T1 assetB)
            where T0 : class, IGlobalAsset
            where T1 : class, IGlobalAsset {
            assetA = Game.Assets.GetGlobal<T0>();
            assetB = Game.Assets.GetGlobal<T1>();
        }

        /// <summary>
        /// Looks up the global assets of the given types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void GlobalAsset<T0, T1, T2>(out T0 assetA, out T1 assetB, out T2 assetC)
            where T0 : class, IGlobalAsset
            where T1 : class, IGlobalAsset
            where T2 : class, IGlobalAsset {
            assetA = Game.Assets.GetGlobal<T0>();
            assetB = Game.Assets.GetGlobal<T1>();
            assetC = Game.Assets.GetGlobal<T2>();
        }

        /// <summary>
        /// Looks up the named asset of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public T NamedAsset<T>(StringHash32 id) where T : class, INamedAsset {
            return Game.Assets.GetNamed<T>(id);
        }

        /// <summary>
        /// Looks up the lite asset with the given id.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public T LiteAsset<T>(StringHash32 id) where T : struct, ILiteAsset {
            return Game.Assets.GetLite<T>(id);
        }

        #endregion // Assets

        #region Shared State

        /// <summary>
        /// Looks up the shared state of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public T State<T>() where T : class, ISharedState {
            return Game.SharedState.Get<T>();
        }

        /// <summary>
        /// Looks up the shared state of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void State<T0>(out T0 stateA) where T0 : class, ISharedState {
            stateA = Game.SharedState.Get<T0>();
        }

        /// <summary>
        /// Looks up the shared states of the given types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void State<T0, T1>(out T0 stateA, out T1 stateB)
            where T0 : class, ISharedState
            where T1 : class, ISharedState {
            stateA = Game.SharedState.Get<T0>();
            stateB = Game.SharedState.Get<T1>();
        }

        /// <summary>
        /// Looks up the shared states of the given types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void State<T0, T1, T2>(out T0 stateA, out T1 stateB, out T2 stateC)
            where T0 : class, ISharedState
            where T1 : class, ISharedState
            where T2 : class, ISharedState {
            stateA = Game.SharedState.Get<T0>();
            stateB = Game.SharedState.Get<T1>();
            stateC = Game.SharedState.Get<T2>();
        }

        /// <summary>
        /// Looks up the shared states of the given types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public void State<T0, T1, T2, T3>(out T0 stateA, out T1 stateB, out T2 stateC, out T3 stateD)
            where T0 : class, ISharedState
            where T1 : class, ISharedState
            where T2 : class, ISharedState
            where T3 : class, ISharedState {
            stateA = Game.SharedState.Get<T0>();
            stateB = Game.SharedState.Get<T1>();
            stateC = Game.SharedState.Get<T2>();
            stateD = Game.SharedState.Get<T3>();
        }

        #endregion // Shared State

        #region Gui

        /// <summary>
        /// Looks up the shared gui panel of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public T Panel<T>() where T : class, ISharedGuiPanel {
            return Game.Gui.GetShared<T>();
        }

        /// <summary>
        /// Looks up the gui module of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public T GuiModule<T>() where T : class, IGuiModule {
            return Game.Gui.GetModule<T>();
        }

        /// <summary>
        /// Looks up the named RectTransform of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public RectTransform NamedRectTransform(StringHash32 name) {
            return Game.Gui.FindNamed(name);
        }

        #endregion // Gui

        #region Components

        /// <summary>
        /// Looks up the list of components of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public ComponentIterator<T> Components<T>() where T : class, IComponentData {
            return Game.Components.ComponentsOfType<T>();
        }

        /// <summary>
        /// Looks up the first component of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public T FirstComponent<T>() where T : class, IComponentData {
            return Game.Components.FirstComponentOfType<T>();
        }

        #endregion // Components

        #region Unity

        /// <summary>
        /// Finds a Unity object from its asset id.
        /// </summary>
        static public UnityEngine.Object FromId(int instanceId) {
            return UnityHelper.Find(instanceId);
        }

        /// <summary>
        /// Finds a Unity object from its asset id.
        /// </summary>
        static public T FromId<T>(int instanceId) where T : UnityEngine.Object {
            return UnityHelper.Find<T>(instanceId);
        }

        /// <summary>
        /// Finds an instance of the given type.
        /// </summary>
        static public T Any<T>() where T : UnityEngine.Object {
            return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        }

        /// <summary>
        /// Finds an instance of the given type.
        /// </summary>
        static public T Any<T>(FindObjectsInactive findInactive) where T : UnityEngine.Object {
            return Object.FindAnyObjectByType<T>(findInactive);
        }

        #endregion // Unity
    }
}