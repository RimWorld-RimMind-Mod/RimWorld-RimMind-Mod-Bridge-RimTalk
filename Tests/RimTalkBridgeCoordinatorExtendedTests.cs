using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class RimTalkBridgeCoordinatorExtendedTests
    {
        public RimTalkBridgeCoordinatorExtendedTests()
        {
            RimTalkDetector.IsRimTalkActive = false;
            RimTalkDetector.IsRimTalkApiAvailable = false;
            BridgeRimTalkSettings.Reset();
            ContextPullBridge.RegisterCalled = false;
            ContextPullBridge.UnregisterCalled = false;
            ContextPushBridge.RegisterCalled = false;
            ContextPushBridge.UnregisterCalled = false;
            PersonaPushBridge.RegisterCalled = false;
            PersonaPushBridge.UnregisterCalled = false;
        }

        [Fact]
        public void Register_ApiAvailableButPushPersonalityFalse_SkipsPersona()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = true;
            BridgeRimTalkSettings.Get().pushPersonality = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(ContextPushBridge.RegisterCalled);
            Assert.False(PersonaPushBridge.RegisterCalled);
        }

        [Fact]
        public void Register_RimTalkActiveButApiUnavailable_SkipsPushBridges()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(ContextPullBridge.RegisterCalled);
            Assert.False(ContextPushBridge.RegisterCalled);
            Assert.False(PersonaPushBridge.RegisterCalled);
        }

        [Fact]
        public void Unregister_AlwaysCallsAllUnregisters()
        {
            // 即使没有 Register 过，Unregister 也应调用所有桥接模块
            RimTalkBridgeCoordinator.Unregister();

            Assert.True(ContextPullBridge.UnregisterCalled);
            Assert.True(ContextPushBridge.UnregisterCalled);
            Assert.True(PersonaPushBridge.UnregisterCalled);
        }

        [Fact]
        public void Register_CalledTwice_RegistersTwice()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = true;
            BridgeRimTalkSettings.Get().pushPersonality = true;

            RimTalkBridgeCoordinator.Register();
            // 重置标志
            ContextPullBridge.RegisterCalled = false;
            ContextPushBridge.RegisterCalled = false;
            PersonaPushBridge.RegisterCalled = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(ContextPullBridge.RegisterCalled);
            Assert.True(ContextPushBridge.RegisterCalled);
            Assert.True(PersonaPushBridge.RegisterCalled);
        }
    }
}
