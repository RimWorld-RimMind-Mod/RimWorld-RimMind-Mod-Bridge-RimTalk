using System.Text;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Personality.Data;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class PersonaPushBridge
    {
        private const string ModId = "RimMind.Bridge.RimTalk";

        public static void Register()
        {
            if (!RimTalkDetector.IsRimTalkApiAvailable) return;

            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enablePersonaPush) return;

            RegisterPersonaVariables();
            RegisterPersonaHooks();
        }

        private static void RegisterPersonaVariables()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_persona_desc",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null) return "";
                    return string.IsNullOrEmpty(profile.description) ? "" : profile.description;
                },
                "RimMind personality description",
                40
            );

            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_persona_work",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null) return "";
                    return string.IsNullOrEmpty(profile.workTendencies) ? "" : profile.workTendencies;
                },
                "RimMind work tendencies",
                45
            );

            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_persona_social",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null) return "";
                    return string.IsNullOrEmpty(profile.socialTendencies) ? "" : profile.socialTendencies;
                },
                "RimMind social tendencies",
                45
            );

            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_persona_narrative",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null) return "";
                    return string.IsNullOrEmpty(profile.aiNarrative) ? "" : profile.aiNarrative;
                },
                "RimMind AI narrative",
                55
            );
        }

        private static void RegisterPersonaHooks()
        {
            var settings = BridgeRimTalkSettings.Get();

            if (settings.pushPersonaToTraits)
            {
                RimTalkApiShim.RegisterPawnHook(
                    ModId,
                    "Traits",
                    0,
                    (pawn, existing) =>
                    {
                        var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                        if (profile == null || profile.IsEmpty) return existing;

                        var sb = new StringBuilder();
                        if (!string.IsNullOrEmpty(profile.description))
                            sb.AppendLine(profile.description);
                        if (!string.IsNullOrEmpty(profile.workTendencies))
                            sb.AppendLine($"[Work] {profile.workTendencies}");
                        if (!string.IsNullOrEmpty(profile.socialTendencies))
                            sb.AppendLine($"[Social] {profile.socialTendencies}");

                        if (sb.Length == 0) return existing;
                        return existing + "\n" + sb.ToString().TrimEnd();
                    },
                    90
                );
            }

            if (settings.pushPersonaToMood)
            {
                RimTalkApiShim.RegisterPawnHook(
                    ModId,
                    "Mood",
                    0,
                    (pawn, existing) =>
                    {
                        var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                        if (profile == null || string.IsNullOrEmpty(profile.aiNarrative))
                            return existing;

                        return existing + "\n[AI Narrative] " + profile.aiNarrative;
                    },
                    90
                );
            }
        }

        public static void Unregister()
        {
            RimTalkApiShim.Cleanup(ModId);
        }
    }
}
