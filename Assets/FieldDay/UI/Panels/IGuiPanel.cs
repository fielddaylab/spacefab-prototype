using System.Runtime.CompilerServices;
using BeauUtil;
using UnityEngine;

namespace FieldDay.UI {
    /// <summary>
    /// Interface panel.
    /// </summary>
    [TypeIndexCapacity(512)]
    public interface IGuiPanel {
        Transform Root { get; }
        StringHash32 Group { get; }

        void Show();
        void Hide();
        void SetVisibleNow(bool visible);

        bool IsShowing();
        bool IsTransitioning();
        bool IsVisible();
    }

    /// <summary>
    /// Singleton interface panel.
    /// </summary>
    public interface ISharedGuiPanel : IGuiPanel { }

    /// <summary>
    /// Populatable interface.
    /// </summary>
    public interface IParameterizedGuiPanel<TParams> : IGuiPanel {
        void Populate(in TParams parms);
    }

    /// <summary>
    /// Popup panel.
    /// </summary>
    public interface IPopupPanel : IGuiPanel { }

    /// <summary>
    /// Interface panel extensions.
    /// </summary>
    static public class GuiPanelExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SetVisible(this IGuiPanel panel, bool visible) {
            if (visible) {
                panel.Show();
            } else {
                panel.Hide();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PopulateAndShow<TPanel, TParams>(this IParameterizedGuiPanel<TParams> panel, in TParams parms)
            where TPanel : IParameterizedGuiPanel<TParams> {
            panel.Populate(parms);
            panel.Show();
        }
    }
}