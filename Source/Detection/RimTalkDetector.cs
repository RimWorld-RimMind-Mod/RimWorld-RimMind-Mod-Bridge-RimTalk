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
                try
                {
                    var tm = Find.TickManager;
                    if (tm == null) return 0;
                    return tm.TicksGame;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static bool IsRimTalkActive
        {
            get
            {
                int now = SafeTicksGame;
                if (_cachedResult == null || (now > 0 && now - _cacheTick > CacheIntervalTicks))
                {
                    _cachedResult = ModsConfig.IsActive(RimTalkPackageId);
                    if (now > 0)
                    {
                        _cacheTick = now;
                    }
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
