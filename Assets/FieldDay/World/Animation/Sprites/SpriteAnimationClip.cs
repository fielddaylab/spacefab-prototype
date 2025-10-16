using BeauUtil;
using BeauUtil.Variants;
using FieldDay.Assets;
using FieldDay.Audio;
using System;
using UnityEngine;

namespace FieldDay.Animation.Sprites {
    [CreateAssetMenu(menuName = "Field Day/Sprite Animation Clip", order = 200)]
    public sealed class SpriteAnimationClip : NamedAsset {
        [Header("Frames")]
        public float SampleRate = 30;
        public SpriteAnimationFrame[] Frames;

        [Header("Events")]
        public SpriteAnimationAudioEvent[] AudioEvents;
        public SpriteAnimationTriggerEvent[] TriggerEvents;

        [Header("Playback")]
        public SpriteAnimationPlaybackMode PlaybackMode = SpriteAnimationPlaybackMode.Loop;

        [NonSerialized] public float CachedInvSampleRate;
        [NonSerialized] public float CachedTotalDuration;

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Frame.IsActive(this)) {
                return;
            }

            CachedInvSampleRate = 1f / SampleRate;
            CachedTotalDuration = SpriteAnimationUtility.CalculateTotalDuration(this);
        }
#endif // UNITY_EDITOR
    }

    public enum SpriteAnimationPlaybackMode {
        OneShot,
        Loop,
        LoopRandom,
        StillFrames
    }

    [Serializable]
    public struct SpriteAnimationFrame {
        public Sprite Texture;
    }

    [Serializable]
    public struct SpriteAnimationAudioEvent {
        public int FrameIndex;
        [AudioEvent] public StringHash32 Event;
    }

    [Serializable]
    public struct SpriteAnimationTriggerEvent {
        public int FrameIndex;
        public string EventData;
    }
}