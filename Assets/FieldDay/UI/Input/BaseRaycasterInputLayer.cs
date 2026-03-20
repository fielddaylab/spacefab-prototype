using BeauUtil;
using FieldDay.Assets;
using FieldDay.Components;
using ScriptableBake;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FieldDay.UI {
    [DisallowMultipleComponent]
    public abstract class BaseRaycasterInputLayer : BatchedComponent, IInputLayer, IRegistrationCallbacks {
        #region Inspector

        [SerializeField, InputGroupName] private StringHash32 m_GroupId;
        [SerializeField] private BaseRaycaster[] m_Raycasters;

        #endregion // Inspector

        [NonSerialized] protected InputLayerMask m_Mask;
        [NonSerialized] protected bool m_InputEnabled;

        protected virtual void Awake() {
            m_Mask.GroupId = m_GroupId;

            CacheRaycasters();
            if (m_Raycasters.Length > 0) {
                m_InputEnabled = m_Raycasters[0].enabled;
                for(int i = 1; i < m_Raycasters.Length; i++) {
                    m_Raycasters[i].enabled = m_InputEnabled;
                }
            }
        }

        void IRegistrationCallbacks.OnRegister() {
            Game.Gui.RegisterInputLayer(this);
        }

        void IRegistrationCallbacks.OnDeregister() {
            Game.Gui.DeregisterInputLayer(this);
        }

        public InputLayerMask InputMask {
            get { return m_Mask; }
            set { m_Mask = value; }
        }

        public bool IsInputEnabled() {
            return m_InputEnabled;
        }

        void IInputLayer.UpdateInputEnabled(bool enabled) {
            if (m_InputEnabled != enabled) {
                m_InputEnabled = enabled;
                for (int i = m_Raycasters.Length; i-- > 0;) {
                    m_Raycasters[i].enabled = enabled;
                }
            }
        }

        protected void CacheRaycasters() {
            if (m_Raycasters == null || m_Raycasters.Length == 0) {
                m_Raycasters = GetComponentsInChildren<BaseRaycaster>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (Application.IsPlaying(this)) {
                return;
            }

            m_Raycasters = GetComponentsInChildren<BaseRaycaster>();
        }

        [ContextMenu("Find Child Raycasters")]
        private void Reset() {
            m_Raycasters = GetComponentsInChildren<BaseRaycaster>();
            Baking.SetDirty(this);
        }
#endif // UNITY_EDITOR
    }
}