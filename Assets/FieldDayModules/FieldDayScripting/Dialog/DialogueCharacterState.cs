using BeauUtil;
using System;

namespace FieldDay.Scripting {
    public struct DialogueCharacterState : IEquatable<DialogueCharacterState> {
        public StringHash32 CharacterId;
        public StringHash32 PoseId;
        public string OverrideName;

        public bool IsEmpty {
            get { return CharacterId.IsEmpty && PoseId.IsEmpty && string.IsNullOrEmpty(OverrideName); }
        }

        public bool Equals(DialogueCharacterState other) {
            return CharacterId == other.CharacterId
                && PoseId == other.PoseId
                && string.Equals(OverrideName, other.OverrideName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) {
            if (obj is DialogueCharacterState) {
                return Equals((DialogueCharacterState)obj);
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(CharacterId, PoseId, OverrideName);
        }
    }
}