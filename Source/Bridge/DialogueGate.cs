using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class DialogueGate
    {
        public static bool ShouldSkipDialogue(Pawn? pawn, string? triggerType)
        {
            // pawn reserved for future per-pawn gating; currently global gate.
            if (!RimTalkDetector.IsRimTalkActive) return false;

            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enableDialogueGate) return false;

            return triggerType switch
            {
                "Chitchat" => settings.skipChitchat,
                "Auto" => settings.skipAutoDialogue,
                "PlayerInput" => settings.skipPlayerDialogue,
                _ => false
            };
        }

        public static bool ShouldSkipFloatMenuOption()
        {
            if (!RimTalkDetector.IsRimTalkActive) return false;

            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enableDialogueGate) return false;

            return settings.skipPlayerDialogue && !settings.forceRimMindPlayerDialogue;
        }
    }
}
