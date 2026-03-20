using System;
using System.Collections;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Tags;
using BeauUtil.Variants;
using FieldDay.Debugging;
using FieldDay.Localization;
using FieldDay.Vox;
using Leaf;
using Leaf.Runtime;
using UnityEngine;

namespace FieldDay.Scripting {
    public sealed class ScriptPlugin : ILeafPlugin<ScriptNode>, ILeafPlugin, ILeafVariableAccess {
        static public readonly StringHash32 VoxTag = "Script";

        private readonly ScriptRuntimeState m_RuntimeState;
        private readonly ScriptDatabase m_Database;
        private readonly IMethodCache m_CachedMethodCache;
        private readonly VariantTableResolver m_CachedResolver;
        private readonly LeafRuntimeConfiguration m_Configuration;

        public ScriptPlugin(ScriptRuntimeState runtimeState, ScriptDatabase database) {
            m_RuntimeState = runtimeState;
            m_Database = database;

            m_CachedMethodCache = runtimeState.MethodCache;
            m_CachedResolver = runtimeState.Resolver;
            
            m_Configuration = new LeafRuntimeConfiguration();
        }

        #region Tracking

        internal void StopTracking(ScriptThread threadState) {
            LeafThreadHandle handle = threadState.GetHandle();

            if (m_RuntimeState.Cutscene == handle) {
                m_RuntimeState.Cutscene = default;
            }

            StringHash32 who = threadState.Target();
            if (!who.IsEmpty) {
                if (m_RuntimeState.ActorThreadMap.Threads.TryGetValue(who, out var recordedHandle) && handle == recordedHandle) {
                    m_RuntimeState.ActorThreadMap.Threads.Remove(who);
                }
            }

            m_RuntimeState.ActiveThreads.FastRemove(handle);
        }

        #endregion // Tracking

        #region Running

        public LeafThreadState<ScriptNode> Fork(LeafThreadState<ScriptNode> inParentThreadState, ScriptNode inForkNode) {
            ScriptThread thread = (ScriptThread) inParentThreadState;
            var handle = Run(inForkNode, thread.Target(), thread.Actor, thread.Locals, null, true);
            return handle.GetThread<ScriptThread>();
        }

        public LeafThreadHandle Run(ScriptNode node, StringHash32 targetId, ILeafActor actor, VariantTable localVars, string name, bool tickImmediately) {
            if (node == null) {
                return default(LeafThreadHandle);
            }

            if ((node.Flags & ScriptNodeFlags.Cutscene) != 0) {
                m_RuntimeState.Cutscene.Kill();
            }

            bool isFunction = (node.Flags & ScriptNodeFlags.Function) != 0;

            StringHash32 who = ScriptUtility.ResolveThreadTarget(targetId, node);

            if (!isFunction) {
                if (!who.IsEmpty && m_RuntimeState.ActorThreadMap.Threads.TryGetValue(who, out LeafThreadHandle existingActorThread)) {
                    existingActorThread.Kill();
                }
            }

            ScriptThread threadState = m_RuntimeState.ThreadPool.Alloc();
            LeafThreadHandle threadHandle = threadState.Setup(name, actor, localVars);
            threadState.SetInitialNode(node, who);
            threadState.AttachRoutine(Routine.Start(GameLoop.Host, LeafRuntime.Execute(threadState, node)).SetPhase(RoutinePhase.Manual));

            if (!isFunction) {
                if (!who.IsEmpty) {
                    m_RuntimeState.ActorThreadMap.Threads[who] = threadHandle;
                }
            }

            m_RuntimeState.ActiveThreads.PushBack(threadHandle);

            Log.Msg("[ScriptPlugin] Thread '{0}' spawned", node.FullName);

            if (tickImmediately) {
                threadState.ForceTick();
            }

            return threadHandle;
        }

        #endregion // Running

        #region Node Flow

        public void OnNodeEnter(ScriptNode inNode, LeafThreadState<ScriptNode> inThreadState) {
            ScriptThread thread = (ScriptThread) inThreadState;
            
            inNode.Package().AddReference();

            m_RuntimeState.CurrentHistoryBuffer?.RecordVisit(inNode.Id(), inNode.PersistenceScope, Time.realtimeSinceStartup);

            if ((inNode.Flags & ScriptNodeFlags.Cutscene) != 0) {
                thread.PushCutscene();
            }
        }

        public void OnNodeExit(ScriptNode inNode, LeafThreadState<ScriptNode> inThreadState) {
            ScriptThread thread = (ScriptThread) inThreadState;

            inNode.Package().ReleaseReference();

            if ((inNode.Flags & ScriptNodeFlags.Cutscene) != 0) {
                thread.PopCutscene();
            }
        }

        public void OnEnd(LeafThreadState<ScriptNode> inThreadState) {
            ScriptThread thread = (ScriptThread) inThreadState;
            thread.Kill();
        }

        internal void SetCutscene(LeafThreadHandle handle) {
            if (m_RuntimeState.Cutscene != handle) {
                m_RuntimeState.Cutscene.Kill();
                m_RuntimeState.Cutscene = handle;

                if (handle.IsRunning()) {
                    ScriptUtility.KillLowPriorityThreads(ScriptNodePriority.High);
                }
            }
        }

        internal void DereferenceCutscene(LeafThreadHandle handle) {
            if (m_RuntimeState.Cutscene == handle) {
                m_RuntimeState.Cutscene = default;
            }
        }

        internal void BeginSkippingCutscene(LeafThreadHandle handle) {
            if (m_RuntimeState.Cutscene == handle && !m_RuntimeState.SkipCutsceneRoutine) {
                
                DebugFlags.BlockTimeControl();
                Game.Input.PauseDevices();
                Game.Input.PauseRaycasts();
                
                handle.GetThread().Pause();

                m_RuntimeState.IsSkippingCutscene = true;
                m_RuntimeState.SkipCutsceneRoutine.Replace(GameLoop.Host, SkipRoutine(handle));
            }
        }

        internal void StopSkippingCutscene(LeafThreadHandle handle) {
            if (m_RuntimeState.Cutscene == handle) {
                m_RuntimeState.SkipCutsceneRoutine.Stop();
                m_RuntimeState.IsSkippingCutscene = false;

                // fade back in
                handle.GetThread().Resume();
                
                DebugFlags.UnblockTimeControl();
                Game.Input.ResumeDevices();
                Game.Input.ResumeRaycasts();
            }
        }

        static internal IEnumerator SkipRoutine(LeafThreadHandle handle) {
            // fade out
            handle.GetThread<ScriptThread>().StartSkipping();
            yield return 0.1f;
            handle.GetThread().Resume();
        }

        #endregion // Node Flow

        #region Line

        public IEnumerator RunLine(LeafThreadState<ScriptNode> inThreadState, LeafLineInfo inLine) {
            if (inLine.IsEmptyOrWhitespace) {
                return null;
            }

            ScriptThread thread = (ScriptThread) inThreadState;
            if (thread.IsSkipping()) {
                if (LeafRuntime.PredictChoice(thread)) {
                    thread.StopSkipping();
                } else {
                    SkipLine(thread, inLine);
                    return null;
                }
            }
            
            return ExecuteLine(thread, inLine);
        }

        private void SkipLine(ScriptThread thread, LeafLineInfo line) {
            TagString str = thread.TagString;
            ScriptUtility.ParseTag(ref str, line.Text, thread);
            m_RuntimeState.OnTaggedLineProcessed.Invoke(thread, str);

            TagStringEventHandler evtHandler = m_RuntimeState.TagEventHandler;
            for(int i = 0; i < str.NodeCount; i++) {
                var node = str.GetNode(i);
                if (node.Type == TagNodeType.Event && !m_RuntimeState.SkippableTagEvents.Contains(node.Event.Type)) {
                    evtHandler.TryEvaluate(node.Event, thread, out IEnumerator coroutine);
                    if (coroutine != null) {
                        Log.Warn("[ScriptPlugin] Coroutine '{0}' generated for event '{1}' during a skip - this coroutine will not be processed", coroutine.ToString(), node.Event.Type.ToDebugString());
                        (coroutine as IDisposable)?.Dispose();
                    }
                }
            }
        }

        private IEnumerator ExecuteLine(ScriptThread thread, LeafLineInfo line) {
            LeafThreadHandle cachedHandle = thread.GetHandle();

            TagString tagStr = thread.TagString;
            ScriptUtility.ParseTag(ref tagStr, line.Text, thread);
            m_RuntimeState.OnTaggedLineProcessed.Invoke(thread, tagStr);

            DialogueCharacterState charState = thread.GetCharacterState();

            bool voxDesired = (m_RuntimeState.Flags & ScriptRuntimeConfigFlags.VoiceoverAllLinesByDefault) != 0;
            bool dialogBoxDesired = (m_RuntimeState.Flags & ScriptRuntimeConfigFlags.UseDialogBoxByDefault) != 0;

            StringHash32? newStyle = null;

            // INITIAL DATA

            int nodeIndex = 0;
            if (tagStr.EventCount > 0) {
                for(nodeIndex = 0; nodeIndex < tagStr.NodeCount; nodeIndex++) {
                    TagNodeData node = tagStr.GetNode(nodeIndex);
                    if (node.Type != TagNodeType.Event) {
                        break;
                    }

                    StringHash32 eventType = node.Event.Type;
                    if (eventType == TagEvents.HasNoVox) {
                        voxDesired = false;
                        dialogBoxDesired = true;
                    } else if (eventType == TagEvents.HasVox) {
                        voxDesired = true;
                    } else if (eventType == TagEvents.VoxOnly) {
                        voxDesired = true;
                        dialogBoxDesired = false;
                    } else if (eventType == TagEvents.SetStyle) {
                        dialogBoxDesired = true;
                        newStyle = node.Event.Argument0.AsStringHash();
                    } else if (eventType == LeafUtils.Events.Character) {
                        charState.CharacterId = node.Event.Argument0.AsStringHash();
                        charState.PoseId = node.Event.Argument1.AsStringHash();
                        charState.OverrideName = null;
                    } else if (eventType == LeafUtils.Events.Pose) {
                        charState.PoseId = node.Event.Argument0.AsStringHash();
                    } else if (eventType == TagEvents.OverrideCharName) {
                        charState.OverrideName = node.Event.StringArgument.ToString();
                    } else {
                        break;
                    }
                }

                thread.SetCharacterState(charState);
            }

            // CHARACTER ID

            StringHash32 charId = charState.CharacterId;
            ILeafActor actor;
            VoxEmitter vox;
            if (!charId.IsEmpty) {
                actor = ScriptUtility.FindActor(charId);
                vox = voxDesired ? VoxUtility.FindEmitter(charId) : null;
            } else {
                actor = null;
                vox = null;
            }

            // DIALOG BOX

            if (newStyle.HasValue) {
                thread.TakeOwnership(ScriptUtility.GetDialoguePrinter(newStyle.Value));
            }

            IDialoguePrinter currentPrinter = thread.GetPrinter();

            if (currentPrinter == null && dialogBoxDesired) {
                thread.TakeOwnership(ScriptUtility.GetDialoguePrinter(StringHash32.Null));
                currentPrinter = thread.GetPrinter();
            }

            TagStringEventHandler eventHandler = m_RuntimeState.TagEventHandler;
            eventHandler = currentPrinter?.PrepareLine(tagStr, charState, eventHandler) ?? eventHandler;

            // VOX

            VoxRequestHandle voxHandle;
            bool hadVox;
            SubtitleDisplayData fakeSubtitleData;

            if (voxDesired && vox != null && VoxUtility.HasHumanReadableMapping(line.LineCode)) {
                VoxRequest req = default;
                req.CharacterId = charId;
                req.Tag = VoxTag;
                req.LineCode = line.LineCode;
                req.Subtitle = new SubtitleEntry(tagStr.RichTextString);
                req.UnloadAfterPlayback = (thread.PeekNode().Flags & ScriptNodeFlags.Once) != 0;
                req.StartPlayback = false;
                req.Priority = ScriptUtility.ScriptPriorityToVoxPriority(thread.Priority());
                VoxUtility.PushImmediateLoad(line.LineCode);
                voxHandle = VoxUtility.Speak(vox, req);
                thread.AssignVox(voxHandle);
                hadVox = true;
            } else {
                voxHandle = default;
                thread.AssignVox(default);
                hadVox = false;
            }

            if (voxDesired) {
                // peek ahead for loading
                StringHash32 nextLineCode = LeafRuntime.PredictLine(thread);
                if (!nextLineCode.IsEmpty && VoxUtility.HasHumanReadableMapping(nextLineCode)) {
                    VoxUtility.QueueLoad(nextLineCode);
                }

                if (voxHandle.IsValid) {
                    while (VoxUtility.IsLoading(voxHandle)) {
                        yield return null;
                    }

                    if (thread.GetPrinter() != currentPrinter) {
                        currentPrinter = thread.GetPrinter();
                        eventHandler = currentPrinter?.PrepareLine(tagStr, thread.GetCharacterState(), m_RuntimeState.TagEventHandler) ?? m_RuntimeState.TagEventHandler;
                    }

                    VoxUtility.Play(voxHandle);
                }

                if (!hadVox) {
                    fakeSubtitleData = new SubtitleDisplayData() {
                        CharacterId = charId,
                        Priority = ScriptUtility.ScriptPriorityToVoxPriority(thread.Priority()),
                        Subtitle = new SubtitleEntry(tagStr.RichTextString),
                        VoxHandle = VoxRequestHandle.Dummy,
                        Tag = VoxTag
                    };
                } else {
                    fakeSubtitleData = default;
                }
            } else {
                fakeSubtitleData = default;
            }

            // NODES

            bool sentFakeSubtitleData = false;
            int visibleCount = 0,
                richCount = 0;
            for(; nodeIndex < tagStr.NodeCount; nodeIndex++) {
                TagNodeData node = tagStr.GetNode(nodeIndex);
                switch (node.Type) {
                    case TagNodeType.Event: {
                        if (thread.IsSkipping() && m_RuntimeState.SkippableTagEvents.Contains(node.Event.Type)) {
                            continue;
                        }

                        IEnumerator coroutine;
                        if (eventHandler.TryEvaluate(node.Event, thread, out coroutine)) {
                            if (!cachedHandle.IsRunning()) {
                                yield break;
                            }

                            if (coroutine != null) {
                                yield return coroutine;
                            }

                            if (thread.GetPrinter() != currentPrinter) {
                                currentPrinter = thread.GetPrinter();
                                eventHandler = currentPrinter?.PrepareLine(tagStr, thread.GetCharacterState(), m_RuntimeState.TagEventHandler) ?? m_RuntimeState.TagEventHandler;
                                currentPrinter?.FastForwardLine(visibleCount, richCount);
                            }
                        }

                        break;
                    }

                    case TagNodeType.Text: {
                        visibleCount = node.Text.VisibleCharacterOffset + node.Text.VisibleCharacterCount;
                        richCount = node.Text.RichCharacterOffset + node.Text.RichCharacterCount;

                        if (thread.IsSkipping()) {
                            continue;
                        }

                        if (dialogBoxDesired) {
                            yield return Routine.Inline(thread.GetPrinter()?.TypeLine(tagStr, node.Text, thread.GetCharacterState()));
                        } else if (voxDesired && !hadVox && !sentFakeSubtitleData) {
                            SubtitleUtility.RequestDisplay(fakeSubtitleData);
                            sentFakeSubtitleData = true;
                        }
                        break;
                    }
                }
            }

            // COMPLETION

            if (!thread.IsSkipping()) {
                if (dialogBoxDesired && tagStr.RichText.Length > 0) {
                    yield return Routine.Inline(thread.GetPrinter()?.CompleteLine());
                } else if (voxDesired) {
                    if (hadVox) {
                        float voiceReleaseTime = thread.GetVoxReleaseTime();
                        if (voiceReleaseTime > 0) {
                            while (VoxUtility.IsPlaying(voxHandle) && VoxUtility.GetPlaybackPosition(voxHandle) < voiceReleaseTime) {
                                yield return null;
                            }
                            thread.ReleaseVox();
                        } else {
                            while (VoxUtility.IsPlaying(voxHandle)) {
                                yield return null;
                            }
                        }
                    } else {
                        float duration = fakeSubtitleData.Subtitle.Data.Length * 0.08f;
                        while ((duration -= Routine.DeltaTime) > 0 && !thread.PopSkipSingle()) {
                            yield return null;
                        }

                        SubtitleUtility.RequestDismiss(new SubtitleDismissData(fakeSubtitleData));
                    }
                }
            }

            yield return Routine.Command.BreakAndResume;
        }

        #endregion // Line

        #region Choice

        public IEnumerator ShowOptions(LeafThreadState<ScriptNode> inThreadState, LeafChoice inChoice) {
            return ExecuteChoice((ScriptThread) inThreadState, inChoice);
        }

        private IEnumerator ExecuteChoice(ScriptThread thread, LeafChoice choice) {
            IDialogueChoicePresenter choicePresenter = thread.GetChoicePresenter();
            if (choicePresenter == null) {
                thread.TakeOwnership(ScriptUtility.GetDialogueChoicePresenter(StringHash32.Null));
                choicePresenter = thread.GetChoicePresenter();
            }
            Assert.NotNull(choicePresenter, "ChoicePresenter must be assigned before ExecuteChoice is called");
            m_RuntimeState.OnLeafChoicePresented.Invoke(thread, choice);

            thread.BeginChoice();
            
            yield return Routine.Inline(choicePresenter.ShowOptions(choice, thread.PeekNode(), thread, thread.GetCharacterState()));
            
            Assert.True(choice.HasChosen(), "LeafChoice must be chosen during ShowOptions");
            m_RuntimeState.OnLeafChoiceChosen.Invoke(thread, choice);
            thread.EndChoice();
        }

        #endregion // Choice

        #region Lookups

        public bool TryLookupObject(StringHash32 inObjectId, LeafThreadState inThreadState, out object outObject) {
            bool result = m_RuntimeState.Actors.TryGet(inObjectId, out ScriptActor actor);
            outObject = actor;
            return result;
        }

        public bool TryLookupLine(StringHash32 inLineCode, LeafNode inLocalNode, out string outLine) {
            if (Loc.IsDefaultLanguage()) {
                outLine = null;
                return false;
            }
            // TODO: lookup from localization instead
            outLine = null;
            return false;
        }

        public bool TryLookupNode(StringHash32 inNodeId, ScriptNode inLocalNode, out ScriptNode outLeafNode) {
            return ScriptDBUtility.TryLookupNode(m_Database, inLocalNode, inNodeId, out outLeafNode);
        }

        #endregion // Lookups

        #region ILeafPlugin

        IMethodCache ILeafPlugin.MethodCache {
            get { return m_CachedMethodCache; }
        }

        public LeafRuntimeConfiguration Configuration {
            get { return m_Configuration; }
        }

        int ILeafPlugin.RandomInt(int inMin, int inMaxExclusive) {
            return m_RuntimeState.Random.Next(inMin, inMaxExclusive);
        }

        float ILeafPlugin.RandomFloat(float inMin, float inMax) {
            return m_RuntimeState.Random.NextFloat(inMin, inMax);
        }

        #endregion // ILeafPlugin

        #region ILeafVariableAccess

        VariantTableResolver ILeafVariableAccess.Resolver {
            get { return m_CachedResolver; }
        }

        #endregion // ILeafVariableAccess
    }
}