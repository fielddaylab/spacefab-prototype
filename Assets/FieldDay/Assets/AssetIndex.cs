using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Data;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace FieldDay.Assets {
    /// <summary>
    /// IGlobalAsset type index mapping.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    static internal class GlobalAssetIndex {
        public const int Capacity = TypeIndexTable.Capacity;

        static private TypeIndexTable Table = new TypeIndexTable(typeof(IGlobalAsset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Get(Type type) {
            return Table.Get(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Get<T>() {
            return Cache<T>.Index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Type Type(int index) {
            return Table.Type(index);
        }

        static public int Count {
            get { return Table.Count; }
        }

        private struct Cache<U> {
            static public readonly int Index;

            static Cache() {
                Index = Table.Get(typeof(U));
            }
        }
    }

    /// <summary>
    /// INamedAsset type index mapping.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    static internal class NamedAssetIndex {
        public const int Capacity = HierarchicalTypeIndexTable.Capacity;

        static private HierarchicalTypeIndexTable Table = new HierarchicalTypeIndexTable(typeof(INamedAsset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Get(Type type) {
            return Table.Get(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Get<T>() {
            return Cache<T>.Index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int GetParent(int index) {
            return Table.GetParent(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Type Type(int index) {
            return Table.Type(index);
        }

        static public int Count {
            get { return Table.Count; }
        }

        private struct Cache<U> {
            static public readonly int Index;

            static Cache() {
                Index = Table.Get(typeof(U));
            }
        }
    }

    /// <summary>
    /// ILiteAsset type index mapping.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    static internal class LiteAssetIndex {
        public const int Capacity = TypeIndexTable.Capacity;

        static private TypeIndexTable Table = new TypeIndexTable(typeof(ILiteAsset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Get(Type type) {
            return Table.Get(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Get<T>() {
            return Cache<T>.Index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Type Type(int index) {
            return Table.Type(index);
        }

        static public int Count {
            get { return Table.Count; }
        }

        private struct Cache<U> {
            static public readonly int Index;

            static Cache() {
                Index = Table.Get(typeof(U));
            }
        }
    }
}