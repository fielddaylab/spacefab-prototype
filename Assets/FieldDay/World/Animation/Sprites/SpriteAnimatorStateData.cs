using UnityEngine;

namespace FieldDay.Animation.Sprites {
    public struct SpriteAnimatorStateData {
        public float CurrentFrame;
        public short SampleIndex;
        public SpriteAnimatorStateFlags Flags;
        public float PlaybackSpeed;

        public SpriteAnimationClip Clip;
        public SpriteAnimationGraph Graph;
    }

    public enum SpriteAnimatorStateFlags : byte {
        Uninitialized = 0,
        Playing = 0x01,
        Paused = 0x02,
        Reversed = 0x04
    }

    static public class SpriteAnimationUtility {
        static public void Advance(ref SpriteAnimatorStateData state, float deltaTime) {
            if ((state.Flags & SpriteAnimatorStateFlags.Paused) != 0 || (state.Flags & SpriteAnimatorStateFlags.Playing) == 0 || Mathf.Approximately(state.PlaybackSpeed, 0) || ReferenceEquals(state.Clip, null)) {
                return;
            }

            state.CurrentFrame += state.PlaybackSpeed * deltaTime * state.Clip.CachedInvSampleRate;
        }

        static public float CalculateTotalDuration(SpriteAnimationClip clip) {
            return clip.Frames.Length / clip.SampleRate;
        }
    }
}