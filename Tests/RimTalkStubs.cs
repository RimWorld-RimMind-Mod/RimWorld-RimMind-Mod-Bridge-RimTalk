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

    public class Map { }

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
