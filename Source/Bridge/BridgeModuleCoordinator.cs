using System;
using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Features.Bridges;
using RimMind.Presentation.Api;

namespace RimMind.Bridge.RimTalk.Bridge
{
    [Obsolete("Use RimMindAPI.Bridges.CreateCoordinator / ExternalModBridgeCoordinator directly.")]
    internal sealed class BridgeModuleCoordinator
    {
        private readonly ExternalModBridgeCoordinator _inner;

        public BridgeModuleCoordinator(
            IReadOnlyList<IBridgeModule> modules,
            Func<bool> isDependencyActive,
            Action<string> log,
            Action<string> warn)
        {
            _inner = RimMindAPI.Bridges.CreateCoordinator(
                isDependencyActive,
                modules,
                gates: null,
                logInfo: log,
                logWarn: warn,
                inactiveMessage: "RimTalk not active, bridge modules skipped.");
        }

        public bool AreAllRegistered => _inner.AreAllRegistered;
        public void RegisterAll() => _inner.RegisterAll();
        public void UnregisterAll() => _inner.UnregisterAll();
        public void Refresh() => _inner.Refresh();
    }
}
