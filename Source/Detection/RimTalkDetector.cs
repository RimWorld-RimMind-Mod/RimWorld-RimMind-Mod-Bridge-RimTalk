using HarmonyLib;
using RimMind.Application.Features.Bridges;
using Verse;

namespace RimMind.Bridge.RimTalk.Detection
{
    public static class RimTalkDetector
    {
        public const string RimTalkPackageId = "cj.rimtalk";
        private const string RimTalkApiTypeName = "RimTalk.API.RimTalkPromptAPI";
        private const int CacheIntervalTicks = 6000;

        internal static readonly SoftDependencyDetector Detector = new SoftDependencyDetector(
            RimTalkPackageId,
            new[] { RimTalkApiTypeName },
            ModsConfig.IsActive,
            AccessTools.TypeByName,
            () => SafeTicksGame,
            CacheIntervalTicks);

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
            get => Detector.IsModActive();
            internal set => Detector.SetForcedModActiveForTesting(value);
        }

        public static void InvalidateCache()
        {
            Detector.ResetCache();
        }

        public static bool IsRimTalkApiAvailable
        {
            get => Detector.IsAvailable();
            internal set => Detector.SetForcedApiAvailableForTesting(value);
        }
    }
}
