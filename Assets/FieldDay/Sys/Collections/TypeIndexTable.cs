using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace FieldDay.Data {
    /// <summary>
    /// Minimal type table, without parent information.
    /// </summary>
    internal unsafe struct TypeIndexTable {
        public const int Capacity = 512;
        
        public readonly Type RootType;
        public readonly Dictionary<Type, int> TypeMap;
        public readonly Type[] IndexMap;
        public int Count;

        public TypeIndexTable(Type rootType) {
            RootType = rootType;
            TypeMap = new Dictionary<Type, int>(Capacity);
            IndexMap = new Type[Capacity];

            TypeMap[rootType] = 0;
            IndexMap[0] = RootType;
            Count = 1;
        }

        private int AllocateIndex(Type type) {
            if (Count >= Capacity) {
                Assert.Fail("Exceeded maximum number of type indices {0} for type '{1}'", Capacity, RootType.FullName);
                return -1;
            }

            if (!RootType.IsAssignableFrom(type)) {
                Assert.Fail("Attempting to allocate index for type '{0}' that does not inherit from the base type '{1}', and is not an interface with the [Indexed] attribute", type.FullName, RootType.FullName);
                return -1;
            }

            if (type.IsDefined((typeof(NonIndexedAttribute)), false)) {
                Assert.Fail("Attempting to allocate index for type '{0}' that is marked as NonIndexed", type.FullName);
                return -1;
            }

            lock (TypeMap) {
                int index = Count++;
                TypeMap.Add(type, index);
                IndexMap[index] = type;
                return index;
            }
        }

        /// <summary>
        /// Retrieves the index for the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public int Get(Type type) {
            if (!TypeMap.TryGetValue(type, out int index)) {
                index = AllocateIndex(type);
            }
            return index;
        }

        /// <summary>
        /// Retrieves the index for the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public Type Type(int index) {
            Assert.True(index >= 0 && index < Count, "Index {0} is out of mapped range 0-{1}", index, Count - 1);
            return IndexMap[index];
        }
    }

    /// <summary>
    /// Minimal type table.
    /// </summary>
    internal unsafe struct HierarchicalTypeIndexTable {
        public const int Capacity = 512;

        public readonly Type RootType;
        public readonly Dictionary<Type, int> TypeMap;
        public readonly Type[] IndexMap;
        public readonly short[] ParentMap;
        public int Count;

        public HierarchicalTypeIndexTable(Type rootType) {
            RootType = rootType;
            TypeMap = new Dictionary<Type, int>(Capacity);
            IndexMap = new Type[Capacity];
            ParentMap = new short[Capacity];
            
            TypeMap[rootType] = 0;
            IndexMap[0] = RootType;
            ParentMap[0] = -1;
            Count = 1;
        }

        private int AllocateIndex(Type type) {
            if (Count >= Capacity) {
                Assert.Fail("Exceeded maximum number of type indices {0} for type '{1}'", Capacity, RootType.FullName);
                return -1;
            }

            if (!RootType.IsAssignableFrom(type)) {
                if (!type.IsInterface || !type.IsDefined(typeof(IndexedAttribute), true)) {
                    Assert.Fail("Attempting to allocate index for type '{0}' that does not inherit from the base type '{1}', and is not an interface with the [Indexed] attribute", type.FullName, RootType.FullName);
                    return -1;
                }
            }

            if (type.IsDefined((typeof(NonIndexedAttribute)), false)) {
                Assert.Fail("Attempting to allocate index for type '{0}' that is marked as NonIndexed", type.FullName);
                return -1;
            }

            int parentIndex = AllocateClassHierarchy(type);

            lock (TypeMap) {
                int index = Count++;
                TypeMap.Add(type, index);
                IndexMap[index] = type;
                ParentMap[index] = (short) parentIndex;
                return index;
            }
        }

        /// <summary>
        /// Allocates the base type chain for the given type.
        /// </summary>
        private int AllocateClassHierarchy(Type type) {
            int parentIndex = -1;
            if (!type.IsInterface && !type.IsValueType) {
                Type parentType = type.BaseType;
                if (parentType != RootType && RootType.IsAssignableFrom(parentType) && !parentType.IsDefined(typeof(NonIndexedAttribute), false)) {
                    parentIndex = Get(parentType);
                }
            }

            return parentIndex;
        }

        /// <summary>
        /// Retrieves the index for the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public int Get(Type type) {
            if (!TypeMap.TryGetValue(type, out int index)) {
                index = AllocateIndex(type);
            }
            return index;
        }

        /// <summary>
        /// Retrieves the parent index for the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public int GetParent(int index) {
            Assert.True(index >= 0 && index < Count, "Index {0} is out of mapped range 0-{1}", index, Count - 1);
            return ParentMap[index];
        }

        /// <summary>
        /// Retrieves the index for the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public Type Type(int index) {
            Assert.True(index >= 0 && index < Count, "Index {0} is out of mapped range 0-{1}", index, Count - 1);
            return IndexMap[index];
        }
    }
}