using RimMind.Contracts.Extension;

namespace RimMind.Bridge.RimTalk
{
    internal sealed class RimTalkDialogueSkipCheck : ISkipCheck
    {
        public string Id => "rimtalk_bridge_dialogue";
        public SkipCheckKind Kind => SkipCheckKind.Dialogue;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.DialogueGate.ShouldSkipDialogue(args.Pawn, args.Trigger);
    }
}
