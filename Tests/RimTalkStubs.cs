using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Domain.ValueObjects;

namespace Verse
{
    public static class Log
    {
        private static readonly HashSet<int> WarningKeys = new HashSet<int>();
        public static List<string> Warnings { get; } = new List<string>();

        public static void Warning(string msg) => Warnings.Add(msg);
        public static void WarningOnce(string msg, int key)
        {
            if (WarningKeys.Add(key))
                Warnings.Add(msg);
        }

        public static void Message(string msg) { }
        public static void Error(string msg) { }

        public static void Reset()
        {
            WarningKeys.Clear();
            Warnings.Clear();
        }
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
        public static class Providers
        {
            public static Result<string?, RimMindError> PawnResult { get; set; } = Missing();
            public static Result<string?, RimMindError> StaticResult { get; set; } = Missing();
            public static List<string> Categories { get; } = new List<string>();

            public static Result<string?, RimMindError> GetProviderData(
                string category,
                Verse.Pawn pawn) => PawnResult;

            public static Result<string?, RimMindError> GetStaticProviderData(
                string category) => StaticResult;

            public static List<string> GetRegisteredCategories() =>
                new List<string>(Categories);

            public static void Reset()
            {
                PawnResult = Missing();
                StaticResult = Missing();
                Categories.Clear();
            }

            private static Result<string?, RimMindError> Missing() =>
                Result<string?, RimMindError>.Err(RimMindErrors.Internal("missing"));
        }
    }
}

namespace HarmonyLib
{
    public static class AccessTools
    {
        public static System.Type? TypeByName(string name) => null;
    }
}
