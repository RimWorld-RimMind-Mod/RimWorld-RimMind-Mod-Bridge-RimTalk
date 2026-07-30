using System.Collections.Generic;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Application.Common.Interfaces.Extension;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    /// <summary>
    /// Coordinates registration of RimTalk bridge modules.
    /// Modules are iterated uniformly; each module decides whether to actually register
    /// based on its own settings/detector checks inside <see cref="IBridgeModule.Register"/>.
    /// Unregister iterates all modules but each module's Unregister is a no-op if not registered,
    /// making Register/Unregister symmetric (previously Register was conditional, Unregister unconditional).
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

        private static BridgeModuleCoordinator CreateCoordinator() =>
            new BridgeModuleCoordinator(
                GetModules(),
                () => RimTalkDetector.IsRimTalkActive,
                message => Log.Message($"[RimMind-Bridge-RimTalk] {message}"),
                message => Log.Warning($"[RimMind-Bridge-RimTalk] {message}"));

        public static void Register()
        {
            CreateCoordinator().RegisterAll();
            Log.Message("[RimMind-Bridge-RimTalk] Initialized.");
        }

        public static void Unregister() => CreateCoordinator().UnregisterAll();
    }
}
