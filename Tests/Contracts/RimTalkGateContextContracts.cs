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
        public void Persona_context_preserves_public_sections()
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
                    RimMindAPI.Providers.PawnResult =
                        Result<string?, RimMindError>.Ok("value");
                    RimMindAPI.Providers.StaticResult =
                        Result<string?, RimMindError>.Ok("world");

                    Assert.Equal("value", RimMindProviderReader.GetPawn("test.pawn", new Pawn()));
                    Assert.Equal("world", RimMindProviderReader.GetStatic("test.static"));
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
                    RimMindAPI.Providers.PawnResult =
                        Result<string?, RimMindError>.Err(RimMindErrors.Internal("provider failed"));

                    Assert.Equal(string.Empty,
                        RimMindProviderReader.GetPawn("failing.category", new Pawn()));
                    Assert.Equal(string.Empty,
                        RimMindProviderReader.GetPawn("failing.category", new Pawn()));

                    Assert.Single(Log.Warnings);
                    Assert.Contains("Provider 'failing.category' failed", Log.Warnings[0]);
                }),
                ("production source has no child-mod compile dependency", () =>
                {
                    var root = RepositoryRoot();
                    var contextPush = NormalizeSource(File.ReadAllText(Path.Combine(
                        root,
                        "Source",
                        "Bridge",
                        "ContextPushBridge.cs")));
                    var personaPush = NormalizeSource(File.ReadAllText(Path.Combine(
                        root,
                        "Source",
                        "Bridge",
                        "PersonaPushBridge.cs")));
                    var providerReader = NormalizeSource(File.ReadAllText(Path.Combine(
                        root,
                        "Source",
                        "Bridge",
                        "RimMindProviderReader.cs")));
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

                    AssertProviderCategoryContracts(providerReader);
                    AssertContextPushWiring(contextPush);
                    AssertPersonaPushWiring(personaPush);
                }));
        }

        private static void AssertProviderCategoryContracts(string source)
        {
            var constants = SourceSection(
                source,
                "internal static class RimMindProviderReader",
                "internal static string GetPawn(");
            AssertContainsInOrder(
                constants,
                "internal const string PersonalityDescription = \"personality.description\";",
                "internal const string PersonalityWorkTendencies = \"personality.work_tendencies\";",
                "internal const string PersonalitySocialTendencies = \"personality.social_tendencies\";",
                "internal const string PersonalityNarrative = \"personality.ai_narrative\";",
                "internal const string PersonalityShaping = \"personality.shaping_history\";",
                "internal const string PawnMemoryBrief = \"memory.pawn_brief\";",
                "internal const string NarratorMemoryBrief = \"memory.narrator_brief\";",
                "internal const string AdvisorHistoryBrief = \"advisor.history_brief\";");
        }

        private static void AssertContextPushWiring(string source)
        {
            var register = SourceSection(
                source,
                "public void Register()",
                "private void RegisterPersonalityVariable()");
            AssertContainsInOrder(
                register,
                "if (settings.enableContextPush)",
                "if (plan.RegisterPersonality)",
                "RegisterPersonalityVariable();",
                "if (plan.RegisterStoryteller)",
                "RegisterStorytellerVariable();",
                "if (plan.RegisterMemory)",
                "RegisterMemoryVariable();",
                "if (plan.RegisterShaping)",
                "RegisterShapingVariable();",
                "if (plan.RegisterAdvisorLog)",
                "RegisterAdvisorLogVariable();");

            var personality = RegistrationBlock(
                SourceSection(
                    source,
                    "private void RegisterPersonalityVariable()",
                    "private void RegisterStorytellerVariable()"),
                "RegisterPawnVariable",
                "rimmind_personality");
            AssertContainsInOrder(
                personality,
                "\"rimmind_personality\"",
                "var description = RimMindProviderReader.GetPawn(",
                "RimMindProviderReader.PersonalityDescription",
                "pawn);",
                "var workTendencies = RimMindProviderReader.GetPawn(",
                "RimMindProviderReader.PersonalityWorkTendencies",
                "pawn);",
                "var socialTendencies = RimMindProviderReader.GetPawn(",
                "RimMindProviderReader.PersonalitySocialTendencies",
                "pawn);",
                "var narrative = RimMindProviderReader.GetPawn(",
                "RimMindProviderReader.PersonalityNarrative",
                "pawn);",
                "PersonaFormatter.BuildFullProfile(",
                "description,",
                "workTendencies,",
                "socialTendencies));",
                "sb.AppendLine($\"[AI] {narrative}\");",
                "\"RimMind personality profile\"",
                "\n                50\n");

            var storyteller = RegistrationBlock(
                SourceSection(
                    source,
                    "private void RegisterStorytellerVariable()",
                    "private void RegisterMemoryVariable()"),
                "RegisterEnvironmentVariable",
                "rimmind_storyteller");
            AssertContainsInOrder(
                storyteller,
                "\"rimmind_storyteller\"",
                "map => RimMindProviderReader.GetStatic(",
                "RimMindProviderReader.NarratorMemoryBrief",
                "\"RimMind storyteller state\"",
                "\n                80\n");

            AssertDirectPawnVariable(
                SourceSection(
                    source,
                    "private void RegisterMemoryVariable()",
                    "private void RegisterShapingVariable()"),
                "rimmind_memory",
                "PawnMemoryBrief",
                "RimMind memory data",
                60);
            AssertDirectPawnVariable(
                SourceSection(
                    source,
                    "private void RegisterShapingVariable()",
                    "private void RegisterAdvisorLogVariable()"),
                "rimmind_shaping",
                "PersonalityShaping",
                "RimMind shaping history",
                70);
            AssertDirectPawnVariable(
                SourceSection(
                    source,
                    "private void RegisterAdvisorLogVariable()",
                    "private static void RegisterPromptEntry"),
                "rimmind_advisor_log",
                "AdvisorHistoryBrief",
                "RimMind advisor history",
                80);
        }

        private static void AssertPersonaPushWiring(string source)
        {
            var register = SourceSection(
                source,
                "public void Register()",
                "private void RegisterPersonaVariables()");
            AssertContainsInOrder(
                register,
                "if (!settings.enableContextPush || !settings.pushPersonality) return;",
                "RegisterPersonaVariables();",
                "if (settings.injectPersonaToTraits || settings.injectPersonaToMood)",
                "RegisterPersonaHooks();");

            var variables = SourceSection(
                source,
                "private void RegisterPersonaVariables()",
                "private void RegisterPersonaHooks()");
            AssertDirectPawnVariable(
                variables,
                "rimmind_persona_desc",
                "PersonalityDescription",
                "RimMind personality description",
                40);
            AssertDirectPawnVariable(
                variables,
                "rimmind_persona_work",
                "PersonalityWorkTendencies",
                "RimMind work tendencies",
                45);
            AssertDirectPawnVariable(
                variables,
                "rimmind_persona_social",
                "PersonalitySocialTendencies",
                "RimMind social tendencies",
                45);
            AssertDirectPawnVariable(
                variables,
                "rimmind_persona_narrative",
                "PersonalityNarrative",
                "RimMind AI narrative",
                55);

            var hooks = SourceSection(
                source,
                "private void RegisterPersonaHooks()",
                "public void Unregister()");
            var traits = SourceSection(
                hooks,
                "if (settings.injectPersonaToTraits)",
                "if (settings.injectPersonaToMood)");
            AssertContainsInOrder(
                traits,
                "if (settings.injectPersonaToTraits)",
                "RimTalkApiShim.RegisterPawnHook(",
                "\"Traits\"",
                "\n                    0,\n",
                "PersonaFormatter.BuildFullProfile(",
                "RimMindProviderReader.PersonalityDescription",
                "RimMindProviderReader.PersonalityWorkTendencies",
                "RimMindProviderReader.PersonalitySocialTendencies",
                "return existing + \"\\n\" + formatted;",
                "\n                    90\n");

            var mood = SourceSection(
                hooks,
                "if (settings.injectPersonaToMood)",
                null);
            AssertContainsInOrder(
                mood,
                "if (settings.injectPersonaToMood)",
                "RimTalkApiShim.RegisterPawnHook(",
                "\"Mood\"",
                "\n                    0,\n",
                "RimMindProviderReader.GetPawn(",
                "RimMindProviderReader.PersonalityNarrative",
                "return existing + \"\\n[AI Narrative] \" + narrative;",
                "\n                    90\n");
        }

        private static void AssertDirectPawnVariable(
            string source,
            string variableName,
            string categoryConstant,
            string description,
            int priority)
        {
            var registration = RegistrationBlock(
                source,
                "RegisterPawnVariable",
                variableName);
            AssertContainsInOrder(
                registration,
                $"\"{variableName}\"",
                "pawn => RimMindProviderReader.GetPawn(",
                $"RimMindProviderReader.{categoryConstant}",
                "pawn),",
                $"\"{description}\"",
                $"\n                {priority}\n");
        }

        private static string RegistrationBlock(
            string source,
            string registrationMethod,
            string variableName)
        {
            var methodMarker = $"RimTalkApiShim.{registrationMethod}(";
            var nameMarker = $"\"{variableName}\"";
            var nameIndex = source.IndexOf(nameMarker, StringComparison.Ordinal);
            Assert.True(nameIndex >= 0, $"Missing variable registration: {variableName}");

            var startIndex = source.LastIndexOf(
                methodMarker,
                nameIndex,
                StringComparison.Ordinal);
            Assert.True(startIndex >= 0, $"Missing {registrationMethod} call: {variableName}");

            var nextIndex = source.IndexOf(
                methodMarker,
                nameIndex + nameMarker.Length,
                StringComparison.Ordinal);
            return nextIndex >= 0
                ? source.Substring(startIndex, nextIndex - startIndex)
                : source.Substring(startIndex);
        }

        private static string SourceSection(
            string source,
            string startMarker,
            string? endMarker)
        {
            var startIndex = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(startIndex >= 0, $"Missing source marker: {startMarker}");

            if (endMarker == null)
                return source.Substring(startIndex);

            var endIndex = source.IndexOf(
                endMarker,
                startIndex + startMarker.Length,
                StringComparison.Ordinal);
            Assert.True(endIndex >= 0, $"Missing source marker: {endMarker}");
            return source.Substring(startIndex, endIndex - startIndex);
        }

        private static void AssertContainsInOrder(string source, params string[] fragments)
        {
            var offset = 0;
            foreach (var fragment in fragments)
            {
                var index = source.IndexOf(fragment, offset, StringComparison.Ordinal);
                Assert.True(index >= 0, $"Missing ordered source fragment: {fragment}");
                offset = index + fragment.Length;
            }
        }

        private static string NormalizeSource(string source)
            => source.Replace("\r\n", "\n");

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
