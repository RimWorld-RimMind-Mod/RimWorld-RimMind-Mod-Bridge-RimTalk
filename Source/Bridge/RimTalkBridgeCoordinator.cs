using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Features.Bridges;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    /// <summary>
    /// Coordinates registration of RimTalk bridge gates and modules via Core's
    /// <see cref="RimMindAPI.Bridges.CreateCoordinator"/>.
    /// Modules and gates are iterated uniformly with symmetric Register/Unregister.
    /// </summary>
    public static class RimTalkBridgeCoordinator
    {
        private static List<IBridgeModule>? _modules;
        private static List<ISkipCheck>? _gates;
        private static ExternalModBridgeCoordinator? _coordinator;

        public static IReadOnlyList<IBridgeModule> Modules => GetModules();
        public static IReadOnlyList<ISkipCheck> Gates => GetGates();

        private static List<IBridgeModule> GetModules() =>
            _modules ??= new List<IBridgeModule>
            {
                new ContextPullBridge(),
                new ContextPushBridge(),
                new PersonaPushBridge()
            };

        private static List<ISkipCheck> GetGates() =>
            _gates ??= new List<ISkipCheck>
            {
                new RimTalkDialogueSkipCheck(),
                new RimTalkFloatMenuSkipCheck()
            };

        private static ExternalModBridgeCoordinator GetCoordinator() =>
            _coordinator ??= RimMindAPI.Bridges.CreateCoordinator(
                () => RimTalkDetector.IsRimTalkActive,
                GetModules(),
                gates: GetGates(),
                logInfo: message => Log.Message($"[RimMind-Bridge-RimTalk] {message}"),
                logWarn: message => Log.Warning($"[RimMind-Bridge-RimTalk] {message}"),
                inactiveMessage: "RimTalk not active, bridge modules skipped.");

        public static void Register()
        {
            GetCoordinator().RegisterAll();
            Log.Message("[RimMind-Bridge-RimTalk] Initialized.");
        }

        public static void Unregister() => GetCoordinator().UnregisterAll();

        public static void Refresh() => GetCoordinator().Refresh();

        internal static void ResetForTesting()
        {
            _coordinator?.UnregisterAll();
            _coordinator = null;
            _modules = null;
            _gates = null;
        }
    }
}
