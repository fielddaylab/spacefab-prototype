using BeauUtil;
using FieldDay.Components;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FieldDay.UI {
    public interface IInputLayer {
        InputLayerMask InputMask { get; set; }
        bool IsInputEnabled();
        void UpdateInputEnabled(bool enabled);

        static public IInputLayer Find(GameObject go) {
            return go.GetComponentInParent<IInputLayer>();
        }

        static public IInputLayer Find(Component component) {
            return component.GetComponentInParent<IInputLayer>();
        }
    }

    public struct InputLayerMask {
        public CanvasSortKey SortKey;
        public StringHash32 GroupId;
        public InputLayerFlags Flags;
    }

    [Flags]
    public enum InputLayerFlags {
        None = 0x00,
        IgnoreSortOrder = 0x01,
        ForceOn = 0x02,
        ForceOff = 0x04
    }

    static public class InputLayerUtility {
        static public bool? GetInputOverride(this IInputLayer layer) {
            InputLayerFlags flags = layer.InputMask.Flags;
            if ((flags & InputLayerFlags.ForceOn) != 0) {
                return true;
            }
            if ((flags & InputLayerFlags.ForceOff) != 0) {
                return false;
            }
            return null;
        }

        static public void SetInputOverride(this IInputLayer layer, bool? overrideEnabled) {
            InputLayerMask mask = layer.InputMask;
            InputLayerFlags flags = mask.Flags & ~(InputLayerFlags.ForceOff | InputLayerFlags.ForceOn);
            if (overrideEnabled.HasValue) {
                flags |= (overrideEnabled.Value ? InputLayerFlags.ForceOn : InputLayerFlags.ForceOff);
            }
            if (flags != mask.Flags) {
                mask.Flags = flags;
                layer.InputMask = mask;
                Game.Gui.ForceUpdate(layer);
            }
        }
    }
}