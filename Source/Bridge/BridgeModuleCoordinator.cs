using System;
using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimTalk.Bridge
{
    internal sealed class BridgeModuleCoordinator
    {
        private readonly IReadOnlyList<IBridgeModule> _modules;
        private readonly Func<bool> _isDependencyActive;
        private readonly Action<string> _log;
        private readonly Action<string> _warn;

        public BridgeModuleCoordinator(
            IReadOnlyList<IBridgeModule> modules,
            Func<bool> isDependencyActive,
            Action<string> log,
            Action<string> warn)
        {
            _modules = modules ?? throw new ArgumentNullException(nameof(modules));
            _isDependencyActive = isDependencyActive
                ?? throw new ArgumentNullException(nameof(isDependencyActive));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _warn = warn ?? throw new ArgumentNullException(nameof(warn));
        }

        public void RegisterAll()
        {
            if (!_isDependencyActive())
            {
                _log("RimTalk not active, bridge modules skipped.");
                return;
            }

            foreach (IBridgeModule module in _modules)
            {
                try
                {
                    module.Register();
                    _log($"{module.Id} registered={module.IsRegistered}.");
                }
                catch (Exception ex)
                {
                    _warn($"{module.Id} registration failed: {ex.Message}");
                }
            }
        }

        public void UnregisterAll()
        {
            foreach (IBridgeModule module in _modules)
            {
                try
                {
                    module.Unregister();
                }
                catch (Exception ex)
                {
                    _warn($"{module.Id} cleanup failed: {ex.Message}");
                }
            }
        }
    }
}
