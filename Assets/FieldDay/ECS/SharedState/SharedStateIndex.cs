using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Components;
using FieldDay.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace FieldDay.SharedState {
    /// <summary>
    /// ISharedState type index mapping.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    static internal class SharedStateIndex {
        public const int Capacity = TypeIndexTable.Capacity;

        static private TypeIndexTable Table = new TypeIndexTable(typeof(ISharedState));

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