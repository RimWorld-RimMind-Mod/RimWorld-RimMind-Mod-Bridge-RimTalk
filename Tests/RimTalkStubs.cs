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

    public class TickManager
    {
        public int TicksGame;
    }

    public static class Find
    {
        public static TickManager? TickManager;
    }
}

namespace RimMind.Presentation.Api
{
    public static class RimMindAPI
    {
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

namespace HarmonyLib
{
    public static class AccessTools
    {
        public static System.Type? TypeByName(string name) => null;
    }
}
