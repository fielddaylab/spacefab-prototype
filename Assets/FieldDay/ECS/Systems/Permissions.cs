#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

#if DEVELOPMENT && UNITY_EDITOR
#define ECS_VALIDATE_SYSTEM_PERMISSIONS
#endif // DEVELOPMENT && UNITY_EDITOR

#if !DEVELOPMENT
#undef ECS_VALIDATE_SYSTEM_PERMISSIONS
#endif // !DEVELOPMENT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Components;
using FieldDay.SharedState;
using UnityEngine.Scripting;

namespace FieldDay.Systems {
    /// <summary>
    /// System permission information.
    /// </summary>
    public struct SysPermissions {
        /// <summary>
        /// Contains system conflicts.
        /// </summary>
        internal struct ConflictSet {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            public const int Capacity = 2047;
            private const int OverflowBit = 1 << 30;

            private int m_Count;
            private unsafe fixed uint m_Packed[Capacity];

            public unsafe void AddConflict(Conflict conflict) {
                if (m_Count >= Capacity) {
                    m_Count |= OverflowBit;
                    return;
                }

                uint packed = *(uint*)&conflict;
                m_Packed[m_Count++] = packed;
            }

            /// <summary>
            /// Sorts the list of conflicts.
            /// </summary>
            public unsafe void SortConflicts() {
                fixed(uint* buffer = m_Packed) {
                    Unsafe.Quicksort(buffer, Count);
                }
            }

            /// <summary>
            /// Resets the number of conflicts.
            /// </summary>
            public void Reset() {
                m_Count = 0;
            }

            /// <summary>
            /// Returns the number of conflicts.
            /// </summary>
            public readonly int Count {
                get { return m_Count & ~OverflowBit; }
            }

            /// <summary>
            /// Returns if the number of conflicts was more than the maximum capacity.
            /// </summary>
            public readonly bool Overflowed() {
                return (m_Count & OverflowBit) != 0;
            }

            /// <summary>
            /// Retrieves the conflict at the given index.
            /// </summary>
            public unsafe readonly void GetConflict(int index, out Conflict conflict) {
                Assert.True(index >= 0 && index < Count);
                uint packed = m_Packed[index];
                conflict = *(Conflict*)&packed;
            }
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Conflict {
            [FieldOffset(0)] public ConflictType Type;
            [FieldOffset(1)] public ConflictFlags Flags;
            [FieldOffset(2)] public ushort TypeIndex;
        }

        internal enum ConflictType : byte {
            /// <summary>
            /// System A reads while System B writes.
            /// </summary>
            ReadDuringWrite,

            /// <summary>
            /// System A writes while System B reads.
            /// </summary>
            WriteDuringRead,

            /// <summary>
            /// System A and B write simultaneously.
            /// </summary>
            WriteDuringWrite,
        }

        internal enum ConflictFlags : byte {
            IsSharedState = 0x01,
            IsCustomArea = 0x02
        }

#if ECS_VALIDATE_SYSTEM_PERMISSIONS
        internal BitSet512 ReadComponentMask;
        internal BitSet512 WriteComponentMask;
        internal BitSet512 ReadSharedMask;
        internal BitSet512 WriteSharedMask;
        internal BitSet64 ReadAreaMask;
        internal BitSet64 WriteAreaMask;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS

        #region Components

        /// <summary>
        /// The system will read data from the given component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions Read<TComponent>() where TComponent : UnityEngine.Object, IComponentData {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            ReadComponentMask.Set(ComponentIndex.Get<TComponent>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will write data to the given component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions Write<TComponent>() where TComponent : UnityEngine.Object, IComponentData {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            WriteComponentMask.Set(ComponentIndex.Get<TComponent>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will read and write data to the given component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadWrite<TComponent>() where TComponent : UnityEngine.Object, IComponentData {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            int index = ComponentIndex.Get<TComponent>();
            ReadComponentMask.Set(index);
            WriteComponentMask.Set(index);
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        #endregion // Components

        #region SharedState

        /// <summary>
        /// The system will read data from the given SharedState.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadShared<TShared>() where TShared : ISharedState {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            ReadSharedMask.Set(SharedStateIndex.Get<TShared>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will write data to the given SharedState.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions WriteShared<TShared>() where TShared : ISharedState {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            WriteSharedMask.Set(SharedStateIndex.Get<TShared>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will read and write data to the given SharedState.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadWriteShared<TShared>() where TShared : ISharedState {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            int index = SharedStateIndex.Get<TShared>();
            ReadSharedMask.Set(index);
            WriteSharedMask.Set(index);
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        #endregion // SharedState

        #region Custom

        /// <summary>
        /// The system will read data from the given custom area.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadArea(int areaIndex) {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            ReadAreaMask.Set(areaIndex);
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will write data to the given custom area.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions WriteArea(int areaIndex) {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            WriteAreaMask.Set(areaIndex);
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will read and write data to the given SharedState.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadWriteArea(int areaIndex) {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            ReadAreaMask.Set(areaIndex);
            WriteAreaMask.Set(areaIndex);
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        #endregion // Custom

        /// <summary>
        /// Checks if the given permission set overlaps.
        /// </summary>
        static internal unsafe int CheckForConflicts(in SysPermissions a, in SysPermissions b, ref ConflictSet conflictSet) {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            conflictSet.Reset();
            
            if (!HasPotentialConflicts(a, b)) {
                return 0;
            }

            int maxComponent = ComponentIndex.Count;
            for(int i = 0; i < maxComponent; i++) {
                if (a.ReadComponentMask.IsSet(i)) {
                    if (b.WriteComponentMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.ReadDuringWrite,
                            Flags = 0,
                            TypeIndex = (ushort) i
                        });
                    }
                }
                if (a.WriteComponentMask.IsSet(i)) {
                    if (b.WriteComponentMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.WriteDuringWrite,
                            Flags = 0,
                            TypeIndex = (ushort)i
                        });
                    }
                    if (b.ReadComponentMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.WriteDuringRead,
                            Flags = 0,
                            TypeIndex = (ushort)i
                        });
                    }
                }
            }

            int maxSharedState = SharedStateIndex.Count;
            for(int i = 0; i < maxSharedState; i++) {
                if (a.ReadSharedMask.IsSet(i)) {
                    if (b.WriteSharedMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.ReadDuringWrite,
                            Flags = ConflictFlags.IsSharedState,
                            TypeIndex = (ushort)i
                        });
                    }
                }
                if (a.WriteSharedMask.IsSet(i)) {
                    if (b.WriteSharedMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.WriteDuringWrite,
                            Flags = ConflictFlags.IsSharedState,
                            TypeIndex = (ushort)i
                        });
                    }
                    if (b.ReadSharedMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.WriteDuringRead,
                            Flags = ConflictFlags.IsSharedState,
                            TypeIndex = (ushort)i
                        });
                    }
                }
            }

            int maxCustom = 64;
            for (int i = 0; i < maxCustom; i++) {
                if (a.ReadAreaMask.IsSet(i)) {
                    if (b.WriteAreaMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.ReadDuringWrite,
                            Flags = ConflictFlags.IsCustomArea,
                            TypeIndex = (ushort)i
                        });
                    }
                }
                if (a.WriteAreaMask.IsSet(i)) {
                    if (b.WriteAreaMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.WriteDuringWrite,
                            Flags = ConflictFlags.IsCustomArea,
                            TypeIndex = (ushort)i
                        });
                    }
                    if (b.ReadAreaMask.IsSet(i)) {
                        conflictSet.AddConflict(new Conflict() {
                            Type = ConflictType.WriteDuringRead,
                            Flags = ConflictFlags.IsCustomArea,
                            TypeIndex = (ushort)i
                        });
                    }
                }
            }

            if (conflictSet.Count > 0) {
                conflictSet.SortConflicts();
            }

            return conflictSet.Count;
#else
            conflictSet = default;
            return 0;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        static private bool HasPotentialConflicts(in SysPermissions a, in SysPermissions b) {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            return (bool) (a.ReadComponentMask & b.WriteComponentMask)
                | (bool)(a.WriteComponentMask & (b.ReadComponentMask | b.WriteSharedMask))
                | (bool)(a.ReadSharedMask & b.WriteSharedMask)
                | (bool)(a.WriteSharedMask & (b.ReadSharedMask | b.WriteSharedMask))
                | (bool)(a.ReadAreaMask & b.WriteAreaMask)
                | (bool)(a.WriteAreaMask & (b.ReadAreaMask | b.WriteAreaMask));
#else
            return false;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }
    }
}