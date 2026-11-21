using System;
using System.Reflection;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Testing;
using Verse;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests.Contracts
{
    [Collection("RimTalk")]
    public sealed class RimTalkSettingsCompatibilityContracts
    {
        [Fact]
        public void Settings_preserve_safe_defaults_and_change_tracking()
        {
            ContractCaseRunner.Run(
                ("defaults enable coordination but keep optional memory data private", () =>
                {
                    BridgeRimTalkSettings.ResetForTesting();
                    var settings = BridgeRimTalkSettings.Get();

                    Assert.True(settings.enableDialogueGate);
                    Assert.True(settings.enableContextPush);
                    Assert.True(settings.enableContextPull);
                    Assert.False(settings.pushMemory);
                    Assert.False(settings.pushShaping);
                }),
                ("ApplyDefaults restores all safe default flags", () =>
                {
                    BridgeRimTalkSettings.ResetForTesting();
                    var settings = BridgeRimTalkSettings.Get();
                    settings.enableDialogueGate = false;
                    settings.forceRimMindPlayerDialogue = true;
                    settings.enableContextPush = false;
                    settings.pushMemory = true;
                    settings.pushShaping = true;
                    settings.injectPersonaToTraits = true;
                    settings.injectPersonaToMood = true;
                    settings.enableContextPull = false;
                    settings.pullRimTalkHistory = false;

                    settings.ApplyDefaults();

                    Assert.True(settings.enableDialogueGate);
                    Assert.False(settings.forceRimMindPlayerDialogue);
                    Assert.True(settings.enableContextPush);
                    Assert.False(settings.pushMemory);
                    Assert.False(settings.pushShaping);
                    Assert.False(settings.injectPersonaToTraits);
                    Assert.False(settings.injectPersonaToMood);
                    Assert.True(settings.enableContextPull);
                    Assert.True(settings.pullRimTalkHistory);
                }),
                ("context push plan exposes only enabled variables", () =>
                {
                    BridgeRimTalkSettings.ResetForTesting();
                    BridgeRimTalkSettings settings = BridgeRimTalkSettings.Get();
                    settings.pushMemory = true;
                    settings.pushAdvisorLog = false;

                    RimTalkContextPushPlan plan = RimTalkContextPushPlan.Build(settings);

                    Assert.True(plan.RegisterPersonality);
                    Assert.True(plan.RegisterMemory);
                    Assert.False(plan.RegisterAdvisorLog);
                    Assert.Contains("rimmind_memory", plan.PromptContent);
                    Assert.DoesNotContain("rimmind_advisor_log", plan.PromptContent);
                }));
        }

        [Fact]
        public void Detector_cache_preserves_dependency_availability_boundaries()
        {
            ContractCaseRunner.Run(
                ("explicit inactive state is observable", () =>
                {
                    RimTalkDetector.IsRimTalkActive = false;
                    Assert.False(RimTalkDetector.IsRimTalkActive);
                }),
                ("cache invalidation discards active and API state", () =>
                {
                    RimTalkDetector.IsRimTalkActive = true;
                    RimTalkDetector.IsRimTalkApiAvailable = true;

                    RimTalkDetector.InvalidateCache();

                    Assert.False(RimTalkDetector.IsRimTalkActive);
                    Assert.False(RimTalkDetector.IsRimTalkApiAvailable);
                }),
                ("API shim is unavailable without the reflected dependency", () =>
                {
                    RimTalkDetector.IsRimTalkApiAvailable = false;
                    Assert.False(RimTalkApiShim.IsAvailable);
                    Assert.False(RimTalkApiShim.RegisterPawnVariable(
                        "owner",
                        "value",
                        _ => "context"));
                }));
        }

        [Fact]
        public void API_shim_preserves_exact_signature_and_failure_contracts()
        {
            ContractCaseRunner.Run(
                ("exact prompt signature continues to AddPromptEntry and caches reflection MethodInfo", () =>
                {
                    PrepareShim(typeof(CorrectPromptApi));

                    Assert.True(RimTalkApiShim.AddPromptEntry("name", "content"));
                    Assert.True(RimTalkApiShim.AddPromptEntry("name2", "content2"));

                    var cachedAdd1 = RimTalkApiShim.Accessor.GetCachedMethod(
                        typeof(CorrectPromptApi),
                        "AddPromptEntry",
                        BindingFlags.Public | BindingFlags.Static);
                    var cachedAdd2 = RimTalkApiShim.Accessor.GetCachedMethod(
                        typeof(CorrectPromptApi),
                        "AddPromptEntry",
                        BindingFlags.Public | BindingFlags.Static);
                    Assert.NotNull(cachedAdd1);
                    Assert.Same(cachedAdd1, cachedAdd2);
                }),
                ("near-match prompt signatures fail closed and warn once", () =>
                {
                    Log.Reset();
                    PrepareShim(typeof(WrongPromptApi));

                    Assert.False(RimTalkApiShim.AddPromptEntry("name", "content"));
                    Assert.False(RimTalkApiShim.AddPromptEntry("name", "content"));
                    Assert.Single(Log.Warnings);
                }),
                ("missing environment registration fails closed", () =>
                {
                    PrepareShim(typeof(MissingApi));

                    Assert.False(RimTalkApiShim.RegisterEnvironmentVariable(
                        "owner",
                        "value",
                        _ => "context"));
                }),
                ("cleanup remains safe when the dependency omits methods", () =>
                {
                    PrepareShim(typeof(MissingApi));

                    var exception = Record.Exception(() =>
                        RimTalkApiShim.Cleanup("owner"));

                    Assert.Null(exception);
                }));
        }

        private static void PrepareShim(Type apiType)
        {
            RimTalkDetector.IsRimTalkApiAvailable = true;
            RimTalkApiShim.ConfigureTypesForTesting(
                apiType,
                promptEntryType: typeof(object));
        }

        private static class CorrectPromptApi
        {
            public static object CreatePromptEntry(
                string name,
                string content,
                int role,
                int position,
                int depth,
                string? sourceModId) => new object();

            public static bool AddPromptEntry(object entry) => true;
        }

        private static class WrongPromptApi
        {
            public static object CreatePromptEntry(string name) => new object();
        }

        private static class MissingApi
        {
        }
    }
}
