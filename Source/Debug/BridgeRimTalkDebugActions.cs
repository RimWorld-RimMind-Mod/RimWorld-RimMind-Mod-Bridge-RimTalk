using System.Linq;
using System.Text;
using LudeonTK;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Core;
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
                Log.Warning("[RimMind-Bridge-RimTalk] No pawn selected.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[RimMind-Bridge-RimTalk] Dialogue Gate Test for {pawn.Name.ToStringShort}:");
            sb.AppendLine($"  ShouldSkipDialogue(Chitchat): {RimMindAPI.ShouldSkipDialogue(pawn, "Chitchat")}");
            sb.AppendLine($"  ShouldSkipDialogue(Auto): {RimMindAPI.ShouldSkipDialogue(pawn, "Auto")}");
            sb.AppendLine($"  ShouldSkipDialogue(PlayerInput): {RimMindAPI.ShouldSkipDialogue(pawn, "PlayerInput")}");
            sb.AppendLine($"  ShouldSkipFloatMenu: {RimMindAPI.ShouldSkipFloatMenu()}");

            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimTalk", "Show Context Push State", actionType = DebugActionType.Action)]
        private static void ShowContextPushState()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[RimMind-Bridge-RimTalk] Context Push State:");

            var categories = RimMindAPI.GetRegisteredCategories();
            var rimtalkCategories = categories.Where(c => c.StartsWith("rimmind_") || c.StartsWith("rimtalk_")).ToList();
            sb.AppendLine($"  RimMind registered rimtalk-related categories: {(rimtalkCategories.Count > 0 ? string.Join(", ", rimtalkCategories) : "none")}");
            sb.AppendLine("  (Note: Push variables are registered in RimTalk, not RimMind)");

            var settings = BridgeRimTalkSettings.Get();
            sb.AppendLine("  Push Settings:");
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

            var categories = RimMindAPI.GetRegisteredCategories();
            var rimtalkCategories = categories.Where(c => c.StartsWith("rimtalk_")).ToList();
            sb.AppendLine($"  RimMind registered rimtalk-related providers: {(rimtalkCategories.Count > 0 ? string.Join(", ", rimtalkCategories) : "none")}");

            var settings = BridgeRimTalkSettings.Get();
            sb.AppendLine("  Pull Settings:");
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
    }
}
