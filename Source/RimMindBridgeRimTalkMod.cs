using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Presentation.Api;
using RimMind.Presentation.Settings;
using Verse;

namespace RimMind.Bridge.RimTalk
{
    public class RimMindBridgeRimTalkMod : Mod
    {
        public RimMindBridgeRimTalkMod(ModContentPack content) : base(content)
        {
            GetSettings<BridgeRimTalkSettings>();

            RimMindAPI.Extensions<ISettingsTab>().Register(new RimTalkSettingsTab());

            if (RimTalkDetector.IsRimTalkActive)
            {
                RimMindAPI.Extensions<ISkipCheck>().Register(new RimTalkDialogueSkipCheck());
                RimMindAPI.Extensions<ISkipCheck>().Register(new RimTalkFloatMenuSkipCheck());
            }

            RimTalkBridgeCoordinator.Register();
        }

        public override string SettingsCategory() => "RimMind.BridgeRimTalk.Settings.Category".Translate();

        public override void DoSettingsWindowContents(UnityEngine.Rect rect)
        {
            BridgeRimTalkSettings.DrawSettingsContent(rect);
        }
    }
}
