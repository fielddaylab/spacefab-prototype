using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace FieldDay.Components {
    /// <summary>
    /// IComponentData type index mapping.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    static internal class ComponentIndex {
        public const int Capacity = HierarchicalTypeIndexTable.Capacity;

        static private HierarchicalTypeIndexTable Table = new HierarchicalTypeIndexTable(typeof(IComponentData));

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
}