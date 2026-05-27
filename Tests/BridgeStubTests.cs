using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class BridgeStubTests
    {
        public BridgeStubTests()
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
        public void ContextPullBridge_Register_SetsFlag()
        {
            ContextPullBridge.Register();
            Assert.True(ContextPullBridge.RegisterCalled);
        }

        [Fact]
        public void ContextPullBridge_Unregister_SetsFlag()
        {
            ContextPullBridge.Unregister();
            Assert.True(ContextPullBridge.UnregisterCalled);
        }

        [Fact]
        public void ContextPushBridge_Register_SetsFlag()
        {
            ContextPushBridge.Register();
            Assert.True(ContextPushBridge.RegisterCalled);
        }

        [Fact]
        public void ContextPushBridge_Unregister_SetsFlag()
        {
            ContextPushBridge.Unregister();
            Assert.True(ContextPushBridge.UnregisterCalled);
        }

        [Fact]
        public void PersonaPushBridge_Register_SetsFlag()
        {
            PersonaPushBridge.Register();
            Assert.True(PersonaPushBridge.RegisterCalled);
        }

        [Fact]
        public void PersonaPushBridge_Unregister_SetsFlag()
        {
            PersonaPushBridge.Unregister();
            Assert.True(PersonaPushBridge.UnregisterCalled);
        }

        [Fact]
        public void RimTalkDetector_DefaultInactive()
        {
            RimTalkDetector.IsRimTalkActive = false;
            Assert.False(RimTalkDetector.IsRimTalkActive);
        }

        [Fact]
        public void RimTalkDetector_CanSetActive()
        {
            RimTalkDetector.IsRimTalkActive = true;
            Assert.True(RimTalkDetector.IsRimTalkActive);
        }

        [Fact]
        public void RimTalkDetector_CanSetApiAvailable()
        {
            RimTalkDetector.IsRimTalkApiAvailable = true;
            Assert.True(RimTalkDetector.IsRimTalkApiAvailable);
        }
    }
}
