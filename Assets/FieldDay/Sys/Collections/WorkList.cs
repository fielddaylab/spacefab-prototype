using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace FieldDay.Collections {
    public interface IWorkList<T> {
        void Add(T item);
        void Clear();
        void CleanUp();
        void EnsureCapacity(int capacity);
        int Count { get; }
        int Capacity { get; }
        ref T this[int index] { get; }
    }

    [Il2CppEagerStaticClassConstruction]
    public sealed class WorkList<T> : IWorkList<T> {
        private T[] m_Array;
        private int m_Count;

        public WorkList() {
            m_Array = Array.Empty<T>();
            m_Count = 0;
        }

        public WorkList(int capacity) {
            m_Array = new T[capacity];
            m_Count = 0;
        }

        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                Assert.True(index >= 0 && index < m_Count, "Index out of range");
                return ref m_Array[index];
            }
        }

        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Count; }
        }

        public int Capacity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Array.Length; }
        }

        public void Add(T item) {
            EnsureCapacity(m_Count + 1);
            m_Array[m_Count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            m_Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CleanUp() {
            Array.Clear(m_Array, 0, m_Array.Length);
            m_Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity) {
            if (m_Array.Length < capacity) {
                Array.Resize(ref m_Array, Mathf.Max(4, Mathf.NextPowerOfTwo(capacity)));
            }
        }
    }
}