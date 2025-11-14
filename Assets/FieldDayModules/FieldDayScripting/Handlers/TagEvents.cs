using BeauUtil;
using BeauUtil.Tags;
using Leaf;

namespace FieldDay.Scripting {
    static public class TagEvents {
        static public readonly StringHash32 ConfigureVoxOverlap = "vox-overlap";
        static public readonly StringHash32 DispatchEvent = "dispatch-event";
        static public readonly StringHash32 SubtitleTimecodes = "subtitle-timecodes";
        static public readonly StringHash32 OverrideCharName = "override-character-name";
        static public readonly StringHash32 HasVox = "vox-present";
        static public readonly StringHash32 HasNoVox = "vox-not-present";
        static public readonly StringHash32 VoxOnly = "vox-only";
        static public readonly StringHash32 SetStyle = "set-style";

        static internal void ConfigureParsers(CustomTagParserConfig parser, ILeafPlugin plugin) {
            LeafUtils.ConfigureDefaultParsers(parser, plugin, null);

            parser.AddEvent("vox", HasVox);
            parser.AddEvent("vox-only", VoxOnly);
            parser.AddEvent("no-vox", HasNoVox);
            parser.AddEvent("vox-overlap", ConfigureVoxOverlap).WithFloatData(-0.2f);

            parser.AddEvent("char-name", OverrideCharName).WithStringData();
            parser.AddEvent("srt", SubtitleTimecodes).WithFloatData();
            parser.AddEvent("dispatch-event", DispatchEvent).WithStringHashData();
            parser.AddEvent("style", SetStyle).WithStringHashData();

            parser.AddReplace("icon", ReplaceIcon);
        }

        static internal void ConfigureHandlers(TagStringEventHandler handler, ILeafPlugin plugin) {
            LeafUtils.ConfigureDefaultHandlers(handler, plugin);

            handler.Register(ConfigureVoxOverlap, Event_VoxOverlap);
            handler.Register(DispatchEvent, Event_DispatchEvent);
            handler.Register(OverrideCharName, Event_SetCharacterName);
            handler.Register(LeafUtils.Events.Character, Event_SetCharacter);
            handler.Register(LeafUtils.Events.Pose, Event_SetPose);
            handler.Register(SetStyle, Event_SetStyle);

            handler.Register(SubtitleTimecodes, Event_NoOp);
            handler.Register(HasVox, Event_NoOp);
            handler.Register(HasNoVox, Event_NoOp);
            handler.Register(VoxOnly, Event_NoOp);
        }

        /// <summary>
        /// No-op event.
        /// </summary>
        static public readonly TagStringEventHandler.InstantEventWithContextDelegate Event_NoOp = (e, o) => { };

        static private string ReplaceIcon(TagData tag, object context) {
            return string.Format("<sprite name=\"{0}\">", tag.Data.ToString());
        }

        static private void Event_VoxOverlap(TagEventData evt, object context) {
            var thread = (ScriptThread) context;
            thread.SetVoxReleaseTime(evt.GetFloat());
        }

        static private void Event_SetCharacter(TagEventData evt, object context) {
            var thread = (ScriptThread) context;
            thread.SetCharacterState(new DialogueCharacterState() {
                CharacterId = evt.Argument0.AsStringHash(),
                PoseId = evt.Argument1.AsStringHash()
            });
        }

        static private void Event_SetPose(TagEventData evt, object context) {
            var thread = (ScriptThread)context;
            var character = thread.GetCharacterState();
            character.PoseId = evt.Argument0.AsStringHash();
            thread.SetCharacterState(character);
        }

        static private void Event_SetCharacterName(TagEventData evt, object context) {
            var thread = (ScriptThread)context;
            var character = thread.GetCharacterState();
            character.OverrideName = evt.StringArgument.ToString();
            thread.SetCharacterState(character);
        }

        static private void Event_SetStyle(TagEventData evt, object context) {
            var thread = (ScriptThread)context;
            StringHash32 style = evt.Argument0.AsStringHash();
            thread.TakeOwnership(ScriptUtility.GetDialoguePrinter(style));
        }

        static private void Event_DispatchEvent(TagEventData evt, object context) {
            Game.Events.Dispatch(evt.GetStringHash());
        }
    }
}