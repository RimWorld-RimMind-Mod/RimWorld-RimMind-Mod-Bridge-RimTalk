using RimMind.Bridge.RimTalk.Detection;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class RimTalkDetectorCacheTests
    {
        public RimTalkDetectorCacheTests()
        {
            // 重置静态缓存状态，隔离每个测试
            RimTalkDetector.IsRimTalkActive = false;
            RimTalkDetector.IsRimTalkApiAvailable = false;
        }

        [Fact]
        public void InvalidateCache_ResetsApiAvailableFlag()
        {
            // 模拟 RimTalk API 已检测为可用（_apiChecked=true, _apiAvailable=true）
            RimTalkDetector.IsRimTalkApiAvailable = true;
            Assert.True(RimTalkDetector.IsRimTalkApiAvailable);

            // 运行时卸载/重载场景：失效缓存后，API 可用性应回到默认（重新检测前为 false）
            RimTalkDetector.InvalidateCache();

            Assert.False(RimTalkDetector.IsRimTalkApiAvailable);
        }

        [Fact]
        public void InvalidateCache_ResetsRimTalkActiveFlag()
        {
            // 模拟 RimTalk 已激活（_cachedResult=true）
            RimTalkDetector.IsRimTalkActive = true;
            Assert.True(RimTalkDetector.IsRimTalkActive);

            // 失效缓存后，激活状态应回到默认（重新检测前为 false）
            RimTalkDetector.InvalidateCache();

            Assert.False(RimTalkDetector.IsRimTalkActive);
        }
    }
}
