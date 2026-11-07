using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimTalk
{
    internal sealed class RimTalkDialogueSkipCheck : IChannelSkipCheck
    {
        public string Id => "rimtalk_bridge_dialogue";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public SkipCheckKind Kind => SkipCheckKind.Dialogue;
        public string Channel => SkipCheckChannels.Dialogue;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.DialogueGate.ShouldSkipDialogue(args.Pawn as Verse.Pawn, args.Trigger ?? args.Tag);
    }
}
