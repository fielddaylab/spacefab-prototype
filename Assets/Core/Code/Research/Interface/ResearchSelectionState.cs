using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.SharedState;
using SpaceFab.Research;
using System;

namespace SpaceFab.Research {
	[SharedStateInitOrder(-10)]
	public sealed class ResearchSelectionState : SharedStateComponent {
		[NonSerialized] public ResearchMaterial Current;
		[NonSerialized] public bool Locked;

		public CastableEvent<ResearchMaterial> OnUpdated = new CastableEvent<ResearchMaterial>();
    }

	static public partial class ResearchMaterialUtility {
		static public void UpdateSelectedMaterial(ResearchMaterial material) {
			var state = Find.State<ResearchSelectionState>();
			if (state.Locked || state.Current == material) {
				return;
			}

			ResearchInventory inv = Find.State<ResearchInventory>();

			state.Current = material;
			if (inv.KnownMaterials.Add(material.AssetId)) {
				ResearchMaterialItem spawned = ResearchMaterialUtility.SpawnNewTrayItem(material);
				ResearchMaterialUtility.ArrangeTrayItems();
                VfxUtility.PlayFromPool(Find.State<ResearchPools>().ShineEffectPool, spawned.transform);
            }

			state.OnUpdated.Invoke(material);
		}
    }
}