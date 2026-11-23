using System;
using System.Collections.Concurrent;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    internal static class RimMindProviderReader
    {
        internal const string PersonalityDescription = "personality.description";
        internal const string PersonalityWorkTendencies = "personality.work_tendencies";
        internal const string PersonalitySocialTendencies = "personality.social_tendencies";
        internal const string PersonalityNarrative = "personality.ai_narrative";
        internal const string PersonalityShaping = "personality.shaping_history";
        internal const string PawnMemoryBrief = "memory.pawn_brief";
        internal const string NarratorMemoryBrief = "memory.narrator_brief";
        internal const string AdvisorHistoryBrief = "advisor.history_brief";

        private static readonly ConcurrentDictionary<string, byte> WarnedCategories =
            new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

        internal static void ResetForTesting() => WarnedCategories.Clear();

        internal static string GetPawn(string category, Pawn? pawn)
        {
            if (pawn == null || string.IsNullOrEmpty(category))
                return string.Empty;

            return Resolve(category, RimMindAPI.Providers.GetProviderData(category, pawn));
        }

        internal static string GetStatic(string category)
        {
            if (string.IsNullOrEmpty(category))
                return string.Empty;

            return Resolve(category, RimMindAPI.Providers.GetStaticProviderData(category));
        }

        private static string Resolve(
            string category,
            Result<string?, RimMindError> result)
        {
            if (result.IsOk)
                return result.Value ?? string.Empty;

            string errorMessage = result.Error.Message ?? string.Empty;
            if (errorMessage.StartsWith("No provider registered", StringComparison.Ordinal)
                || errorMessage.StartsWith("No static provider registered", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            if (WarnedCategories.ContainsKey(category))
                return string.Empty;

            if (RimMindAPI.Providers.GetRegisteredCategories().Contains(category)
                && WarnedCategories.TryAdd(category, 1))
            {
                Log.WarningOnce(
                    $"[RimMind-Bridge-RimTalk] Provider '{category}' failed: {result.Error}",
                    StringComparer.Ordinal.GetHashCode(category));
            }

            return string.Empty;
        }
    }
}
