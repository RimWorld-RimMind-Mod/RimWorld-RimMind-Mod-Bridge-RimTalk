using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Features.Bridges;
using RimMind.Bridge.RimTalk.Detection;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    /// <summary>
    /// Coordinates registration of RimTalk bridge modules using Core's <see cref="ExternalModBridgeCoordinator"/>.
    /// Modules are iterated uniformly; each module decides whether to actually register
    /// based on its own settings/detector checks inside <see cref="IBridgeModule.Register"/>.
    /// Unregister iterates all modules symmetrically.
    /// </summary>
    public static class RimTalkBridgeCoordinator
    {
        private static List<IBridgeModule>? _modules;

        public static IReadOnlyList<IBridgeModule> Modules => GetModules();

        private static List<IBridgeModule> GetModules() =>
            _modules ??= new List<IBridgeModule>
            {
                new ContextPullBridge(),
                new ContextPushBridge(),
                new PersonaPushBridge()
            };

        private static ExternalModBridgeCoordinator CreateCoordinator() =>
            new ExternalModBridgeCoordinator(
                () => RimTalkDetector.IsRimTalkActive,
                GetModules(),
                gates: null,
                logInfo: message => Log.Message($"[RimMind-Bridge-RimTalk] {message}"),
                logWarn: message => Log.Warning($"[RimMind-Bridge-RimTalk] {message}"),
                inactiveMessage: "RimTalk not active, bridge modules skipped.");

        public static void Register()
        {
            CreateCoordinator().RegisterAll();
            Log.Message("[RimMind-Bridge-RimTalk] Initialized.");
        }

        public static void Unregister() => CreateCoordinator().UnregisterAll();
    }
}
