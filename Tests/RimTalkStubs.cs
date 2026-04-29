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
    public static class DialogueGate
    {
        public static bool RegisterSkipChecksCalled { get; set; }
        public static void RegisterSkipChecks() { RegisterSkipChecksCalled = true; }
    }

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
        private static BridgeRimTalkSettings _instance = new BridgeRimTalkSettings();
        public static BridgeRimTalkSettings Get() => _instance;

        public bool pushPersonality { get; set; }

        public static void Reset() { _instance = new BridgeRimTalkSettings(); }
    }
}
