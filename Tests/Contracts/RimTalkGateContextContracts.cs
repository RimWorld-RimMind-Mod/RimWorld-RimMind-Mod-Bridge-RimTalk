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
                    Assert.Contains("NarratorMemoryBrief", source);
                    Assert.Contains("PawnMemoryBrief", source);
                    Assert.Contains("PersonalityShaping", source);
                    Assert.Contains("AdvisorHistoryBrief", source);
                }));
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
