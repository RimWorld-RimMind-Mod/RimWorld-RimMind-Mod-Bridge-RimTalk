using System;
using System.Linq;
using System.Text;
using LudeonTK;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimTalk.Debug
{
    [StaticConstructorOnStartup]
    public static class BridgeRimTalkDebugActions
    {
        [DebugAction("RimMind Bridge-RimTalk", "Show Bridge State", actionType = DebugActionType.Action)]
        private static void ShowBridgeState()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[RimMind-Bridge-RimTalk] Bridge State:");
            sb.AppendLine($"  RimTalkDetector.IsRimTalkActive: {RimTalkDetector.IsRimTalkActive}");
            sb.AppendLine($"  RimTalkDetector.IsRimTalkApiAvailable: {RimTalkDetector.IsRimTalkApiAvailable}");

            var settings = BridgeRimTalkSettings.Get();
            sb.AppendLine("  Settings:");
            sb.AppendLine($"    enableDialogueGate: {settings.enableDialogueGate}");
            sb.AppendLine($"    enableContextPush: {settings.enableContextPush}");
            sb.AppendLine($"    enableContextPull: {settings.enableContextPull}");

            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimTalk", "Test Dialogue Gate (selected)", actionType = DebugActionType.Action)]
        private static void TestDialogueGateSelected()
        {
            var pawn = Find.Selector?.SingleSelectedThing as Pawn;
            if (pawn == null)
            {
                RimMindErrors.Warn("[RimMind-Bridge-RimTalk] No pawn selected.");
                return;
            }

            string pawnName = pawn.Name?.ToStringShort ?? pawn.LabelShort ?? "?";
            var sb = new StringBuilder();
            sb.AppendLine($"[RimMind-Bridge-RimTalk] Dialogue Gate Test for {pawnName}:");
            sb.AppendLine($"  ShouldSkipDialogue(Chitchat): {RimMindAPI.Ext.ShouldSkipDialogue(pawn, "Chitchat")}");
            sb.AppendLine($"  ShouldSkipDialogue(Auto): {RimMindAPI.Ext.ShouldSkipDialogue(pawn, "Auto")}");
            sb.AppendLine($"  ShouldSkipDialogue(PlayerInput): {RimMindAPI.Ext.ShouldSkipDialogue(pawn, "PlayerInput")}");
            sb.AppendLine($"  ShouldSkipFloatMenu: {RimMindAPI.Ext.ShouldSkipFloatMenu()}");

            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimTalk", "Show Context Push State", actionType = DebugActionType.Action)]
        private static void ShowContextPushState()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[RimMind-Bridge-RimTalk] Context Push State:");

            var categories = RimMindAPI.Providers.GetRegisteredCategories();
            var sourceProviders = categories
                .Where(c => c.StartsWith("personality.", StringComparison.Ordinal)
                    || c.StartsWith("memory.", StringComparison.Ordinal)
                    || c.StartsWith("advisor.", StringComparison.Ordinal))
                .ToList();
            sb.AppendLine($"  RimMind source providers available ({sourceProviders.Count}): {(sourceProviders.Count > 0 ? string.Join(", ", sourceProviders) : "none")}");
            sb.AppendLine("  (Note: Push variables are registered in RimTalk via RimTalkApiShim)");

            var settings = BridgeRimTalkSettings.Get();
            sb.AppendLine("  Push Settings:");
            sb.AppendLine($"    enableContextPush: {settings.enableContextPush}");
            sb.AppendLine($"    pushPersonality: {settings.pushPersonality}");
            sb.AppendLine($"    pushStoryteller: {settings.pushStoryteller}");
            sb.AppendLine($"    pushMemory: {settings.pushMemory}");
            sb.AppendLine($"    pushAdvisorLog: {settings.pushAdvisorLog}");
            sb.AppendLine($"    pushShaping: {settings.pushShaping}");
            sb.AppendLine($"    injectPersonaToTraits: {settings.injectPersonaToTraits}");
            sb.AppendLine($"    injectPersonaToMood: {settings.injectPersonaToMood}");

            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimTalk", "Show Context Pull State", actionType = DebugActionType.Action)]
        private static void ShowContextPullState()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[RimMind-Bridge-RimTalk] Context Pull State:");

            var contextKeys = RimMindAPI.Context.ContextKeys.GetAll();
            var rimtalkKeys = contextKeys
                .Where(k => k.Key.StartsWith("rimtalk_", StringComparison.Ordinal))
                .ToList();
            sb.AppendLine($"  RimMind registered rimtalk context keys ({rimtalkKeys.Count}): {(rimtalkKeys.Count > 0 ? string.Join(", ", rimtalkKeys.Select(k => $"{k.Key} ({k.OwnerMod})")) : "none")}");

            var settings = BridgeRimTalkSettings.Get();
            sb.AppendLine("  Pull Settings:");
            sb.AppendLine($"    enableContextPull: {settings.enableContextPull}");
            sb.AppendLine($"    pullRimTalkHistory: {settings.pullRimTalkHistory}");

            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimTalk", "Test RimTalk API Shim", actionType = DebugActionType.Action)]
        private static void TestRimTalkApiShim()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[RimMind-Bridge-RimTalk] RimTalk API Shim Test:");
            sb.AppendLine($"  RimTalkApiShim.IsAvailable: {RimTalkApiShim.IsAvailable}");
            sb.AppendLine($"  RimTalkDetector.IsRimTalkActive: {RimTalkDetector.IsRimTalkActive}");
            sb.AppendLine($"  RimTalkDetector.IsRimTalkApiAvailable: {RimTalkDetector.IsRimTalkApiAvailable}");

            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimTalk", "Force Unregister Bridges", actionType = DebugActionType.Action)]
        private static void ForceUnregisterBridges()
        {
            RimTalkBridgeCoordinator.Unregister();
            Log.Message("[RimMind-Bridge-RimTalk] All bridges unregistered.");
        }
    }
}
