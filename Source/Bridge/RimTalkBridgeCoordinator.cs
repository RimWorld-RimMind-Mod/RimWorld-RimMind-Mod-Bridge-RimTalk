using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Contracts.Result;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class RimTalkBridgeCoordinator
    {
        public static void Register()
        {
            if (!RimTalkDetector.IsRimTalkActive)
            {
                Log.Message("[RimMind-Bridge-RimTalk] RimTalk not active, bridge modules skipped.");
                return;
            }

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
                RimMindErrors.Warn("[RimMind-Bridge-RimTalk] RimTalk API not available, push modules skipped.");
            }

            Log.Message("[RimMind-Bridge-RimTalk] Initialized.");
        }

        public static void Unregister()
        {
            ContextPullBridge.Unregister();
            ContextPushBridge.Unregister();
            PersonaPushBridge.Unregister();
        }
    }
}
