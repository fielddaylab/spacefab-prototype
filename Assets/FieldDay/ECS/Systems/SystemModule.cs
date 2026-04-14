using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Collections;
using UnityEngine;

namespace FieldDay.Systems {
    /// <summary>
    /// Registers and deregisters all system behaviours in this hierarchy.
    /// </summary>
    [DefaultExecutionOrder(-21000), DisallowMultipleComponent]
    public abstract class SystemModule : MonoBehaviour {
        private SystemRegistrationTable m_RegisteredModules;

        protected abstract unsafe void RegisterSystems(ref SystemRegistrationTable ecs);

        private void Awake() {
            RegisterSystems(ref m_RegisteredModules);
        }

        private void OnDestroy() {
            if (Game.IsShuttingDown) {
                return;
            }

            m_RegisteredModules.Reset();
        }
    }

    /// <summary>
    /// System registration table.
    /// </summary>
    public struct SystemRegistrationTable {
        public const int Capacity = SystemsMgr.MaxSystems - 1;

        private short m_Count;
        private unsafe fixed ushort m_Packed[Capacity];

        public unsafe void Register(delegate*<float, void> systemFunction, in SysUpdate update, in SysPermissions permissions) {
            Assert.True(m_Count < Capacity, "SystemRegistrationTable has reached capacity " + Capacity);
            m_Packed[m_Count++] = Game.Systems.Register(systemFunction, in update, in permissions).Id;
        }

        public unsafe void Reset() {
            while(m_Count-- > 0) {
                Game.Systems.Deregister(new UniqueId16(m_Packed[m_Count]));
            }
        }
    }
}