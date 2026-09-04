using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using RimMind.Testing;
using RimTalk.API;
using RimTalk.Prompt;
using Verse;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests.Contracts
{
    [Collection("RimTalk")]
    public sealed class RimTalkGateContextContracts
    {
        [Fact]
        public void Dialogue_gate_preserves_trigger_and_override_boundaries()
        {
            ContractCaseRunner.Run(
                ("unavailable RimTalk never suppresses RimMind", () =>
                {
                    Reset(active: false);
                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "Chitchat"));
                    Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
                }),
                ("known automatic triggers follow their independent switches", () =>
                {
                    var settings = Reset(active: true);
                    settings.skipChitchat = true;
                    settings.skipAutoDialogue = false;

                    Assert.True(DialogueGate.ShouldSkipDialogue(null, "Chitchat"));
                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "Auto"));
                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "unknown"));
                }),
                ("disabled gate leaves every dialogue path available", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableDialogueGate = false;

                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "PlayerInput"));
                    Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
                }),
                ("player override keeps float-menu dialogue with RimMind", () =>
                {
                    var settings = Reset(active: true);
                    settings.skipPlayerDialogue = true;
                    settings.forceRimMindPlayerDialogue = true;

                    Assert.True(DialogueGate.ShouldSkipDialogue(null, "PlayerInput"));
                    Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
                }));
        }

        [Fact]
        public void Coordinator_preserves_registration_lifecycle()
        {
            ContractCaseRunner.Run(
                ("inactive dependency skips every bridge module", () =>
                {
                    var setup = InstallModules(active: false);
                    setup.Coordinator.RegisterAll();

                    Assert.All(setup.Modules, module => Assert.False(module.IsRegistered));
                }),
                ("active dependency registers all bridge directions", () =>
                {
                    var setup = InstallModules(active: true);
                    setup.Coordinator.RegisterAll();

                    Assert.All(setup.Modules, module => Assert.True(module.IsRegistered));
                }),
                ("registration is idempotent per module", () =>
                {
                    var setup = InstallModules(active: true);
                    setup.Coordinator.RegisterAll();
                    foreach (var module in setup.Modules)
                    {
                        module.RegisterCalled = false;
                    }

                    setup.Coordinator.RegisterAll();

                    Assert.All(setup.Modules, module => Assert.False(module.RegisterCalled));
                }),
                ("unregister is symmetric after registration", () =>
                {
                    var setup = InstallModules(active: true);
                    setup.Coordinator.RegisterAll();

                    setup.Coordinator.UnregisterAll();

                    Assert.All(setup.Modules, module =>
                    {
                        Assert.True(module.UnregisterCalled);
                        Assert.False(module.IsRegistered);
                    });
                }),
                ("one failing module does not block later modules", () =>
                {
                    var setup = InstallModules(active: true);
                    setup.Modules[1].ThrowOnRegister = true;

                    setup.Coordinator.RegisterAll();

                    Assert.True(setup.Modules[0].IsRegistered);
                    Assert.False(setup.Modules[1].IsRegistered);
                    Assert.True(setup.Modules[2].IsRegistered);
                }));
        }

        [Fact]
        public void Context_providers_and_bridge_wiring_preserve_contracts()
        {
            ContractCaseRunner.Run(
                ("null and empty profiles expose no context", () =>
                {
                    Assert.Equal(string.Empty, PersonaFormatter.BuildFullProfile(null, null, null));
                    Assert.Equal(string.Empty, PersonaFormatter.BuildFullProfile("", "", ""));
                }),
                ("description work and social sections retain their labels", () =>
                {
                    Assert.Equal(
                        $"Brave{Environment.NewLine}[Work] Diligent{Environment.NewLine}[Social] Friendly",
                        PersonaFormatter.BuildFullProfile("Brave", "Diligent", "Friendly"));
                }),
                ("formatted context has no trailing newline", () =>
                {
                    Assert.Equal(
                        "[Work] Careful",
                        PersonaFormatter.BuildFullProfile(null, "Careful", null));
                }),
                ("provider seam returns pawn and static values", () =>
                {
                    ResetProviders();
                    RimMindAPI.Providers.SetPawn("test.pawn", "value");
                    RimMindAPI.Providers.SetStatic("test.static", "world");

                    var pawn = new Pawn();
                    Assert.Equal("value", RimMindProviderReader.GetPawn("test.pawn", pawn));
                    Assert.Equal("world", RimMindProviderReader.GetStatic("test.static"));
                    Assert.Collection(
                        RimMindAPI.Providers.PawnCalls,
                        call =>
                        {
                            Assert.Equal("test.pawn", call.Category);
                            Assert.Same(pawn, call.Pawn);
                        });
                    Assert.Equal(
                        new[] { "test.static" },
                        RimMindAPI.Providers.StaticCalls);
                }),
                ("missing optional providers are silent", () =>
                {
                    ResetProviders();

                    Assert.Equal(string.Empty,
                        RimMindProviderReader.GetPawn("missing.category", new Pawn()));
                    Assert.Empty(Log.Warnings);
                }),
                ("registered provider failures warn once per category", () =>
                {
                    ResetProviders();
                    RimMindAPI.Providers.Categories.Add("failing.category");
                    RimMindAPI.Providers.SetPawnError(
                        "failing.category",
                        RimMindErrors.Internal("provider failed"));

                    Assert.Equal(string.Empty,
                        RimMindProviderReader.GetPawn("failing.category", new Pawn()));
                    Assert.Equal(string.Empty,
                        RimMindProviderReader.GetPawn("failing.category", new Pawn()));

                    Assert.Single(Log.Warnings);
                    Assert.Contains("Provider 'failing.category' failed", Log.Warnings[0]);
                }),
                ("real context bridge registers five mapped variables", () =>
                {
                    BridgeRimTalkSettings settings = PrepareBridgeHarness();
                    settings.pushMemory = true;
                    settings.pushShaping = true;
                    SeedProviderValues();

                    var bridge = new ContextPushBridge();
                    bridge.Register();

                    Assert.True(bridge.IsRegistered);
                    Assert.Equal(4, FakeRimTalkPromptAPI.PawnVariables.Count);
                    Assert.Single(FakeRimTalkPromptAPI.EnvironmentVariables);

                    var pawn = new Pawn();
                    AssertPawnVariable(
                        "rimmind_personality",
                        "RimMind.Bridge.RimTalk.Push",
                        "RimMind personality profile",
                        50,
                        $"Brave{Environment.NewLine}[Work] Diligent{Environment.NewLine}[Social] Friendly{Environment.NewLine}[AI] Reflective",
                        pawn);
                    AssertPawnVariable(
                        "rimmind_memory",
                        "RimMind.Bridge.RimTalk.Push",
                        "RimMind memory data",
                        60,
                        "Remembered a rescue",
                        pawn);
                    AssertPawnVariable(
                        "rimmind_shaping",
                        "RimMind.Bridge.RimTalk.Push",
                        "RimMind shaping history",
                        70,
                        "Became more patient",
                        pawn);
                    AssertPawnVariable(
                        "rimmind_advisor_log",
                        "RimMind.Bridge.RimTalk.Push",
                        "RimMind advisor history",
                        80,
                        "Accepted shelter advice",
                        pawn);

                    FakeRimTalkPromptAPI.EnvironmentVariableRegistration storyteller =
                        EnvironmentVariable("rimmind_storyteller");
                    Assert.Equal("RimMind.Bridge.RimTalk.Push", storyteller.ModId);
                    Assert.Equal("RimMind storyteller state", storyteller.Description);
                    Assert.Equal(80, storyteller.Priority);
                    Assert.Equal("A cold snap approaches", storyteller.Provider(new Map()));

                    Assert.Equal(
                        new[]
                        {
                            "personality.description",
                            "personality.work_tendencies",
                            "personality.social_tendencies",
                            "personality.ai_narrative",
                            "memory.pawn_brief",
                            "personality.shaping_history",
                            "advisor.history_brief"
                        },
                        RimMindAPI.Providers.PawnCalls.Select(call => call.Category));
                    Assert.All(
                        RimMindAPI.Providers.PawnCalls,
                        call => Assert.Same(pawn, call.Pawn));
                    Assert.Equal(
                        new[] { "memory.narrator_brief" },
                        RimMindAPI.Providers.StaticCalls);

                    FakePromptEntry prompt = Assert.Single(FakeRimTalkPromptAPI.PromptEntries);
                    Assert.Equal("RimMind Context", prompt.Name);
                    Assert.Equal(FakePromptRole.System, prompt.Role);
                    Assert.Equal(FakePromptPosition.BeforeHistory, prompt.Position);
                    Assert.Equal(0, prompt.InChatDepth);
                    Assert.Equal("RimMind.Bridge.RimTalk.Push", prompt.SourceModId);
                    Assert.Contains("rimmind_personality", prompt.Content);
                    Assert.Contains("rimmind_storyteller", prompt.Content);
                    Assert.Contains("rimmind_memory", prompt.Content);
                    Assert.Contains("rimmind_shaping", prompt.Content);
                    Assert.Contains("rimmind_advisor_log", prompt.Content);
                }),
                ("context bridge honors global and per-provider push gates", () =>
                {
                    BridgeRimTalkSettings settings = PrepareBridgeHarness();
                    settings.enableContextPush = false;

                    var disabledBridge = new ContextPushBridge();
                    disabledBridge.Register();

                    Assert.True(disabledBridge.IsRegistered);
                    Assert.Empty(FakeRimTalkPromptAPI.PawnVariables);
                    Assert.Empty(FakeRimTalkPromptAPI.EnvironmentVariables);
                    Assert.Empty(FakeRimTalkPromptAPI.PromptEntries);

                    settings = PrepareBridgeHarness();
                    settings.pushPersonality = false;
                    settings.pushStoryteller = false;
                    settings.pushMemory = true;
                    settings.pushAdvisorLog = false;
                    settings.pushShaping = false;

                    var selectiveBridge = new ContextPushBridge();
                    selectiveBridge.Register();

                    Assert.True(selectiveBridge.IsRegistered);
                    Assert.Equal(
                        new[] { "rimmind_memory" },
                        FakeRimTalkPromptAPI.PawnVariables.Select(variable => variable.Name));
                    Assert.Empty(FakeRimTalkPromptAPI.EnvironmentVariables);
                    FakePromptEntry prompt = Assert.Single(FakeRimTalkPromptAPI.PromptEntries);
                    Assert.Contains("rimmind_memory", prompt.Content);
                    Assert.DoesNotContain("rimmind_personality", prompt.Content);
                }),
                ("real persona bridge registers four mapped variables and both hooks", () =>
                {
                    BridgeRimTalkSettings settings = PrepareBridgeHarness();
                    settings.injectPersonaToTraits = true;
                    settings.injectPersonaToMood = true;
                    SeedProviderValues();

                    var bridge = new PersonaPushBridge();
                    bridge.Register();

                    Assert.True(bridge.IsRegistered);
                    Assert.Equal(4, FakeRimTalkPromptAPI.PawnVariables.Count);
                    Assert.Equal(2, FakeRimTalkPromptAPI.PawnHooks.Count);

                    var pawn = new Pawn();
                    AssertPawnVariable(
                        "rimmind_persona_desc",
                        "RimMind.Bridge.RimTalk.Persona",
                        "RimMind personality description",
                        40,
                        "Brave",
                        pawn);
                    AssertPawnVariable(
                        "rimmind_persona_work",
                        "RimMind.Bridge.RimTalk.Persona",
                        "RimMind work tendencies",
                        45,
                        "Diligent",
                        pawn);
                    AssertPawnVariable(
                        "rimmind_persona_social",
                        "RimMind.Bridge.RimTalk.Persona",
                        "RimMind social tendencies",
                        45,
                        "Friendly",
                        pawn);
                    AssertPawnVariable(
                        "rimmind_persona_narrative",
                        "RimMind.Bridge.RimTalk.Persona",
                        "RimMind AI narrative",
                        55,
                        "Reflective",
                        pawn);
                    Assert.Equal(
                        new[]
                        {
                            "personality.description",
                            "personality.work_tendencies",
                            "personality.social_tendencies",
                            "personality.ai_narrative"
                        },
                        RimMindAPI.Providers.PawnCalls.Select(call => call.Category));

                    RimMindAPI.Providers.ClearCalls();
                    FakeRimTalkPromptAPI.PawnHookRegistration traits = PawnHook("Traits");
                    FakeRimTalkPromptAPI.PawnHookRegistration mood = PawnHook("Mood");
                    AssertHookMetadata(traits);
                    AssertHookMetadata(mood);
                    Assert.Equal(
                        $"Existing traits\nBrave{Environment.NewLine}[Work] Diligent{Environment.NewLine}[Social] Friendly",
                        traits.Handler(pawn, "Existing traits"));
                    Assert.Equal(
                        "Existing mood\n[AI Narrative] Reflective",
                        mood.Handler(pawn, "Existing mood"));
                    Assert.Equal(
                        new[]
                        {
                            "personality.description",
                            "personality.work_tendencies",
                            "personality.social_tendencies",
                            "personality.ai_narrative"
                        },
                        RimMindAPI.Providers.PawnCalls.Select(call => call.Category));

                    RimMindAPI.Providers.Reset();
                    RimMindAPI.Providers.SetPawn("personality.description", string.Empty);
                    RimMindAPI.Providers.SetPawn("personality.work_tendencies", string.Empty);
                    RimMindAPI.Providers.SetPawn("personality.social_tendencies", string.Empty);
                    RimMindAPI.Providers.SetPawn("personality.ai_narrative", string.Empty);
                    Assert.Equal("Existing traits", traits.Handler(pawn, "Existing traits"));
                    Assert.Equal("Existing mood", mood.Handler(pawn, "Existing mood"));
                }),
                ("persona bridge honors enable push and hook gates", () =>
                {
                    BridgeRimTalkSettings settings = PrepareBridgeHarness();
                    settings.enableContextPush = false;
                    var disabledBridge = new PersonaPushBridge();
                    disabledBridge.Register();
                    Assert.False(disabledBridge.IsRegistered);
                    Assert.Empty(FakeRimTalkPromptAPI.PawnVariables);

                    settings = PrepareBridgeHarness();
                    settings.pushPersonality = false;
                    var excludedBridge = new PersonaPushBridge();
                    excludedBridge.Register();
                    Assert.False(excludedBridge.IsRegistered);
                    Assert.Empty(FakeRimTalkPromptAPI.PawnVariables);

                    settings = PrepareBridgeHarness();
                    var variablesOnlyBridge = new PersonaPushBridge();
                    variablesOnlyBridge.Register();
                    Assert.True(variablesOnlyBridge.IsRegistered);
                    Assert.Equal(4, FakeRimTalkPromptAPI.PawnVariables.Count);
                    Assert.Empty(FakeRimTalkPromptAPI.PawnHooks);

                    settings = PrepareBridgeHarness();
                    settings.injectPersonaToTraits = true;
                    new PersonaPushBridge().Register();
                    Assert.Equal(
                        new[] { "Traits" },
                        FakeRimTalkPromptAPI.PawnHooks.Select(hook => hook.Category));

                    settings = PrepareBridgeHarness();
                    settings.injectPersonaToMood = true;
                    new PersonaPushBridge().Register();
                    Assert.Equal(
                        new[] { "Mood" },
                        FakeRimTalkPromptAPI.PawnHooks.Select(hook => hook.Category));
                }),
                ("production source has no child-mod compile dependency", () =>
                {
                    var root = RepositoryRoot();
                    var source = string.Join(
                        Environment.NewLine,
                        Directory.EnumerateFiles(
                                Path.Combine(root, "Source"),
                                "*.cs",
                                SearchOption.AllDirectories)
                            .Select(File.ReadAllText));
                    var project = File.ReadAllText(
                        Path.Combine(root, "Source", "RimMindBridgeRimTalk.csproj"));

                    Assert.DoesNotContain("RimMind.Advisor", source);
                    Assert.DoesNotContain("RimMind.Memory", source);
                    Assert.DoesNotContain("RimMind.Personality", source);
                    Assert.DoesNotContain("RimMindAdvisor", project);
                    Assert.DoesNotContain("RimMindMemory", project);
                    Assert.DoesNotContain("RimMindPersonality", project);
                }));
        }

        private static BridgeRimTalkSettings PrepareBridgeHarness()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = true;
            BridgeRimTalkSettings.ResetForTesting();
            RimMindAPI.Providers.Reset();
            Log.Reset();
            FakeRimTalkPromptAPI.Reset();
            RimTalkApiShim.ConfigureTypesForTesting(
                typeof(FakeRimTalkPromptAPI),
                typeof(FakeContextHookRegistry),
                typeof(FakeContextCategories),
                typeof(FakePromptEntry),
                typeof(FakePromptRole),
                typeof(FakePromptPosition));
            return BridgeRimTalkSettings.Get();
        }

        private static void SeedProviderValues()
        {
            RimMindAPI.Providers.SetPawn("personality.description", "Brave");
            RimMindAPI.Providers.SetPawn("personality.work_tendencies", "Diligent");
            RimMindAPI.Providers.SetPawn("personality.social_tendencies", "Friendly");
            RimMindAPI.Providers.SetPawn("personality.ai_narrative", "Reflective");
            RimMindAPI.Providers.SetPawn("personality.shaping_history", "Became more patient");
            RimMindAPI.Providers.SetPawn("memory.pawn_brief", "Remembered a rescue");
            RimMindAPI.Providers.SetStatic("memory.narrator_brief", "A cold snap approaches");
            RimMindAPI.Providers.SetPawn("advisor.history_brief", "Accepted shelter advice");
        }

        private static void AssertPawnVariable(
            string name,
            string modId,
            string description,
            int priority,
            string expectedOutput,
            Pawn pawn)
        {
            FakeRimTalkPromptAPI.PawnVariableRegistration variable =
                FakeRimTalkPromptAPI.PawnVariables.Single(item => item.Name == name);
            Assert.Equal(modId, variable.ModId);
            Assert.Equal(description, variable.Description);
            Assert.Equal(priority, variable.Priority);
            Assert.Equal(expectedOutput, variable.Provider(pawn));
        }

        private static FakeRimTalkPromptAPI.EnvironmentVariableRegistration EnvironmentVariable(
            string name)
        {
            return FakeRimTalkPromptAPI.EnvironmentVariables.Single(
                item => item.Name == name);
        }

        private static FakeRimTalkPromptAPI.PawnHookRegistration PawnHook(string category)
        {
            return FakeRimTalkPromptAPI.PawnHooks.Single(
                item => item.Category == category);
        }

        private static void AssertHookMetadata(
            FakeRimTalkPromptAPI.PawnHookRegistration hook)
        {
            Assert.Equal("RimMind.Bridge.RimTalk.Persona", hook.ModId);
            Assert.Equal(FakeContextHookRegistry.HookOperation.InsertBefore, hook.Operation);
            Assert.Equal(90, hook.Priority);
        }

        private static void ResetProviders()
        {
            RimMindAPI.Providers.Reset();
            Log.Reset();
        }

        private static string RepositoryRoot()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(
                    directory.FullName,
                    "Source",
                    "RimMindBridgeRimTalk.csproj")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("RimMind-Bridge-RimTalk repository root not found.");
        }

        private static BridgeRimTalkSettings Reset(bool active)
        {
            RimTalkDetector.IsRimTalkActive = active;
            RimTalkDetector.IsRimTalkApiAvailable = false;
            BridgeRimTalkSettings.ResetForTesting();
            return BridgeRimTalkSettings.Get();
        }

        private static (
            BridgeModuleCoordinator Coordinator,
            List<BridgeModuleProbe> Modules) InstallModules(bool active)
        {
            Reset(active);
            var modules = new List<BridgeModuleProbe>
            {
                new BridgeModuleProbe("pull"),
                new BridgeModuleProbe("push"),
                new BridgeModuleProbe("persona")
            };
            var coordinator = new BridgeModuleCoordinator(
                new List<IBridgeModule>(modules),
                () => active,
                _ => { },
                _ => { });
            return (coordinator, modules);
        }

        private sealed class BridgeModuleProbe : IBridgeModule
        {
            public BridgeModuleProbe(string id)
            {
                Id = id;
            }

            public string Id { get; }
            public string OwnerModId => "RimMindBridgeRimTalk";
            public bool IsRegistered { get; private set; }
            public bool RegisterCalled { get; set; }
            public bool UnregisterCalled { get; private set; }
            public bool ThrowOnRegister { get; set; }

            public void Register()
            {
                if (IsRegistered)
                {
                    return;
                }

                if (ThrowOnRegister)
                    throw new System.InvalidOperationException("module failed");

                RegisterCalled = true;
                IsRegistered = true;
            }

            public void Unregister()
            {
                if (!IsRegistered)
                {
                    return;
                }

                UnregisterCalled = true;
                IsRegistered = false;
            }
        }
    }
}
