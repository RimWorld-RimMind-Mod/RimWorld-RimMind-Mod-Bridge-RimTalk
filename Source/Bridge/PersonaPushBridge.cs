using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Personality.Data;
using RimMind.Application.Common.Interfaces.Extension;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public sealed class PersonaPushBridge : IBridgeModule
    {
        private const string ModId = "RimMind.Bridge.RimTalk.Persona";

        public string Id => "persona_push";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public bool IsRegistered { get; private set; }

        public void Register()
        {
            if (IsRegistered) return;
            if (!RimTalkDetector.IsRimTalkApiAvailable) return;

            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enableContextPush || !settings.pushPersonality) return;

            RegisterPersonaVariables();

            if (settings.injectPersonaToTraits || settings.injectPersonaToMood)
                RegisterPersonaHooks();

            IsRegistered = true;
        }

        private void RegisterPersonaVariables()
        {
            RimTalkApiShim.RegisterPawnVariable(
                ModId,
                "rimmind_persona_desc",
                pawn =>
                {
                    var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                    if (profile == null) return "";
                    return profile.description ?? "";
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
                    return profile.workTendencies ?? "";
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
                    return profile.socialTendencies ?? "";
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
                    return profile.aiNarrative ?? "";
                },
                "RimMind AI narrative",
                55
            );
        }

        private void RegisterPersonaHooks()
        {
            var settings = BridgeRimTalkSettings.Get();

            if (settings.injectPersonaToTraits)
            {
                RimTalkApiShim.RegisterPawnHook(
                    ModId,
                    "Traits",
                    0,
                    (pawn, existing) =>
                    {
                        var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                        if (profile == null || profile.IsEmpty) return existing;

                        var formatted = PersonaFormatter.BuildFullProfile(profile);
                        if (string.IsNullOrEmpty(formatted)) return existing;
                        return existing + "\n" + formatted;
                    },
                    90
                );
            }

            if (settings.injectPersonaToMood)
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

        public void Unregister()
        {
            if (!IsRegistered) return;
            RimTalkApiShim.Cleanup(ModId);
            IsRegistered = false;
        }
    }
}
