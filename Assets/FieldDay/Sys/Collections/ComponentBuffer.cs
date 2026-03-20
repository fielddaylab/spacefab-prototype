using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Runtime.CompilerServices;
using TinyIL;
using Unity.IL2CPP.CompilerServices;

namespace FieldDay.Collections {
    public struct TempComponentBuffer<T> : IWorkList<T>, IDisposable
        where T : class
    {
        private WorkList<object> m_PooledList;
        private IPool<WorkList<object>> m_Pool;

        private TempComponentBuffer(IPool<WorkList<object>> pool) {
            m_Pool = pool;
            m_PooledList = pool.Alloc();
        }

        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ref PooledObjectWorkList.CastRef<T>(ref m_PooledList[index]); }
        }

        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_PooledList?.Count ?? 0; }
        }

        public int Capacity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_PooledList?.Capacity ?? 0; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) {
            Assert.NotNull(m_PooledList);
            m_PooledList.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            m_PooledList?.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CleanUp() {
            m_PooledList?.CleanUp();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            m_PooledList?.CleanUp();
            m_Pool?.Free(m_PooledList);
            m_PooledList = null;
            m_Pool = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity) {
            Assert.NotNull(m_PooledList);
            m_PooledList.EnsureCapacity(capacity);
        }

        static public TempComponentBuffer<T> Create() {
            return new TempComponentBuffer<T>(PooledObjectWorkList.GetPoolForCapacity(0));
        }

        static public TempComponentBuffer<T> Create(int capacity) {
            return new TempComponentBuffer<T>(PooledObjectWorkList.GetPoolForCapacity(capacity));
        }
    }

    [Il2CppEagerStaticClassConstruction]
    static internal class PooledObjectWorkList {
        private const int SmallThreshold = 64;
        private const int SmallSize = 64;
        private const int LargeSize = 512;

        static private IPool<WorkList<object>> s_SmallWorkLists;
        static private IPool<WorkList<object>> s_LargeWorkLists;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; ret;")]
        static internal ref T CastRef<T>(ref object value) where T : class {
            throw new NotImplementedException();
        }

        static internal void Initialize() {
            s_SmallWorkLists = new FixedPool<WorkList<object>>(8, (p) => new WorkList<object>(SmallSize));
            s_LargeWorkLists = new FixedPool<WorkList<object>>(8, (p) => new WorkList<object>(LargeSize));

            s_SmallWorkLists.Prewarm(4);
            s_LargeWorkLists.Prewarm(1);
        }

        static internal void Shutdown() {
            s_SmallWorkLists.Dispose();
            s_LargeWorkLists.Dispose();
        }

        static internal IPool<WorkList<object>> GetPoolForCapacity(int capacity) {
            if (capacity <= SmallThreshold) {
                return s_SmallWorkLists;
            }
            return s_LargeWorkLists;
        }
    }
}