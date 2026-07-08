using RimMind.Application.Common.Interfaces.Extension;
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
        }

        [Fact]
        public void ContextPullBridge_Register_SetsFlagAndIsRegistered()
        {
            var bridge = new ContextPullBridge();
            bridge.Register();
            Assert.True(bridge.RegisterCalled);
            Assert.True(bridge.IsRegistered);
        }

        [Fact]
        public void ContextPullBridge_Unregister_WhenRegistered_ClearsIsRegistered()
        {
            var bridge = new ContextPullBridge();
            bridge.Register();
            bridge.Unregister();
            Assert.True(bridge.UnregisterCalled);
            Assert.False(bridge.IsRegistered);
        }

        [Fact]
        public void ContextPullBridge_Unregister_WhenNotRegistered_IsNoOp()
        {
            var bridge = new ContextPullBridge();
            bridge.Unregister();
            Assert.False(bridge.UnregisterCalled);
            Assert.False(bridge.IsRegistered);
        }

        [Fact]
        public void ContextPullBridge_Register_Twice_IsIdempotent()
        {
            var bridge = new ContextPullBridge();
            bridge.Register();
            bridge.RegisterCalled = false;
            bridge.Register();
            Assert.False(bridge.RegisterCalled);
            Assert.True(bridge.IsRegistered);
        }

        [Fact]
        public void ContextPushBridge_Register_SetsFlagAndIsRegistered()
        {
            var bridge = new ContextPushBridge();
            bridge.Register();
            Assert.True(bridge.RegisterCalled);
            Assert.True(bridge.IsRegistered);
        }

        [Fact]
        public void ContextPushBridge_Unregister_WhenRegistered_ClearsIsRegistered()
        {
            var bridge = new ContextPushBridge();
            bridge.Register();
            bridge.Unregister();
            Assert.True(bridge.UnregisterCalled);
            Assert.False(bridge.IsRegistered);
        }

        [Fact]
        public void ContextPushBridge_Unregister_WhenNotRegistered_IsNoOp()
        {
            var bridge = new ContextPushBridge();
            bridge.Unregister();
            Assert.False(bridge.UnregisterCalled);
            Assert.False(bridge.IsRegistered);
        }

        [Fact]
        public void PersonaPushBridge_Register_SetsFlagAndIsRegistered()
        {
            var bridge = new PersonaPushBridge();
            bridge.Register();
            Assert.True(bridge.RegisterCalled);
            Assert.True(bridge.IsRegistered);
        }

        [Fact]
        public void PersonaPushBridge_Unregister_WhenRegistered_ClearsIsRegistered()
        {
            var bridge = new PersonaPushBridge();
            bridge.Register();
            bridge.Unregister();
            Assert.True(bridge.UnregisterCalled);
            Assert.False(bridge.IsRegistered);
        }

        [Fact]
        public void PersonaPushBridge_Unregister_WhenNotRegistered_IsNoOp()
        {
            var bridge = new PersonaPushBridge();
            bridge.Unregister();
            Assert.False(bridge.UnregisterCalled);
            Assert.False(bridge.IsRegistered);
        }

        [Fact]
        public void AllBridges_ImplementIBridgeModule_WithCorrectIds()
        {
            IBridgeModule pull = new ContextPullBridge();
            IBridgeModule push = new ContextPushBridge();
            IBridgeModule persona = new PersonaPushBridge();

            Assert.Equal("context_pull", pull.Id);
            Assert.Equal("context_push", push.Id);
            Assert.Equal("persona_push", persona.Id);
            Assert.All(new[] { pull, push, persona }, m => Assert.Equal("RimMindBridgeRimTalk", m.OwnerModId));
            Assert.All(new[] { pull, push, persona }, m => Assert.False(m.IsRegistered));
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
