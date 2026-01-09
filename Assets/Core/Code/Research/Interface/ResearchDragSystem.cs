using BeauRoutine;
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
            bool cursorOnCanvas = Game.Input.IsPointerOverCanvas();

            bool cursorOnWorld = MouseControls.TryGetWorldPosition2D(out Vector2 worldPos);
            bool cursorValid = cursorOnWorld && CursorUtility.IsCursorWithinVirtualViewport() && !cursorOnCanvas;

            if (Game.Input.IsMousePressed(1)) {
                cancelQueued = true;
            } else {
                OverlapResults results = default;
                bool leftClicked = Game.Input.IsMousePressed(0);

                if (m_State.CurrentlyDragging) {
                    if (cursorValid) {
                        GetPositionOverlap(worldPos, out results);
                    }

                    if (leftClicked) {
                        if (results.Slot) {
                            ResearchSlotUtility.DepositCurrentDrag(results.Slot);
                        } else if (results.Gem) {
                            if (results.Gem.Material == m_State.CurrentlyDragging) {
                                cancelQueued = true;
                            } else {
                                ResearchSlotUtility.LiftItem(results.Gem);
                            }
                        } else {
                            cancelQueued = true;
                        }
                    } else {
                        if (results.Slot != m_State.SlotHoveredOver) {
                            if (m_State.SlotHoveredOver) {
                                m_State.SlotHoveredOver.HoverVfx.Stop(true, UnityEngine.ParticleSystemStopBehavior.StopEmitting);
                            }
                            m_State.SlotHoveredOver = results.Slot;
                            if (m_State.SlotHoveredOver) {
                                m_State.SlotHoveredOver.HoverVfx.Play();
                            }
                        }
                    }
                } else if (cursorValid && leftClicked) {
                    GetPositionOverlap(worldPos, out results);
                    if (results.Gem != null) {
                        ResearchSlotUtility.LiftItem(results.Gem);
                    }
                }
            }

            if (cancelQueued) {
                ResearchSlotUtility.CancelCurrentDrag();
            }

            if (cursorOnWorld && m_State.CurrentlyDragging) {
                m_State.DragRenderer.transform.SetPosition(worldPos, Axis.XY, Space.World);
            }
        }

        static private void GetPositionOverlap(Vector2 worldPos, out OverlapResults results) {
            Collider2D overlappingSlot = Physics2D.OverlapCircle(worldPos, 0.01f, LayerMasks.ResearchSlot_Mask);
            Collider2D overlappingGem = Physics2D.OverlapCircle(worldPos, 0.01f, LayerMasks.ResearchGem_Mask);
            results.Slot = overlappingSlot.ResolveComponent<ResearchSlot>();
            results.Gem = overlappingGem.ResolveComponent<ResearchMaterialItem>();
        }

        private struct OverlapResults {
            public ResearchSlot Slot;
            public ResearchMaterialItem Gem;
        }
    }
}