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

		public CastableEvent<ResearchMaterial> OnUpdated = new CastableEvent<ResearchMaterial>();
    }

	static public partial class ResearchMaterialUtility {
		static public void UpdateSelectedMaterial(ResearchMaterial material) {
			var state = Find.State<ResearchSelectionState>();
			if (state.Current == material) {
				return;
			}

			state.Current = material;
			state.OnUpdated.Invoke(material);
		}
    }
}