using System.Reflection;
using System.Text;
using HarmonyLib;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Core;
using RimMind.Core.Prompt;
using RimMind.Advisor.Data;
using RimMind.Memory.Data;
using RimMind.Personality.Data;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class ContextPushBridge
    {
        private const string ModId = "RimMind.Bridge.RimTalk";

        public static void Register()
        {
            if (!RimTalkDetector.IsRimTalkApiAvailable) return;

            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enableContextPush) return;

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

            if (settings.enableRimTalkHistoryPull)
                RegisterRimTalkHistoryProvider();
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
                        foreach (var m in store.dark)
                            sb.AppendLine($"- {m.content}");
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

            if (settings.pushPersonality)
            {
                sb.AppendLine("{{ for p in pawns }}");
                sb.AppendLine("## {{ p.name }}'s Personality:");
                sb.AppendLine("{{ p.rimmind_personality }}");
                sb.AppendLine("{{ end }}");
            }

            if (settings.pushStoryteller)
            {
                sb.AppendLine("# Storyteller State");
                sb.AppendLine("{{rimmind_storyteller}}");
            }

            RimTalkApiShim.AddPromptEntry(
                name: "RimMind Context",
                content: sb.ToString().TrimEnd(),
                roleValue: 0,
                positionValue: 0,
                sourceModId: ModId
            );
        }

        private static void RegisterRimTalkHistoryProvider()
        {
            RimMindAPI.RegisterPawnContextProvider("rimtalk_history", pawn =>
            {
                return BuildRimTalkHistoryContext(pawn);
            }, PromptSection.PriorityMemory, ModId);
        }

        private static string? BuildRimTalkHistoryContext(Pawn pawn)
        {
            if (!RimTalkDetector.IsRimTalkApiAvailable) return null;

            try
            {
                var talkHistoryType = AccessTools.TypeByName("RimTalk.Data.TalkHistory");
                if (talkHistoryType == null) return null;

                var getMessageHistoryMethod = talkHistoryType.GetMethod("GetMessageHistory",
                    BindingFlags.Public | BindingFlags.Static);
                if (getMessageHistoryMethod == null) return null;

                var messages = getMessageHistoryMethod.Invoke(null,
                    new object?[] { pawn, true }) as System.Collections.IEnumerable;
                if (messages == null) return null;

                var sb = new StringBuilder("[RimTalk Dialog History]");
                int count = 0;
                foreach (var msg in messages)
                {
                    if (count >= 6) break;
                    var msgType = msg.GetType();

                    var roleField = msgType.GetField("Item1",
                        BindingFlags.Public | BindingFlags.Instance);
                    var messageField = msgType.GetField("Item2",
                        BindingFlags.Public | BindingFlags.Instance);

                    if (roleField == null || messageField == null) continue;

                    var roleValue = roleField.GetValue(msg);
                    var content = messageField.GetValue(msg)?.ToString() ?? "";

                    if (string.IsNullOrEmpty(content)) continue;

                    string roleLabel = roleValue?.ToString() ?? "?";
                    if (roleValue is int roleInt)
                    {
                        roleLabel = roleInt switch
                        {
                            0 => "System",
                            1 => "User",
                            2 => "AI",
                            _ => roleInt.ToString()
                        };
                    }

                    sb.AppendLine($"[{roleLabel}] {content}");
                    count++;
                }
                return count > 0 ? sb.ToString().TrimEnd() : null;
            }
            catch
            {
                return null;
            }
        }

        public static void Unregister()
        {
            RimTalkApiShim.Cleanup(ModId);
            RimMindAPI.UnregisterPawnContextProvider("rimtalk_history");
        }
    }
}
