#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

using BeauUtil;
using FieldDay.SharedState;
using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Scripting;

namespace FieldDay.Systems {
    /// <summary>
    /// System function pointer.
    /// </summary>
    public delegate void SystemFunction(float deltaTime);

    /// <summary>
    /// System execution flags.
    /// </summary>
    [Flags]
    public enum SysFlags : ushort {
        DuringLoading = 0x01
    }

    /// <summary>
    /// System update information.
    /// </summary>
    public struct SysUpdate {
        public GameLoopPhaseMask PhaseMask;
        public int Order;
        public int CategoryMask;
        public SysFlags Flags;

        public SysUpdate(GameLoopPhase phase, int order) {
            PhaseMask = (GameLoopPhaseMask)(1 << (int)phase);
            Order = order;
            CategoryMask = Bits.All32;
            Flags = SysFlags.DuringLoading;
        }

        public SysUpdate(GameLoopPhaseMask phaseMask, int order) {
            PhaseMask = phaseMask;
            Order = order;
            CategoryMask = Bits.All32;
            Flags = SysFlags.DuringLoading;
        }

        public SysUpdate RestrictDuringLoad() {
            return new SysUpdate() {
                PhaseMask = this.PhaseMask,
                Flags = this.Flags & ~SysFlags.DuringLoading,
                Order = this.Order,
                CategoryMask = this.CategoryMask
            };
        }

        public SysUpdate AllowDuringLoad() {
            return new SysUpdate() {
                PhaseMask = this.PhaseMask,
                Flags = this.Flags | SysFlags.DuringLoading,
                Order = this.Order,
                CategoryMask = this.CategoryMask
            };
        }

        public SysUpdate AllowDuringCategories(int categoryMask) {
            return new SysUpdate() {
                PhaseMask = this.PhaseMask,
                Flags = this.Flags,
                Order = this.Order,
                CategoryMask = categoryMask
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public SysUpdate Default(int order = 0) {
            return new SysUpdate(GameLoopPhase.Update, order);
        }
    }
}