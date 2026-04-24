using System;
using HarmonyLib;
using Verse;

namespace RimMind.Bridge.RimTalk.Detection
{
    public static class RimTalkDetector
    {
        public const string RimTalkPackageId = "cj.rimtalk";

        private static bool? _cachedResult;
        private static int _cacheTick = -1;
        private const int CacheIntervalTicks = 6000;

        private static int SafeTicksGame
        {
            get
            {
                try { return Find.TickManager?.TicksGame ?? 0; }
                catch { Log.Warning("[RimMind-Bridge-RimTalk] Failed to access TickManager.TicksGame, returning 0."); return 0; }
            }
        }

        public static bool IsRimTalkActive
        {
            get
            {
                int now = SafeTicksGame;
                if (_cachedResult == null || now - _cacheTick > CacheIntervalTicks)
                {
                    _cachedResult = ModsConfig.IsActive(RimTalkPackageId);
                    _cacheTick = now;
                }
                return _cachedResult.Value;
            }
        }

        public static void InvalidateCache()
        {
            _cachedResult = null;
            _cacheTick = -1;
        }

        private static bool? _apiAvailable;
        private static bool _apiChecked;

        public static bool IsRimTalkApiAvailable
        {
            get
            {
                if (!_apiChecked)
                {
                    _apiAvailable = IsRimTalkActive && AccessTools.TypeByName("RimTalk.API.RimTalkPromptAPI") != null;
                    _apiChecked = true;
                }
                return _apiAvailable ?? false;
            }
        }
    }
}
