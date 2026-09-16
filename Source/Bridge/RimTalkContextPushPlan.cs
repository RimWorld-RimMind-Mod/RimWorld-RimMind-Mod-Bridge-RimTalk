using System.Text;
using RimMind.Bridge.RimTalk.Settings;

namespace RimMind.Bridge.RimTalk.Bridge
{
    internal sealed class RimTalkContextPushPlan
    {
        private RimTalkContextPushPlan()
        {
        }

        public bool RegisterPersonality { get; private set; }
        public bool RegisterStoryteller { get; private set; }
        public bool RegisterMemory { get; private set; }
        public bool RegisterAdvisorLog { get; private set; }
        public bool RegisterShaping { get; private set; }
        public string PromptContent { get; private set; } = string.Empty;

        public static RimTalkContextPushPlan Build(BridgeRimTalkSettings settings)
        {
            var plan = new RimTalkContextPushPlan();
            if (settings == null || !settings.enableContextPush)
                return plan;

            plan.RegisterPersonality = settings.pushPersonality;
            plan.RegisterStoryteller = settings.pushStoryteller;
            plan.RegisterMemory = settings.pushMemory;
            plan.RegisterAdvisorLog = settings.pushAdvisorLog;
            plan.RegisterShaping = settings.pushShaping;

            var prompt = new StringBuilder("# RimMind Context");
            if (plan.RegisterPersonality)
            {
                prompt.AppendLine();
                prompt.AppendLine("{{ for p in pawns }}");
                prompt.AppendLine("## {{ p.name }}'s Personality:");
                prompt.AppendLine("{{ p.rimmind_personality }}");
                prompt.Append("{{ end }}");
            }
            if (plan.RegisterStoryteller)
            {
                prompt.AppendLine();
                prompt.AppendLine("# Storyteller State");
                prompt.Append("{{rimmind_storyteller}}");
            }
            if (plan.RegisterMemory)
            {
                prompt.AppendLine();
                prompt.AppendLine("{{ for p in pawns }}");
                prompt.AppendLine("## {{ p.name }}'s Memory:");
                prompt.AppendLine("{{ p.rimmind_memory }}");
                prompt.Append("{{ end }}");
            }
            if (plan.RegisterAdvisorLog)
            {
                prompt.AppendLine();
                prompt.AppendLine("{{ for p in pawns }}");
                prompt.AppendLine("## {{ p.name }}'s Advisor Log:");
                prompt.AppendLine("{{ p.rimmind_advisor_log }}");
                prompt.Append("{{ end }}");
            }
            if (plan.RegisterShaping)
            {
                prompt.AppendLine();
                prompt.AppendLine("{{ for p in pawns }}");
                prompt.AppendLine("## {{ p.name }}'s Shaping History:");
                prompt.AppendLine("{{ p.rimmind_shaping }}");
                prompt.Append("{{ end }}");
            }

            if (plan.RegisterPersonality
                || plan.RegisterStoryteller
                || plan.RegisterMemory
                || plan.RegisterAdvisorLog
                || plan.RegisterShaping)
            {
                plan.PromptContent = prompt.ToString();
            }

            return plan;
        }
    }
}
