using BeauRoutine.Extensions;
using BeauUtil;
using System;
using UnityEngine;

namespace FieldDay.UI {
    /// <summary>
    /// Shared panel.
    /// </summary>
    [DefaultExecutionOrder(SharedPanel.DefaultExecutionOrder)]
    [NonIndexed]
    public abstract class SharedPanel : MonoBehaviour, ISharedGuiPanel {
        public const int DefaultExecutionOrder = -100;

        [SerializeField, PanelGroupName] private StringHash32 m_PanelGroup;

        [NonSerialized] protected IInputLayer m_InputLayer;

        protected virtual void Awake() {
            m_InputLayer = IInputLayer.Find(this);
            Game.Gui.RegisterPanel(this);
        }

        protected virtual void OnDestroy() {
            if (!Game.IsShuttingDown) {
                Game.Gui.DeregisterPanel(this);
            }
        }

        #region ISharedGuiPanel

        public virtual Transform Root {
            get { return transform; }
        }

        public StringHash32 Group {
            get { return m_PanelGroup; }
        }

        public virtual void Hide() {
            gameObject.SetActive(false);
        }

        public virtual bool IsVisible() {
            return gameObject.activeInHierarchy;
        }

        public virtual bool IsShowing() {
            return gameObject.activeSelf;
        }

        public virtual void Show() {
            gameObject.SetActive(true);
        }

        public virtual void SetVisibleNow(bool visible) {
            gameObject.SetActive(visible);
        }

        public virtual bool IsTransitioning() {
            return false;
        }

        #endregion // ISharedGuiPanel
    }
}