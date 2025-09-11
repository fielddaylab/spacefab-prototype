using BeauUtil;
using FieldDay.SharedState;
using System.Collections.Generic;

namespace SpaceFab.Research {
    public sealed class ResearchInventory : SharedStateComponent {
        public HashSet<StringHash32> KnownMaterials = SetUtils.Create<StringHash32>(64);
    }
}