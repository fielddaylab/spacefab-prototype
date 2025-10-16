using BeauUtil;
using System.Runtime.CompilerServices;

namespace FieldDay {
    /// <summary>
    /// Pseudo-random number generation.
    /// </summary>
    public struct PseudoRandom {
        private const ulong Muliplier = 0xBC8F;
        private const ulong Addition = 0x3079;
        private const int Fold = 12;
        private const int FoldMask = 0xFFFFFF;
        private const float FloatMultiplier = 1f / FoldMask;

        public uint Seed;

        #region Constructors

        public PseudoRandom(uint seed) {
            Seed = seed;
        }

        public PseudoRandom(StringHash32 seed) {
            Seed = seed.HashValue;
        }

        #endregion // Constructors

        #region Basic

        /// <summary>
        /// Skips a given number of samples.
        /// </summary>
        public void Skip(int count) {
            while(count-- > 0) {
                Next();
            }
        }

        public int Next() {
            Seed = (uint)((Seed * Muliplier + Addition));
            return ((int)Seed >> Fold) & FoldMask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Float() {
            return Next() * FloatMultiplier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Bool() {
            return Float() < 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Bool(float chance) {
            return Float() < chance;
        }

        #endregion // Basic

        #region Ranges

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next(int range) {
            return Next() % range;
        }

        public int Range(int min, int max) {
            return min + Next() % (max - min);
        }

        public float Range(float min, float max) {
            return min + Float() * (max - min);
        }

        #endregion // Ranges

        #region Shuffle

        public unsafe void Shuffle<T>(UnsafeSpan<T> span) where T : unmanaged {
            Shuffle(span.Ptr, span.Length);
        }

        public unsafe void Shuffle<T>(T* ptr, int length) where T : unmanaged {
            int i = length, j;
            while (--i > 0) {
                T old = ptr[i];
                ptr[i] = ptr[j = Next(i + 1)];
                ptr[j] = old;
            }
        }

        public void Shuffle<T>(T[] array) where T : unmanaged {
            int i = array.Length, j;
            while (--i > 0) {
                T old = array[i];
                array[i] = array[j = Next(i + 1)];
                array[j] = old;
            }
        }

        public void Shuffle<T>(RingBuffer<T> buffer) where T : unmanaged {
            int i = buffer.Count, j;
            while (--i > 0) {
                T old = buffer[i];
                buffer[i] = buffer[j = Next(i + 1)];
                buffer[j] = old;
            }
        }

        #endregion // Shuffle
    }
}