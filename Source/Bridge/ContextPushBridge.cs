using System.Text;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Advisor.Data;
using RimMind.Memory.Data;
using RimMind.Personality.Data;
using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public sealed class ContextPushBridge : IBridgeModule
    {
        private const string ModId = "RimMind.Bridge.RimTalk.Push";

        public string Id => "context_push";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public bool IsRegistered { get; private set; }

        public void Register()
        {
            if (IsRegistered) return;
            if (!RimTalkDetector.IsRimTalkApiAvailable) return;

            var settings = BridgeRimTalkSettings.Get();
            RimTalkContextPushPlan plan = RimTalkContextPushPlan.Build(settings);

            if (settings.enableContextPush)
            {
                if (plan.RegisterPersonality)
                    RegisterPersonalityVariable();

                if (plan.RegisterStoryteller)
                    RegisterStorytellerVariable();

                if (plan.RegisterMemory)
                    RegisterMemoryVariable();

                if (plan.RegisterShaping)
                    RegisterShapingVariable();

                if (plan.RegisterAdvisorLog)
                    RegisterAdvisorLogVariable();

                RegisterPromptEntry(plan);
            }

            IsRegistered = true;
        }

        private void RegisterPersonalityVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_personality",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null || profile.IsEmpty) return "";

                    var sb = new StringBuilder();
                    sb.AppendLine(PersonaFormatter.BuildFullProfile(profile));
                    if (!string.IsNullOrEmpty(profile.aiNarrative))
                        sb.AppendLine($"[AI] {profile.aiNarrative}");
                    return sb.ToString().TrimEnd();
                },
                "RimMind personality profile",
                50
            );
        }

        private void RegisterStorytellerVariable()
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

        private void RegisterMemoryVariable()
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

        private void RegisterShapingVariable()
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
                    int start = System.Math.Max(0, history.Count - 5);
                    for (int i = start; i < history.Count; i++)
                    {
                        var r = history[i];
                        sb.AppendLine($"- [{r.action}] {r.label}");
                    }
                    return sb.ToString().TrimEnd();
                },
                "RimMind shaping history",
                70
            );
        }

        private void RegisterAdvisorLogVariable()
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

        private static void RegisterPromptEntry(RimTalkContextPushPlan plan)
        {
            if (string.IsNullOrEmpty(plan.PromptContent)) return;

            RimTalkApiShim.AddPromptEntry(
                name: "RimMind Context",
                content: plan.PromptContent,
                roleValue: 0,
                positionValue: 0,
                sourceModId: ModId
            );
        }

        public void Unregister()
        {
            if (!IsRegistered) return;
            RimTalkApiShim.Cleanup(ModId);
            IsRegistered = false;
        }
    }
}
