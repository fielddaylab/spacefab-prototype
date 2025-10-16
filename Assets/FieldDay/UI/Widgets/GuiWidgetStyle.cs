using BeauUtil;
using FieldDay.Components;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FieldDay.UI.Widgets {
    public interface IGuiWidgetStyle<TValue> {
        void Populate(in TValue data);
    }

    public abstract class GuiWidgetStyle<TValue> : MonoBehaviour, IGuiWidgetStyle<TValue> {
        public abstract void Populate(in TValue data);
    }
}