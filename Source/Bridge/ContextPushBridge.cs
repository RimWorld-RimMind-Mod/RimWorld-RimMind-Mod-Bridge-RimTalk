using System.Text;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
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
                    var description = RimMindProviderReader.GetPawn(
                        RimMindProviderReader.PersonalityDescription,
                        pawn);
                    var workTendencies = RimMindProviderReader.GetPawn(
                        RimMindProviderReader.PersonalityWorkTendencies,
                        pawn);
                    var socialTendencies = RimMindProviderReader.GetPawn(
                        RimMindProviderReader.PersonalitySocialTendencies,
                        pawn);
                    var narrative = RimMindProviderReader.GetPawn(
                        RimMindProviderReader.PersonalityNarrative,
                        pawn);

                    var sb = new StringBuilder();
                    sb.AppendLine(PersonaFormatter.BuildFullProfile(
                        description,
                        workTendencies,
                        socialTendencies));
                    if (!string.IsNullOrEmpty(narrative))
                        sb.AppendLine($"[AI] {narrative}");
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
                map => RimMindProviderReader.GetStatic(
                    RimMindProviderReader.NarratorMemoryBrief),
                "RimMind storyteller state",
                80
            );
        }

        private void RegisterMemoryVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_memory",
                pawn => RimMindProviderReader.GetPawn(
                    RimMindProviderReader.PawnMemoryBrief,
                    pawn),
                "RimMind memory data",
                60
            );
        }

        private void RegisterShapingVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_shaping",
                pawn => RimMindProviderReader.GetPawn(
                    RimMindProviderReader.PersonalityShaping,
                    pawn),
                "RimMind shaping history",
                70
            );
        }

        private void RegisterAdvisorLogVariable()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_advisor_log",
                pawn => RimMindProviderReader.GetPawn(
                    RimMindProviderReader.AdvisorHistoryBrief,
                    pawn),
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
