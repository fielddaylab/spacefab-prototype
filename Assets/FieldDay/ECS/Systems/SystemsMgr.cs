#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

#if DEVELOPMENT && UNITY_EDITOR
#define ECS_VALIDATE_SYSTEM_PERMISSIONS
#endif // DEVELOPMENT && UNITY_EDITOR

#if !DEVELOPMENT
#undef ECS_VALIDATE_SYSTEM_PERMISSIONS
#endif // !DEVELOPMENT

using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Components;
using FieldDay.Debugging;
using FieldDay.SharedState;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TinyIL;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace FieldDay.Systems {
    /// <summary>
    /// Manages game system updates.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    public sealed class SystemsMgr {
        public const int MaxSystems = 128;

        private const ushort SystemFlag_IsLoading = 0x01;

        #region Types

        [Il2CppEagerStaticClassConstruction]
        private struct OrderedSystemReference { // 8 bytes
            public int Order;
            public UniqueId16 Id;

            static public unsafe readonly Unsafe.ComparisonPtr<OrderedSystemReference> Comparer = (a, b) => {
                if (a->Order < b->Order) {
                    return -1;
                } else if (a->Order > b->Order) {
                    return 1;
                }
                return 0;
            };

            static public unsafe readonly Predicate<OrderedSystemReference, UniqueId16> FindPredicate = (a, b) => a.Id == b;
        }

        private struct InternalSystemDefinition { // 12-16 bytes
#if ENABLE_IL2CPP
            public unsafe delegate*<float, void> Function;
#else
            public SystemFunction Function;
#endif // ENABLE_IL2CPP
            public int CategoryMask;
            public ushort PackedSystemMask;
            public ushort PackedPhaseMask;
        }

        #endregion // Types

        internal SystemsMgr() {
            m_Updates = new PhaseBuckets<OrderedSystemReference>(16);
            m_Updates.SetDefaultCapacity(GameLoopPhase.DebugUpdate, 4);
            m_Updates.SetDefaultCapacity(GameLoopPhase.FixedUpdate, 16);
            m_Updates.SetDefaultCapacity(GameLoopPhase.Update, 32);
            m_Updates.SetDefaultCapacity(GameLoopPhase.LateUpdate, 32);
        }

        #region System Lists

        private PhaseBuckets<OrderedSystemReference> m_Updates;
        private UniqueIdAllocator16 m_IdAllocator = new UniqueIdAllocator16(MaxSystems, false);

        static private InternalSystemDefinition[] s_SystemDefinitions = new InternalSystemDefinition[MaxSystems];
#if DEVELOPMENT
        static private string[] s_SystemDebugNames = new string[MaxSystems];
#endif // DEVELOPMENT
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
        static private SysPermissions[] s_SystemPermissions = new SysPermissions[MaxSystems];
        static private GameLoopPhaseMask s_QueuedPermissionReevaluationMask;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS

        /// <summary>
        /// Queues the given system for registration.
        /// </summary>
        public unsafe UniqueId16 Register(delegate*<float, void> systemFunction, in SysUpdate update, in SysPermissions permissions) {
            Assert.NotNull(systemFunction, "Cannot register null system");
            Assert.False(m_IdAllocator.InUse == MaxSystems, "Cannot allocate more than " + MaxSystems + " systems at a time!");
            Assert.True(update.PhaseMask != 0, "System must be allocated to at least one GameLoopPhase");

            UniqueId16 sysId = m_IdAllocator.Alloc();
            int sysIndex = sysId.Index;

#if DEVELOPMENT || !ENABLE_IL2CPP
            var functionAsDelegate = CreateDelegateFromPointer(systemFunction);
#endif // DEVELOPMENT || !ENABLE_IL2CPP

#if DEVELOPMENT
            {
                MethodInfo functionDebugInfo = functionAsDelegate.Method;
                s_SystemDebugNames[sysIndex] = functionDebugInfo.DeclaringType.FullName + "::" + functionDebugInfo.Name;
            }
#endif // DEVELOPMENT

#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            {
                s_SystemPermissions[sysIndex] = permissions;
            }
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS

            ref InternalSystemDefinition systemDef = ref s_SystemDefinitions[sysIndex];
#if ENABLE_IL2CPP
            systemDef.Function = systemFunction;
#else
            systemDef.Function = functionAsDelegate;
#endif // ENABLE_IL2CPP
            systemDef.PackedPhaseMask = PhaseBuckets.PackMask(update.PhaseMask);
            systemDef.PackedSystemMask = SysFlagsToMask(update.Flags);
            systemDef.CategoryMask = update.CategoryMask;

            foreach(var phase in new PhaseBuckets.PhaseEnumerator(update.PhaseMask)) {
                m_Updates[phase].PushBack(new OrderedSystemReference() {
                    Id = sysId,
                    Order = update.Order
                });
            }
            m_Updates.MarkBucketsDirty(update.PhaseMask);

#if DEVELOPMENT
            Log.Msg("[SystemsMgr] System '{0}' initialized", s_SystemDebugNames[sysIndex]);
#endif // DEVELOPMENT

            return sysId;
        }

        /// <summary>
        /// Immediately deregisters the given system.
        /// </summary>
        public unsafe void Deregister(UniqueId16 systemId) {
            Assert.True(systemId && m_IdAllocator.IsValid(systemId), "Out of date id");

            int sysIndex = systemId.Index;
            ref InternalSystemDefinition sysDef = ref s_SystemDefinitions[sysIndex];
            sysDef.Function = null;

            GameLoopPhaseMask registeredPhases = PhaseBuckets.UnpackMask(sysDef.PackedPhaseMask);
            foreach(var phase in new PhaseBuckets.PhaseEnumerator(registeredPhases)) {
                m_Updates[phase].RemoveWhere(OrderedSystemReference.FindPredicate, systemId);
            }
            m_Updates.MarkBucketsDirty(registeredPhases);

#if DEVELOPMENT
            Log.Msg("[SystemsMgr] System '{0}' shut down", s_SystemDebugNames[sysIndex]);
            s_SystemDebugNames[sysIndex] = default;
#endif // DEVELOPMENT

            m_IdAllocator.Free(systemId);
        }

        /// <summary>
        /// Indicates if a system is registered with the given id.
        /// </summary>
        public bool IsRegistered(UniqueId16 systemId) {
            return m_IdAllocator.IsValid(systemId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[IntrinsicIL("ldnull; ldarg.0; newobj FieldDay.Systems.SystemFunction::.ctor(object, intptr); ret")]
        static private unsafe SystemFunction CreateDelegateFromPointer(delegate*<float, void> delegatePtr) {
            return (SystemFunction)Activator.CreateInstance(typeof(SystemFunction), null, (IntPtr) delegatePtr);
        }

        #endregion // System Lists

        #region Validation

#if ECS_VALIDATE_SYSTEM_PERMISSIONS
        private void ValidateQueuedPermissionGroups() {
            if (s_QueuedPermissionReevaluationMask == 0) {
                return;
            }

            SysPermissions.ConflictSet conflictSet = default;
            int conflictCount = 0;
            using (Log.DisableErrorStackTrace()) {
                using (PooledStringBuilder psb = PooledStringBuilder.CreateLarge()) {
                    using (PooledStringBuilder subPsb = PooledStringBuilder.CreateLarge()) {
                        using (Profiling.Time("analyzing potential system conflicts", ProfileTimeUnits.Microseconds)) {
                            foreach (var bit in Bits.Enumerate<GameLoopPhaseMask, GameLoopPhase>(s_QueuedPermissionReevaluationMask)) {
                                int localConflicts = ValidatePermissionGroups(bit, GetSortedUpdatesForPhase(bit), subPsb, ref conflictSet);
                                conflictCount += localConflicts;
                                if (localConflicts > 0) {
                                    psb.Builder.Append("--- Phase ").Append(bit).Append(": ").Append(localConflicts).Append(" conflicts\n").Append(subPsb);
                                }
                                subPsb.Builder.Clear();
                            }
                        }
                        if (conflictCount > 0) {
                            Log.Error("Permissions Conflicts ({0}):\n{1}", conflictCount, psb.Builder.ToString());
                        }
                    }
                }
            }
            s_QueuedPermissionReevaluationMask = 0;

            Assert.True(conflictCount == 0, "{0} potential permissions conflicts found in registered systems - check console for details");
        }

        /// <summary>
        /// Validates that systems with the same order and overlapping category masks
        /// do not access data in a way such that a non-deterministic execution order
        /// would produce different results.
        /// </summary>
        static private unsafe int ValidatePermissionGroups(GameLoopPhase phase, RingBuffer<OrderedSystemReference> systems, StringBuilder logBuilder, ref SysPermissions.ConflictSet conflictSet) {
            ushort* groupBuffer = stackalloc ushort[MaxSystems];
            int groupBufferCount = 0;
            int conflictCount = 0;
            if (systems.Count > 1) {
                int prevOrder = systems[0].Order;
                foreach (var sysRef in systems) {
                    if (sysRef.Order != prevOrder) {
                        conflictCount += ValidatePermissionGroup(phase, groupBuffer, groupBufferCount, logBuilder, ref conflictSet);
                        groupBufferCount = 0;
                    }

                    groupBuffer[groupBufferCount++] = (ushort) sysRef.Id.Index;
                }

                conflictCount += ValidatePermissionGroup(phase, groupBuffer, groupBufferCount, logBuilder, ref conflictSet);
            }
            return conflictCount;
        }

        static private unsafe int ValidatePermissionGroup(GameLoopPhase phase, ushort* systemIndices, int systemIndexCount, StringBuilder logBuilder, ref SysPermissions.ConflictSet conflictSet) {
            if (systemIndexCount < 2) {
                return 0;
            }

            // We check every system with the same order with all other systems
            // Conflicts are bi-directional so we only need to check all unique pairs
            // Systems whose execution masks do not overlap can be skipped

            int conflictCount = 0;
            for(int a = 0; a < systemIndexCount - 1; a++) {
                int aIndex = systemIndices[a];
                InternalSystemDefinition aDef = s_SystemDefinitions[aIndex];
                ref SysPermissions aPermissions = ref s_SystemPermissions[aIndex];
                for(int b = a + 1; b < systemIndexCount; b++) {
                    int bIndex = systemIndices[b];
                    InternalSystemDefinition bDef = s_SystemDefinitions[bIndex];

                    // if execution category masks do not overlap, then skip
                    if ((aDef.CategoryMask & bDef.CategoryMask) == 0) {
                        continue;
                    }

                    ref SysPermissions bPermissions = ref s_SystemPermissions[bIndex];

                    int localConflicts = SysPermissions.CheckForConflicts(aPermissions, bPermissions, ref conflictSet);
                    if (localConflicts > 0) {
                        conflictCount += localConflicts;
                        string aName = s_SystemDebugNames[aIndex];
                        string bName = s_SystemDebugNames[bIndex];

                        for(int conflictIndex = 0; conflictIndex < localConflicts; conflictIndex++) {
                            conflictSet.GetConflict(conflictIndex, out SysPermissions.Conflict conflict);
                            Type dataConflictType = (conflict.Flags & SysPermissions.ConflictFlags.IsSharedState) != 0 ? SharedStateIndex.Type(conflict.TypeIndex) : ComponentIndex.Type(conflict.TypeIndex);
                            switch(conflict.Type) {
                                case SysPermissions.ConflictType.ReadDuringWrite: {
                                    logBuilder.Append(aName).Append(" reads data from ").Append(dataConflictType.FullName).Append(" while ").Append(bName).Append(" writes\n");
                                    break;
                                }
                                case SysPermissions.ConflictType.WriteDuringRead: {
                                    logBuilder.Append(aName).Append(" writes data to ").Append(dataConflictType.FullName).Append(" while ").Append(bName).Append(" reads\n");
                                    break;
                                }
                                case SysPermissions.ConflictType.WriteDuringWrite: {
                                    logBuilder.Append(aName).Append(" writes data to ").Append(dataConflictType.FullName).Append(" while ").Append(bName).Append(" writes\n");
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return conflictCount;
        }

#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS

        #endregion // Validation

        #region Events

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DebugUpdate(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.DebugUpdate, m_Updates[GameLoopPhase.DebugUpdate], m_Updates.PopBucketDirty(GameLoopPhase.DebugUpdate), deltaTime, categoryMask, isLoading);

#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            ValidateQueuedPermissionGroups();
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS

#if DEVELOPMENT
            RenderDebugInfo();
#endif // DEVELOPMENT
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PreUpdate(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.PreUpdate, m_Updates[GameLoopPhase.PreUpdate], m_Updates.PopBucketDirty(GameLoopPhase.PreUpdate), deltaTime, categoryMask, isLoading);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FixedUpdate(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.FixedUpdate, m_Updates[GameLoopPhase.FixedUpdate], m_Updates.PopBucketDirty(GameLoopPhase.FixedUpdate), deltaTime, categoryMask, isLoading);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LateFixedUpdate(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.LateFixedUpdate, m_Updates[GameLoopPhase.LateFixedUpdate], m_Updates.PopBucketDirty(GameLoopPhase.LateFixedUpdate), deltaTime, categoryMask, isLoading);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Update(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.Update, m_Updates[GameLoopPhase.Update], m_Updates.PopBucketDirty(GameLoopPhase.Update), deltaTime, categoryMask, isLoading);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnscaledUpdate(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.UnscaledUpdate, m_Updates[GameLoopPhase.UnscaledUpdate], m_Updates.PopBucketDirty(GameLoopPhase.UnscaledUpdate), deltaTime, categoryMask, isLoading);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LateUpdate(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.LateUpdate, m_Updates[GameLoopPhase.LateUpdate], m_Updates.PopBucketDirty(GameLoopPhase.LateUpdate), deltaTime, categoryMask, isLoading);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnscaledLateUpdate(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.UnscaledLateUpdate, m_Updates[GameLoopPhase.UnscaledLateUpdate], m_Updates.PopBucketDirty(GameLoopPhase.UnscaledLateUpdate), deltaTime, categoryMask, isLoading);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ApplicationPreRender(float deltaTime, int categoryMask, bool isLoading) {
            ProcessUpdates(GameLoopPhase.ApplicationPreRender, m_Updates[GameLoopPhase.ApplicationPreRender], m_Updates.PopBucketDirty(GameLoopPhase.ApplicationPreRender), deltaTime, categoryMask, isLoading);
        }

        internal void Shutdown() {
            m_Updates.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        static private unsafe void ProcessUpdates(GameLoopPhase phase, RingBuffer<OrderedSystemReference> systems, bool needsSort, float deltaTime, int categoryMask, bool isLoading) {
            if (needsSort) {
                systems.Quicksort(OrderedSystemReference.Comparer);
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
                s_QueuedPermissionReevaluationMask |= (GameLoopPhaseMask)(1 << (int)phase);
#endif // ECS_VALIDATE_SYSTEM_PERMISSION
            }

            ushort currentSystemMask = isLoading ? SystemFlag_IsLoading : (ushort) 0;

            int sysCount = systems.Count;
            for(int i = 0; i < sysCount; i++) {
                int sysIndex = systems[i].Id.Index;
                ref InternalSystemDefinition sys = ref s_SystemDefinitions[sysIndex];
#if DEVELOPMENT
                try {
                    if (((categoryMask & sys.CategoryMask) != 0) & ((currentSystemMask & sys.PackedSystemMask) == 0)) {
                        sys.Function(deltaTime);
                    }
                } catch (Exception e) {
                    Log.Error("[SystemsMgr] Encountered exception when processing system '{0}'", s_SystemDebugNames[sysIndex]);
                    Debug.LogException(e);
                }
#else
                if (((categoryMask & sys.CategoryMask) != 0) & ((currentSystemMask & sys.PackedSystemMask) == 0)) {
                    sys.Function(deltaTime);
                }
#endif // DEVELOPMENT
            }
        }

        #endregion // Events

        #region Masks

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private ushort SysFlagsToMask(SysFlags flags) {
            ushort mask = 0;
            if ((flags & SysFlags.ExecuteDuringLoad) == 0) {
                mask |= SystemFlag_IsLoading;
            }
            return mask;
        }

        #endregion // Masks

        #region Debug

        private enum DebuggingFlags {
            DisplayStats,
            DisplayOrder
        }

#if DEVELOPMENT

        static private int s_DebugBucketIndex;

        internal void RenderDebugInfo() {
            if (DebugFlags.IsFlagSet(DebuggingFlags.DisplayStats)) {
                using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    psb.Builder.Append("System Count: ").AppendNoAlloc(m_IdAllocator.InUse);
                    for(int i = 0; i < BucketList.Length; i++) {
                        psb.Builder.Append("\n   ").Append(BucketNames[i]).Append(": ").AppendNoAlloc(m_Updates[BucketList[i]].Count);
                    }
                    DebugDraw.AddLogText(psb, ColorBank.Azure);
                }
            }

            if (DebugFlags.IsFlagSet(DebuggingFlags.DisplayOrder)) {
                var systems = GetSortedUpdatesForPhase(BucketList[s_DebugBucketIndex]);
                bool isLoading = GameLoop.IsLoading;
                int categoryMask = GameLoop.UpdateMask;
                ushort currentSystemMask = isLoading ? SystemFlag_IsLoading : (ushort) 0;
                using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    psb.Builder.Append("Phase ").Append(BucketNames[s_DebugBucketIndex]).Append(": ").AppendNoAlloc(systems.Count);
                    if (systems.Count > 0) {
                        int prevOrder = systems[0].Order;
                        foreach (var sysRef in systems) {
                            if (sysRef.Order != prevOrder) {
                                psb.Builder.Append("\n ---");
                                prevOrder = sysRef.Order;
                            }
                            psb.Builder.Append('\n');
                            ref InternalSystemDefinition sys = ref s_SystemDefinitions[sysRef.Id.Index];
                            if (((categoryMask & sys.CategoryMask) != 0) & ((currentSystemMask & sys.PackedPhaseMask) == 0)) {
                                psb.Builder.Append("[X] ");
                            } else {
                                psb.Builder.Append("[ ] ");
                            }
                            psb.Builder.Append(s_SystemDebugNames[sysRef.Id.Index]);
                        }
                    }
                    DebugDraw.AddViewportText(new Vector2(0, 1), new Vector2(16, -16), psb, ColorBank.Wheat, 0, TextAnchor.UpperLeft, DebugTextStyle.BackgroundDark);
                }
            }
        }

        static private readonly GameLoopPhase[] BucketList = new GameLoopPhase[] {
            GameLoopPhase.DebugUpdate, GameLoopPhase.PreUpdate,
            GameLoopPhase.FixedUpdate, GameLoopPhase.LateFixedUpdate,
            GameLoopPhase.Update, GameLoopPhase.UnscaledUpdate,
            GameLoopPhase.LateUpdate, GameLoopPhase.UnscaledLateUpdate,
            GameLoopPhase.ApplicationPreRender,
        };

        static private readonly string[] BucketNames = new string[] {
            "Debug Update", "PreUpdate",
            "Fixed Update", "Late Fixed Update",
            "Update", "Unscaled Update",
            "Late Update", "Unscaled Late Update",
            "Application PreRender"
        };
        
        private RingBuffer<OrderedSystemReference> GetSortedUpdatesForPhase(GameLoopPhase phase) {
            RingBuffer<OrderedSystemReference> systems = m_Updates[phase];
            if (m_Updates.PopBucketDirty(phase)) {
                systems.Quicksort(OrderedSystemReference.Comparer);
            }
            return systems;
        }

        [EngineMenuFactory]
        static private DMInfo CreateSystemsDebugMenu() {
            DMInfo info = new DMInfo("ECS Systems", 24);
            info.SetMinWidth(300);
            
            DebugFlags.Menu.AddFlagToggle(info, "Display Stats", DebuggingFlags.DisplayStats);
            info.AddDivider();
            
            DMPredicate groupPredicate = () => DebugFlags.IsFlagSet(DebuggingFlags.DisplayOrder);
            DebugFlags.Menu.AddFlagToggle(info, "Display Order", DebuggingFlags.DisplayOrder);
            info.AddSelector("Bucket Selection", () => s_DebugBucketIndex, (f) => s_DebugBucketIndex = f, BucketNames, groupPredicate);
            return info;
        }

#endif // DEVELOPMENT

        #endregion // Debug
    }
}