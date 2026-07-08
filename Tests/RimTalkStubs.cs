using RimMind.Application.Common.Interfaces.Extension;

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

namespace RimMind.Application.Common.Interfaces.Extension
{
    public interface IExtension
    {
        string Id { get; }
        string OwnerModId { get; }
    }

    public interface IBridgeModule : IExtension
    {
        bool IsRegistered { get; }
        void Register();
        void Unregister();
    }
}

namespace RimMind.Bridge.RimTalk.Bridge
{
    public sealed class ContextPullBridge : IBridgeModule
    {
        public string Id => "context_pull";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public bool IsRegistered { get; set; }
        public bool RegisterCalled { get; set; }
        public bool UnregisterCalled { get; set; }
        public void Register() { if (IsRegistered) return; RegisterCalled = true; IsRegistered = true; }
        public void Unregister() { if (!IsRegistered) return; UnregisterCalled = true; IsRegistered = false; }
    }

    public sealed class ContextPushBridge : IBridgeModule
    {
        public string Id => "context_push";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public bool IsRegistered { get; set; }
        public bool RegisterCalled { get; set; }
        public bool UnregisterCalled { get; set; }
        public void Register() { if (IsRegistered) return; RegisterCalled = true; IsRegistered = true; }
        public void Unregister() { if (!IsRegistered) return; UnregisterCalled = true; IsRegistered = false; }
    }

    public sealed class PersonaPushBridge : IBridgeModule
    {
        public string Id => "persona_push";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public bool IsRegistered { get; set; }
        public bool RegisterCalled { get; set; }
        public bool UnregisterCalled { get; set; }
        public void Register() { if (IsRegistered) return; RegisterCalled = true; IsRegistered = true; }
        public void Unregister() { if (!IsRegistered) return; UnregisterCalled = true; IsRegistered = false; }
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

namespace RimMind.Personality.Data
{
    public class PersonalityProfile
    {
        public string description = string.Empty;
        public string workTendencies = string.Empty;
        public string socialTendencies = string.Empty;
        public string aiNarrative = string.Empty;

        public bool IsEmpty =>
            string.IsNullOrEmpty(description) &&
            string.IsNullOrEmpty(workTendencies) &&
            string.IsNullOrEmpty(socialTendencies) &&
            string.IsNullOrEmpty(aiNarrative);
    }
}

namespace HarmonyLib
{
    public static class AccessTools
    {
        public static System.Type? TypeByName(string name) => null;
    }
}
