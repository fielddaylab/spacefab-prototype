using BeauUtil;
using FieldDay.Assets;
using FieldDay.Components;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FieldDay.UI {
    [DisallowMultipleComponent, RequireComponent(typeof(Camera), typeof(BaseRaycaster))]
    public sealed class PhysicsInputLayer : BaseRaycasterInputLayer {
        #region Inspector

        [Header("Sorting")]
        [SerializeField, SortingLayer] private int m_SortingLayerId;
        [SerializeField] private short m_SortingOrder;

        #endregion // Inspector

        [NonSerialized] private Camera m_CachedCamera;

        protected override void Awake() {
            base.Awake();

            this.CacheComponent(ref m_CachedCamera);
            m_Mask.SortKey = CanvasSortKey.CreateForWorld(m_CachedCamera, m_SortingLayerId, m_SortingOrder);
        }
    }
}