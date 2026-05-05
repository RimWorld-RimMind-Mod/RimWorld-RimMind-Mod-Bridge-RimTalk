namespace Verse
{
    public static class Log
    {
        public static void Warning(string msg) { }
        public static void Message(string msg) { }
        public static void Error(string msg) { }
    }

    public static class ModsConfig
    {
        public static bool IsActive(string packageId) => false;
    }

    public class Pawn { }
}

namespace RimMind.Core
{
    public static class RimMindAPI
    {
        public static int DialogueSkipCheckCount { get; set; }
        public static int FloatMenuSkipCheckCount { get; set; }

        public static void RegisterDialogueSkipCheck(string sourceId, System.Func<Verse.Pawn, string, bool> check)
        {
            DialogueSkipCheckCount++;
        }

        public static void RegisterFloatMenuSkipCheck(string sourceId, System.Func<bool> check)
        {
            FloatMenuSkipCheckCount++;
        }

        public static void ResetCounts()
        {
            DialogueSkipCheckCount = 0;
            FloatMenuSkipCheckCount = 0;
        }
    }
}

namespace RimMind.Bridge.RimTalk.Detection
{
    public static class RimTalkDetector
    {
        public static bool IsRimTalkActive { get; set; }
        public static bool IsRimTalkApiAvailable { get; set; }
    }
}

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class ContextPullBridge
    {
        public static bool RegisterCalled { get; set; }
        public static bool UnregisterCalled { get; set; }
        public static void Register() { RegisterCalled = true; }
        public static void Unregister() { UnregisterCalled = true; }
    }

    public static class ContextPushBridge
    {
        public static bool RegisterCalled { get; set; }
        public static bool UnregisterCalled { get; set; }
        public static void Register() { RegisterCalled = true; }
        public static void Unregister() { UnregisterCalled = true; }
    }

    public static class PersonaPushBridge
    {
        public static bool RegisterCalled { get; set; }
        public static bool UnregisterCalled { get; set; }
        public static void Register() { RegisterCalled = true; }
        public static void Unregister() { UnregisterCalled = true; }
    }
}

namespace RimMind.Bridge.RimTalk.Settings
{
    public class BridgeRimTalkSettings
    {
        public bool enableDialogueGate = true;
        public bool skipChitchat = true;
        public bool skipAutoDialogue = true;
        public bool skipPlayerDialogue = true;
        public bool forceRimMindPlayerDialogue = false;

        public bool enableContextPush = true;
        public bool pushPersonality = true;
        public bool pushStoryteller = true;
        public bool pushMemory = false;
        public bool pushAdvisorLog = true;
        public bool pushShaping = false;

        public bool enableContextPull = true;
        public bool pullRimTalkHistory = true;

        private static BridgeRimTalkSettings? _instance;
        public static BridgeRimTalkSettings Get() => _instance ??= new BridgeRimTalkSettings();

        public BridgeRimTalkSettings() { _instance = this; }

        public static void Reset() { _instance = null; }
    }
}
