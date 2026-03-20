using System;
using BeauPools;
using BeauUtil;
using Leaf;
using Leaf.Runtime;
using FieldDay.Vox;
using BeauUtil.Debugger;
using FieldDay.Audio;
using BeauRoutine;

namespace FieldDay.Scripting {
    /// <summary>
    /// Scripting thread implementation.
    /// </summary>
    public sealed class ScriptThread : LeafThreadState<ScriptNode> {
        private readonly IPool<ScriptThread> m_Pool;
        private readonly ScriptPlugin m_CustomPlugin;

        private StringHash32 m_OriginalNodeId;
        private StringHash32 m_OriginalEvent;
        private StringHash32 m_Target;
        private ScriptNodePriority m_Priority;
        private ScriptThreadFlags m_Flags;

        private int m_CutsceneDepth;

        private VoxRequestHandle m_Voiceover;
        private float m_VoiceoverReleaseTime;

        private IDialoguePrinter m_CurrentPrinter;
        private IDialogueChoicePresenter m_CurrentChoicePresenter;
        private readonly RingBuffer<IScriptThreadOwned> m_OwnedResources;

        private DialogueCharacterState m_LastKnownCharacter;

        public ScriptThread(IPool<ScriptThread> pool, ScriptPlugin inPlugin) : base(inPlugin) {
            m_Pool = pool;
            m_CustomPlugin = inPlugin;
            m_OwnedResources = new RingBuffer<IScriptThreadOwned>(8, RingBufferMode.Expand);
        }

        #region Initial State

        public StringHash32 InitialNodeId() {
            return m_OriginalNodeId;
        }

        public StringHash32 InitialTriggerOrFunction() {
            return m_OriginalEvent;
        }

        public StringHash32 Target() {
            return m_Target;
        }

        public ScriptNodePriority Priority() {
            return m_Priority;
        }

        public bool IsFunction() {
            return (m_Flags & ScriptThreadFlags.IsFunction) != 0;
        }

        public bool IsTrigger() {
            return (m_Flags & ScriptThreadFlags.IsTrigger) != 0;
        }

        internal void SetInitialNode(ScriptNode node, StringHash32 target) {
            m_OriginalNodeId = node.Id();
            m_Target = target;
            m_Priority = node.Priority;

            if ((node.Flags & ScriptNodeFlags.Trigger) != 0) {
                m_Flags |= ScriptThreadFlags.IsTrigger;
            } else if ((node.Flags & ScriptNodeFlags.Function) != 0) {
                m_Flags |= ScriptThreadFlags.IsFunction;
            }

            m_OriginalEvent = node.TriggerOrFunctionId;
        }

        #endregion // Initial State

        #region Cutscene

        public bool IsCutscene() {
            return m_CutsceneDepth > 0 || (m_Flags & ScriptThreadFlags.Cutscene) != 0;
        }

        internal void PushCutscene() {
            if (m_CutsceneDepth++ == 0) {
                m_CustomPlugin.SetCutscene(GetHandle());
            }
        }

        internal void PopCutscene() {
            Assert.True(m_CutsceneDepth > 0, "ScriptThread.Push/PopCutscene calls unbalanced");
            if (m_CutsceneDepth-- == 1) {
                m_CustomPlugin.DereferenceCutscene(GetHandle());
            }
        }

        #endregion // Cutscene

        #region Skipping

        /// <summary>
        /// Returns if the thread is skipping.
        /// </summary>
        public bool IsSkipping() {
            return (m_Flags & ScriptThreadFlags.Skipping) != 0;
        }

        internal void StartSkipping() {
            m_Flags |= ScriptThreadFlags.SkipSingle;
            m_Routine.SetTimeScale(1000);
            m_CurrentPrinter?.StartSkip();
        }

        internal void StopSkipping() {
            if ((m_Flags & ScriptThreadFlags.Skipping) != 0) {
                m_Flags &= ~(ScriptThreadFlags.Skipping | ScriptThreadFlags.SkipSingle);
                m_Routine.SetTimeScale(1);

                m_CurrentPrinter?.CancelSkip();

                m_CustomPlugin.StopSkippingCutscene(GetHandle());
            }
        }

        #endregion // Skipping

        #region Line Skip

        /// <summary>
        /// Returns if the current line should be skipped.
        /// </summary>
        public bool PopSkipSingle() {
            if ((m_Flags & ScriptThreadFlags.SkipSingle) != 0) {
                m_Flags &= ~ScriptThreadFlags.SkipSingle;
                return true;
            }

            return (m_Flags & ScriptThreadFlags.Skipping) != 0;
        }

        /// <summary>
        /// Skips the next line.
        /// </summary>
        public void SkipSingle() {
            m_Flags |= ScriptThreadFlags.SkipSingle;
            SkipCurrentVox();
        }

        #endregion // Line Skip

        #region Voiceover

        /// <summary>
        /// Skips the current voiceover line.
        /// </summary>
        public void SkipCurrentVox() {
            if (m_Voiceover.IsValid) {
                VoxUtility.Stop(m_Voiceover);
                m_Voiceover = default;
                m_VoiceoverReleaseTime = 0;
            }
        }

        /// <summary>
        /// Returns if the current voiceover line is playing.
        /// </summary>
        public bool IsVoxPlaying() {
            return m_Voiceover.IsValid && VoxUtility.IsPlaying(m_Voiceover);
        }

        /// <summary>
        /// Cancels the existing voiceover line
        /// and replaces it with the given line.
        /// </summary>
        internal void AssignVox(VoxRequestHandle voxHandle) {
            if (m_Voiceover != voxHandle) {
                VoxUtility.Stop(m_Voiceover);
                m_Voiceover = voxHandle;
                m_VoiceoverReleaseTime = 0;
            }
        }

        /// <summary>
        /// Sets when voiceover will be released.
        /// </summary>
        internal void SetVoxReleaseTime(float releaseTime) {
            if (m_Voiceover.IsValid) {
                m_VoiceoverReleaseTime = releaseTime;
            }
        }

        /// <summary>
        /// Returns when voiceover will be released.
        /// </summary>
        internal float GetVoxReleaseTime() {
            if (m_VoiceoverReleaseTime >= 0) {
                return m_VoiceoverReleaseTime;
            } else {
                return VoxUtility.GetDuration(m_Voiceover) + m_VoiceoverReleaseTime;
            }
        }

        /// <summary>
        /// Releases reference to the current voiceover line
        /// without stopping it.
        /// </summary>
        internal void ReleaseVox() {
            if (m_Voiceover.IsValid) {
                m_Voiceover = default;
                m_VoiceoverReleaseTime = 0;
            }
        }

        /// <summary>
        /// Returns the current voiceover line.
        /// </summary>
        internal VoxRequestHandle GetCurrentVox() {
            return m_Voiceover;
        }

        #endregion // Voiceover

        #region Choice

        public void BeginChoice() {
            m_Flags |= ScriptThreadFlags.Choosing;
        }

        public void EndChoice() {
            m_Flags &= ~ScriptThreadFlags.Choosing;
        }

        #endregion // Choice

        #region Character State

        public DialogueCharacterState GetCharacterState() {
            return m_LastKnownCharacter;
        }

        public void SetCharacterState(DialogueCharacterState characterState) {
            if (!m_LastKnownCharacter.Equals(characterState)) {
                m_LastKnownCharacter = characterState;
                m_CurrentPrinter?.UpdateCharacter(characterState);
            }
        }

        #endregion // Character State

        #region Resources

        /// <summary>
        /// Returns the current dialogue print interface.
        /// </summary>
        public IDialoguePrinter GetPrinter() {
            return m_CurrentPrinter;
        }

        /// <summary>
        /// Sets the current dialogue print interface.
        /// </summary>
        public void SetPrinter(IDialoguePrinter printer) {
            if (printer != m_CurrentPrinter) {
                if (m_CurrentPrinter != null && m_CurrentPrinter != m_CurrentChoicePresenter) {
                    m_CurrentPrinter.TryClearThreadOwner(GetHandle(), ScriptThreadOwnershipClearReason.Cancelled);
                }
                m_CurrentPrinter = printer;
                if (m_CurrentPrinter != null && m_CurrentPrinter != m_CurrentChoicePresenter) {
                    m_CurrentPrinter.SwitchThreadOwner(GetHandle());
                }
            }
        }

        /// <summary>
        /// Releases the current printer.
        /// </summary>
        public void ReleaseCurrentPrinter(ScriptThreadOwnershipClearReason reason) {
            if (m_CurrentPrinter != null) {
                if (m_CurrentPrinter != m_CurrentChoicePresenter) {
                    m_CurrentPrinter.TryClearThreadOwner(GetHandle(), reason);
                }
                m_CurrentPrinter = null;
            }
        }

        /// <summary>
        /// Returns the current dialogue choice interface.
        /// </summary
        public IDialogueChoicePresenter GetChoicePresenter() {
            return m_CurrentChoicePresenter;
        }

        /// <summary>
        /// Sets the current dialogue choice interface.
        /// </summary>
        public void SetChoicePresenter(IDialogueChoicePresenter choicePresenter) {
            if (choicePresenter != m_CurrentChoicePresenter) {
                if (m_CurrentChoicePresenter != null && m_CurrentChoicePresenter != m_CurrentPrinter) {
                    m_CurrentChoicePresenter.TryClearThreadOwner(GetHandle(), ScriptThreadOwnershipClearReason.Cancelled);
                }
                m_CurrentChoicePresenter = choicePresenter;
                if (m_CurrentChoicePresenter != null && m_CurrentChoicePresenter != m_CurrentPrinter) {
                    m_CurrentChoicePresenter.SwitchThreadOwner(GetHandle());
                }
            }
        }

        /// <summary>
        /// Releases the current choice interface.
        /// </summary>
        public void ReleaseCurrentChoicePresenter(ScriptThreadOwnershipClearReason reason) {
            if (m_CurrentChoicePresenter != null) {
                if (m_CurrentChoicePresenter != m_CurrentPrinter) {
                    m_CurrentChoicePresenter.TryClearThreadOwner(GetHandle(), reason);
                }
                m_CurrentChoicePresenter = null;
            }
        }

        /// <summary>
        /// Acquires ownership of a resource.
        /// </summary>
        public void TakeOwnership(IScriptThreadOwned owned) {
            Assert.NotNull(owned);

            IDialoguePrinter printer = owned as IDialoguePrinter;
            IDialogueChoicePresenter choicePresenter = owned as IDialogueChoicePresenter;

            if (printer != null) {
                SetPrinter(printer);
            }
            if (choicePresenter != null) {
                SetChoicePresenter(choicePresenter);
            }

            if (printer == null && choicePresenter == null) {
                owned.SwitchThreadOwner(GetHandle());
                m_OwnedResources.PushBack(owned);
            }
        }

        /// <summary>
        /// Releases ownership of a resource.
        /// </summary>
        public void ReleaseOwnership(IScriptThreadOwned owned, ScriptThreadOwnershipClearReason reason) {
            Assert.NotNull(owned);

            IDialoguePrinter printer = owned as IDialoguePrinter;
            IDialogueChoicePresenter choicePresenter = owned as IDialogueChoicePresenter;

            if (choicePresenter != null && m_CurrentChoicePresenter == choicePresenter) {
                ReleaseCurrentChoicePresenter(reason);
            }
            if (printer != null && m_CurrentPrinter == printer) {
                ReleaseCurrentPrinter(reason);
            }

            if (printer == null && choicePresenter == null) {
                owned.TryClearThreadOwner(GetHandle(), reason);
                m_OwnedResources.FastRemove(owned);
            }
        }

        #endregion // Resources

        protected override void Reset() {
            ScriptThreadOwnershipClearReason releaseReason = !HasNodes() ? ScriptThreadOwnershipClearReason.Completed : ScriptThreadOwnershipClearReason.Cancelled;

            m_CustomPlugin.StopTracking(this);
            if (m_Voiceover.IsValid) {
                VoxUtility.Stop(ref m_Voiceover);
            }
            m_VoiceoverReleaseTime = 0;
            m_LastKnownCharacter = default;

            StopSkipping();

            LeafThreadHandle handle = GetHandle();

            while(m_OwnedResources.TryPopBack(out var owned)) {
                owned.TryClearThreadOwner(handle, releaseReason);
            }

            m_CurrentChoicePresenter?.TryClearThreadOwner(handle, releaseReason);
            m_CurrentPrinter?.TryClearThreadOwner(handle, releaseReason);

            m_CurrentPrinter = null;
            m_CurrentChoicePresenter = null;

            Log.Msg("[ScriptThread] Thread '{0}' killed", m_OriginalNodeId.ToDebugString());

            base.Reset();

            while(m_CutsceneDepth > 0) {
                PopCutscene();
            }

            m_Flags = default;
            m_OriginalNodeId = null;
            m_Priority = default;

            m_Pool.Free(this);
        }
    }

    [Flags]
    internal enum ScriptThreadFlags {
        None = 0,

        Skipping = 0x01,
        Cutscene = 0x02,
        IsFunction = 0x04,
        IsTrigger = 0x08,
        SkipSingle = 0x10,
        Choosing = 0x20
    }
}