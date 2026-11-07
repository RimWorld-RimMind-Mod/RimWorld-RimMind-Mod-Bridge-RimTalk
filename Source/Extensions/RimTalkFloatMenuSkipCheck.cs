using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimTalk
{
    internal sealed class RimTalkFloatMenuSkipCheck : IChannelSkipCheck
    {
        public string Id => "rimtalk_bridge_floatmenu";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public SkipCheckKind Kind => SkipCheckKind.FloatMenu;
        public string Channel => SkipCheckChannels.FloatMenu;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.DialogueGate.ShouldSkipFloatMenuOption();
    }
}
