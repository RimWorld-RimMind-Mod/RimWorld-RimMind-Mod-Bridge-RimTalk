using HarmonyLib;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Core;
using Verse;

namespace RimMind.Bridge.RimTalk
{
    public class RimMindBridgeRimTalkMod : Mod
    {
        public static BridgeRimTalkSettings Settings = null!;

        public RimMindBridgeRimTalkMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<BridgeRimTalkSettings>();
            new Harmony("mcocdaa.RimMindBridgeRimTalk").PatchAll();

            RimMindAPI.RegisterSettingsTab("bridge_rimtalk",
                () => "RimMind.BridgeRimTalk.Settings.TabLabel".Translate(),
                BridgeRimTalkSettings.DrawSettingsContent);

            if (!RimTalkDetector.IsRimTalkActive)
            {
                Log.Message("[RimMind-Bridge-RimTalk] RimTalk not active, bridge modules skipped.");
                return;
            }

            DialogueGate.RegisterSkipChecks();
            Log.Message("[RimMind-Bridge-RimTalk] DialogueGate registered.");

            ContextPullBridge.Register();
            Log.Message("[RimMind-Bridge-RimTalk] ContextPull registered.");

            if (RimTalkDetector.IsRimTalkApiAvailable)
            {
                ContextPushBridge.Register();
                Log.Message("[RimMind-Bridge-RimTalk] ContextPush registered.");

                if (BridgeRimTalkSettings.Get().pushPersonality)
                {
                    PersonaPushBridge.Register();
                    Log.Message("[RimMind-Bridge-RimTalk] PersonaPush registered.");
                }
            }
            else
            {
                Log.Warning("[RimMind-Bridge-RimTalk] RimTalk API not available, push modules skipped.");
            }

            Log.Message("[RimMind-Bridge-RimTalk] Initialized.");
        }

        public override string SettingsCategory() => "RimMind.BridgeRimTalk.Settings.Category".Translate();

        public override void DoSettingsWindowContents(UnityEngine.Rect rect)
        {
            BridgeRimTalkSettings.DrawSettingsContent(rect);
        }
    }
}
