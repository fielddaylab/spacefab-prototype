using FieldDay;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.SharedState;
using SpaceFab.Research;
using System;
using UnityEngine.EventSystems;

namespace SpaceFab.Research {
	public sealed class ResearchDragState : SharedStateComponent {
		public ResearchMaterialRig DragRenderer;
		public CursorHint DragCursor;
		public bool AllowSwap;
		public Physics2DRaycaster Raycaster;
		
		[NonSerialized] public ResearchMaterial CurrentlyDragging;

        private void Awake() {
			DragRenderer.gameObject.SetActive(false);
        }
    }

	static public partial class ResearchSlotUtility {
		static public bool CancelCurrentDrag() {
			ResearchDragState dragState = Find.State<ResearchDragState>();
			if (dragState.CurrentlyDragging) {
				dragState.CurrentlyDragging = null;
				dragState.DragRenderer.gameObject.SetActive(false);
                CursorHint.Unlock(dragState.DragCursor);
				dragState.Raycaster.eventMask |= LayerMasks.UI_Mask;
                return true;
			}

			return false;
        }

		static public bool DepositCurrentDrag(ResearchSlot slot) {
			if (!slot) {
				CancelCurrentDrag();
				return false;
			}

            ResearchDragState dragState = Find.State<ResearchDragState>();
            if (dragState.CurrentlyDragging) {
				ResearchMaterial swap = null;
				if (dragState.AllowSwap && slot.Item != null) {
					swap = slot.Item.Material;
				}
				FillInSlot(slot, dragState.CurrentlyDragging);
				if (swap) {
					dragState.CurrentlyDragging = swap;
                    ResearchMaterialUtility.ApplyPropertiesToRig(dragState.DragRenderer, dragState.CurrentlyDragging);
					ResearchMaterialUtility.UpdateSelectedMaterial(swap);
                } else {
                    dragState.CurrentlyDragging = null;
                    dragState.DragRenderer.gameObject.SetActive(false);
                    CursorHint.Unlock(dragState.DragCursor);
                    dragState.Raycaster.eventMask |= LayerMasks.UI_Mask;
                }
                return true;
            }

			return false;
        }

        static public bool LiftItem(ResearchMaterialItem item) {
            if (!item) {
                return false;
            }

            ResearchDragState dragState = Find.State<ResearchDragState>();
			dragState.CurrentlyDragging = item.Material;
			if (item.CurrentSlot) {
				FillInSlot(item.CurrentSlot, null);
			}
			dragState.DragRenderer.gameObject.SetActive(true);
			ResearchMaterialUtility.ApplyPropertiesToRig(dragState.DragRenderer, dragState.CurrentlyDragging);
			CursorHint.TryLock(dragState.DragCursor);
            dragState.Raycaster.eventMask &= ~LayerMasks.UI_Mask;
            ResearchMaterialUtility.UpdateSelectedMaterial(dragState.CurrentlyDragging);
            return true;
        }
    }
}