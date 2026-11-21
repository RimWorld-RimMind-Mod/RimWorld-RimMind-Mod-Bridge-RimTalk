using RimMind.Application.Features.Bridges;
using RimMind.Presentation.Api;

namespace RimMind.Bridge.RimTalk.Detection
{
    public static class RimTalkDetector
    {
        public const string RimTalkPackageId = "cj.rimtalk";
        private const string RimTalkApiTypeName = "RimTalk.API.RimTalkPromptAPI";
        private const int CacheIntervalTicks = 6000;

        internal static readonly SoftDependencyDetector Detector = RimMindAPI.Bridges.CreateDetector(
            RimTalkPackageId,
            new[] { RimTalkApiTypeName },
            CacheIntervalTicks);

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
