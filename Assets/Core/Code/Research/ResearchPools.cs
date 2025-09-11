using BeauPools;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.SharedState;
using SpaceFab.Research;
using System;

namespace SpaceFab.Research {
	public sealed class ResearchPools : SharedStateComponent {
		[Serializable] public sealed class ItemPool : SerializablePool<ResearchMaterialItem> { }

		public ItemPool Items;
    }
}