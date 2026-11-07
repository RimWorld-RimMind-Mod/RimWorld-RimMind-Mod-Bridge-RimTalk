using System;
using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Features.Bridges;

namespace RimMind.Bridge.RimTalk.Bridge
{
    internal sealed class BridgeModuleCoordinator
    {
        private readonly ExternalModBridgeCoordinator _inner;

        public BridgeModuleCoordinator(
            IReadOnlyList<IBridgeModule> modules,
            Func<bool> isDependencyActive,
            Action<string> log,
            Action<string> warn)
        {
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            if (isDependencyActive == null) throw new ArgumentNullException(nameof(isDependencyActive));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (warn == null) throw new ArgumentNullException(nameof(warn));

            _inner = new ExternalModBridgeCoordinator(
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
