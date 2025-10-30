using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scenes;
using System;
using System.Collections.Generic;

namespace SpaceFab.Research {
    public sealed class ResearchTool : BatchedComponent, IScenePreload {
        public string ToolName;
        public ResearchSlot[] Slots;
        public ResearchSlot OutputSlot;
        public CircuitRenderer Circuit;

        [NonSerialized] public bool AllSlotsFilled;

        public CastableEvent<ResearchTool> OnInputSlotsUpdated = new CastableEvent<ResearchTool>();

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            Action onUpdate = () => ResearchToolUtility.UpdateSlotFillState(this);
            foreach(var slot in Slots) {
                slot.OnSlotUpdated.Register(onUpdate);
            }
            return null;
        }
    }

    static public partial class ResearchToolUtility {
        static public ResearchMaterial GetInputMaterial(ResearchTool tool, int slotIndex) {
            Assert.True(slotIndex >= 0 && slotIndex < tool.Slots.Length);
            ResearchMaterialItem item = tool.Slots[slotIndex].Item;
            return item ? item.Material : null;
        }

        static public ResearchMaterialItem GetInputMaterialItem(ResearchTool tool, int slotIndex) {
            Assert.True(slotIndex >= 0 && slotIndex < tool.Slots.Length);
            return tool.Slots[slotIndex].Item;
        }

        static public void UpdateSlotFillState(ResearchTool tool) {
            bool areSlotsFilled = true;
            foreach(var slot in tool.Slots) {
                if (slot.Item == null) {
                    areSlotsFilled = false;
                    break;
                }
            }

            tool.AllSlotsFilled = areSlotsFilled;
            tool.OnInputSlotsUpdated.Invoke(tool);
        }
    }
}