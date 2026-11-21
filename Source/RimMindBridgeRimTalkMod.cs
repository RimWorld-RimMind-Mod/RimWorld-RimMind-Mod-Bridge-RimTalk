using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Presentation;
using RimMind.Presentation.Api;
using RimMind.Presentation.Settings;
using Verse;

namespace RimMind.Bridge.RimTalk
{
    public class RimMindBridgeRimTalkMod : RimMindSubmodBase<BridgeRimTalkSettings>
    {
        public static new BridgeRimTalkSettings Settings = null!;

        public RimMindBridgeRimTalkMod(ModContentPack content) : base(content)
        {
            Settings = base.Settings;

            RimMindAPI.Ext.Get<ISettingsTab>().Register(new RimTalkSettingsTab());
            RimTalkBridgeCoordinator.Register();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect rect)
        {
            BridgeRimTalkSettings.DrawSettingsContent(rect);
        }
    }
}
