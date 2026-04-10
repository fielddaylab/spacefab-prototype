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
		[NonSerialized] public ResearchMaterial Context;

		public CastableEvent<ResearchMaterial> OnUpdated = new CastableEvent<ResearchMaterial>();
        public CastableEvent<ResearchMaterial> OnUpdatedContext = new CastableEvent<ResearchMaterial>();
    }

	static public partial class ResearchMaterialUtility {
		static public void UpdateSelectedMaterial(ResearchMaterial material) {
			var state = Find.State<ResearchSelectionState>();
			if (state.Locked || state.Current == material) {
				return;
			}

			state.Current = material;
			state.OnUpdated.Invoke(material);
		}

        static public void UpdateContextMaterial(ResearchMaterial material) {
            var state = Find.State<ResearchSelectionState>();
            if (state.Locked || state.Context == material) {
                return;
            }

            state.Context = material;
            state.OnUpdatedContext.Invoke(material);
        }
    }
}