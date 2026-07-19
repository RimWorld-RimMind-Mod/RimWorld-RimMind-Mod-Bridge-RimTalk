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

        /// <summary>Replaces the module list for testing. Pass null to reset to default list.</summary>
        public static void SetModulesForTesting(List<IBridgeModule>? modules) => _modules = modules;

        public static void Register()
        {
            if (!RimTalkDetector.IsRimTalkActive)
            {
                Log.Message("[RimMind-Bridge-RimTalk] RimTalk not active, bridge modules skipped.");
                return;
            }

            foreach (var module in GetModules())
            {
                module.Register();
                Log.Message($"[RimMind-Bridge-RimTalk] {module.Id} registered={module.IsRegistered}.");
            }

            Log.Message("[RimMind-Bridge-RimTalk] Initialized.");
        }

        public static void Unregister()
        {
            foreach (var module in GetModules())
            {
                module.Unregister();
            }
        }
    }
}
