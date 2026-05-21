using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimTalk
{
    internal sealed class RimTalkDialogueSkipCheck : ISkipCheck
    {
        public string Id => "rimtalk_bridge_dialogue";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public SkipCheckKind Kind => SkipCheckKind.Dialogue;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.DialogueGate.ShouldSkipDialogue((Verse.Pawn)args.Pawn, args.Trigger);
    }
}
