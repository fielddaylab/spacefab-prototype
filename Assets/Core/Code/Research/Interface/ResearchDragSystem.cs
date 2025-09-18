using FieldDay;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.Physics;
using FieldDay.SharedState;
using FieldDay.Systems;
using SpaceFab.Research;
using System;
using UnityEngine;

namespace SpaceFab.Research {
    [SysUpdate(GameLoopPhase.LateUpdate, 1000)]
	public sealed class ResearchDragSystem : SharedStateSystemBehaviour<ResearchDragState> { 
        public override void ProcessWork(float deltaTime) {
            bool cancelQueued = false;

            bool cursorOnWorld = MouseControls.TryGetWorldPosition2D(out Vector2 worldPos);

            if (Game.Input.IsMousePressed(1)) {
                cancelQueued = true;
            }
            
            if (Game.Input.IsMousePressed(0)) {
                if (!cursorOnWorld) {
                    cancelQueued = true;
                } else {
                    Collider2D overlappingSlot = Physics2D.OverlapCircle(worldPos, 0.01f, LayerMasks.ResearchSlot_Mask);
                    Collider2D overlappingGem = Physics2D.OverlapCircle(worldPos, 0.01f, LayerMasks.ResearchGem_Mask);
                    ResearchSlot slot = overlappingSlot.ResolveComponent<ResearchSlot>();
                    ResearchMaterialItem gem = overlappingGem.ResolveComponent<ResearchMaterialItem>();

                    if (m_State.CurrentlyDragging) {
                        if (slot) {
                            ResearchSlotUtility.DepositCurrentDrag(slot);
                        } else if (gem) {
                            if (gem.Material == m_State.CurrentlyDragging) {
                                cancelQueued = true;
                            } else {
                                ResearchSlotUtility.LiftItem(gem);
                            }
                        } else {
                            cancelQueued = true;
                        }
                    } else {
                        if (gem != null) {
                            ResearchSlotUtility.LiftItem(gem);
                        }
                    }
                }
            }

            if (cursorOnWorld) {
                m_State.DragRenderer.transform.position = worldPos;
            }

            if (cancelQueued) {
                ResearchSlotUtility.CancelCurrentDrag();
            }
        }
    }
}