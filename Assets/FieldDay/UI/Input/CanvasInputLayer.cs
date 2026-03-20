using BeauUtil;
using FieldDay.Assets;
using FieldDay.Components;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FieldDay.UI {
    [DisallowMultipleComponent, RequireComponent(typeof(Canvas))]
    public sealed class CanvasInputLayer : BaseRaycasterInputLayer {
        [NonSerialized] private Canvas m_Canvas;

        protected override void Awake() {
            base.Awake();

            this.CacheComponent(ref m_Canvas);
            m_Mask.SortKey = CanvasSortKey.Create(m_Canvas);
        }
    }
}