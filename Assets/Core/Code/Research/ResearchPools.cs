using BeauPools;
using BeauUtil;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.Scenes;
using FieldDay.SharedState;
using SpaceFab.Research;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
	public sealed class ResearchPools : SharedStateComponent, IScenePreload {
		[Serializable] public sealed class ItemPool : SerializablePool<ResearchMaterialItem> { }
        [Serializable] public sealed class VfxPool : SerializablePool<VfxInstance> { }

        public ItemPool Items;
		public Material PreExplodeItemMaterial;

        public VfxPool ExplosionEffectPool;
        public VfxPool ShineEffectPool;
        public VfxPool BoltZapEffectPool;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            Items.Prewarm();

            Items.Config.RegisterOnAlloc((pool, item) => {
                if (item.Clickable) {
                    item.Clickable.enabled = true;
                }
            });

            Items.Config.RegisterOnFree((pool, item) => {
                item.ExplosionRoutine.Stop();
            });

            return null;
        }
    }
}