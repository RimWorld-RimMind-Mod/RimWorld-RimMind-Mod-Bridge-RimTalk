using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Core;
using Verse;

namespace RimMind.Bridge.RimTalk
{
    public class RimMindBridgeRimTalkMod : Mod
    {
        public RimMindBridgeRimTalkMod(ModContentPack content) : base(content)
        {
            GetSettings<BridgeRimTalkSettings>();

            RimMindAPI.RegisterSettingsTab("bridge_rimtalk",
                () => "RimMind.BridgeRimTalk.Settings.TabLabel".Translate(),
                BridgeRimTalkSettings.DrawSettingsContent);

            RimTalkBridgeCoordinator.Register();
        }

        public override string SettingsCategory() => "RimMind.BridgeRimTalk.Settings.Category".Translate();

        public override void DoSettingsWindowContents(UnityEngine.Rect rect)
        {
            BridgeRimTalkSettings.DrawSettingsContent(rect);
        }
    }
}
