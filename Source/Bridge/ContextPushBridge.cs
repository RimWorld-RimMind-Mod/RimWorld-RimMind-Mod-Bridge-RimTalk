using System.Text;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Advisor.Data;
using RimMind.Memory.Data;
using RimMind.Personality.Data;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class ContextPushBridge
    {
        private const string ModId = "RimMind.Bridge.RimTalk.Push";

        public static void Register()
        {
            if (!RimTalkDetector.IsRimTalkApiAvailable) return;

            var settings = BridgeRimTalkSettings.Get();

            if (settings.enableContextPush)
            {
                if (settings.pushPersonality)
                    RegisterPersonalityVariable();

                if (settings.pushStoryteller)
                    RegisterStorytellerVariable();

                if (settings.pushMemory)
                    RegisterMemoryVariable();

                if (settings.pushShaping)
                    RegisterShapingVariable();

                if (settings.pushAdvisorLog)
                    RegisterAdvisorLogVariable();

                RegisterPromptEntry();
            }
        }

        private static void RegisterPersonalityVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_personality",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null || profile.IsEmpty) return "";

                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(profile.description))
                        sb.AppendLine(profile.description);
                    if (!string.IsNullOrEmpty(profile.workTendencies))
                        sb.AppendLine($"[Work] {profile.workTendencies}");
                    if (!string.IsNullOrEmpty(profile.socialTendencies))
                        sb.AppendLine($"[Social] {profile.socialTendencies}");
                    if (!string.IsNullOrEmpty(profile.aiNarrative))
                        sb.AppendLine($"[AI] {profile.aiNarrative}");
                    return sb.ToString().TrimEnd();
                },
                "RimMind personality profile",
                50
            );
        }

        private static void RegisterStorytellerVariable()
        {
            RimTalkApiShim.RegisterEnvironmentVariable(
                ModId,
                "rimmind_storyteller",
                map =>
                {
                    var store = RimMindMemoryWorldComponent.Instance?.NarratorStore;
                    if (store == null || store.IsEmpty) return "";

                    var sb = new StringBuilder("[RimMind Storyteller]");
                    int count = 0;
                    foreach (var m in store.active)
                    {
                        if (count >= 5) break;
                        sb.AppendLine($"- {m.content}");
                        count++;
                    }
                    return sb.ToString().TrimEnd();
                },
                "RimMind storyteller state",
                80
            );
        }

        private static void RegisterMemoryVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_memory",
                pawn =>
                {
                    var store = RimMindMemoryWorldComponent.Instance?.GetOrCreatePawnStore(pawn);
                    if (store == null || store.IsEmpty) return "";

                    var sb = new StringBuilder("[RimMind Memory]");
                    int count = 0;
                    foreach (var m in store.active)
                    {
                        if (count >= 5) break;
                        sb.AppendLine($"- {m.content}");
                        count++;
                    }
                    if (store.dark.Count > 0)
                    {
                        sb.AppendLine("[Long-term]");
                        int darkCount = 0;
                        foreach (var m in store.dark)
                        {
                            if (darkCount >= 5) break;
                            sb.AppendLine($"- {m.content}");
                            darkCount++;
                        }
                    }
                    return sb.ToString().TrimEnd();
                },
                "RimMind memory data",
                60
            );
        }

        private static void RegisterShapingVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_shaping",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null) return "";
                    var history = profile.playerShapingHistory;
                    if (history == null || history.Count == 0) return "";

                    var sb = new StringBuilder("[RimMind Shaping]");
                    int count = 0;
                    int start = System.Math.Max(0, history.Count - 5);
                    for (int i = start; i < history.Count; i++)
                    {
                        var r = history[i];
                        sb.AppendLine($"- [{r.action}] {r.label}");
                        count++;
                    }
                    return count > 0 ? sb.ToString().TrimEnd() : "";
                },
                "RimMind shaping history",
                70
            );
        }

        private static void RegisterAdvisorLogVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_advisor_log",
                pawn =>
                {
                    var history = AdvisorHistoryStore.Instance?.GetRecords(pawn);
                    if (history == null || history.Count == 0) return "";

                    var sb = new StringBuilder("[RimMind Advisor]");
                    int count = 0;
                    foreach (var r in history)
                    {
                        if (count >= 5) break;
                        sb.AppendLine($"- {r.action}: {r.reason} ({r.result})");
                        count++;
                    }
                    return sb.ToString().TrimEnd();
                },
                "RimMind advisor history",
                80
            );
        }

        private static void RegisterPromptEntry()
        {
            var settings = BridgeRimTalkSettings.Get();

            var sb = new StringBuilder();
            sb.AppendLine("# RimMind Context");
            bool hasContent = false;

            if (settings.pushPersonality)
            {
                sb.AppendLine("{{ for p in pawns }}");
                sb.AppendLine("## {{ p.name }}'s Personality:");
                sb.AppendLine("{{ p.rimmind_personality }}");
                sb.AppendLine("{{ end }}");
                hasContent = true;
            }

            if (settings.pushStoryteller)
            {
                sb.AppendLine("# Storyteller State");
                sb.AppendLine("{{rimmind_storyteller}}");
                hasContent = true;
            }

            if (settings.pushMemory)
            {
                sb.AppendLine("{{ for p in pawns }}");
                sb.AppendLine("## {{ p.name }}'s Memory:");
                sb.AppendLine("{{ p.rimmind_memory }}");
                sb.AppendLine("{{ end }}");
                hasContent = true;
            }

            if (settings.pushAdvisorLog)
            {
                sb.AppendLine("{{ for p in pawns }}");
                sb.AppendLine("## {{ p.name }}'s Advisor Log:");
                sb.AppendLine("{{ p.rimmind_advisor_log }}");
                sb.AppendLine("{{ end }}");
                hasContent = true;
            }

            if (settings.pushShaping)
            {
                sb.AppendLine("{{ for p in pawns }}");
                sb.AppendLine("## {{ p.name }}'s Shaping History:");
                sb.AppendLine("{{ p.rimmind_shaping }}");
                sb.AppendLine("{{ end }}");
                hasContent = true;
            }

            if (!hasContent) return;

            RimTalkApiShim.AddPromptEntry(
                name: "RimMind Context",
                content: sb.ToString().TrimEnd(),
                roleValue: 0,
                positionValue: 0,
                sourceModId: ModId
            );
        }

        public static void Unregister()
        {
            RimTalkApiShim.Cleanup(ModId);
        }
    }
}
