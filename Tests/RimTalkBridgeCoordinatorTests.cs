using System;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Core;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class RimTalkBridgeCoordinatorTests
    {
        public RimTalkBridgeCoordinatorTests()
        {
            RimTalkDetector.IsRimTalkActive = false;
            RimTalkDetector.IsRimTalkApiAvailable = false;
            BridgeRimTalkSettings.Reset();
            RimMindAPI.ResetCounts();
            ContextPullBridge.RegisterCalled = false;
            ContextPullBridge.UnregisterCalled = false;
            ContextPushBridge.RegisterCalled = false;
            ContextPushBridge.UnregisterCalled = false;
            PersonaPushBridge.RegisterCalled = false;
            PersonaPushBridge.UnregisterCalled = false;
        }

        [Fact]
        public void Register_RimTalkNotActive_SkipsAll()
        {
            RimTalkDetector.IsRimTalkActive = false;

            RimTalkBridgeCoordinator.Register();

            Assert.Equal(0, RimMindAPI.DialogueSkipCheckCount);
            Assert.False(ContextPullBridge.RegisterCalled);
            Assert.False(ContextPushBridge.RegisterCalled);
            Assert.False(PersonaPushBridge.RegisterCalled);
        }

        [Fact]
        public void Register_RimTalkActive_RegistersDialogueGateAndContextPull()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(RimMindAPI.DialogueSkipCheckCount > 0);
            Assert.True(ContextPullBridge.RegisterCalled);
            Assert.False(ContextPushBridge.RegisterCalled);
            Assert.False(PersonaPushBridge.RegisterCalled);
        }

        [Fact]
        public void Register_ApiAvailable_RegistersPushBridges()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = true;
            BridgeRimTalkSettings.Get().pushPersonality = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(RimMindAPI.DialogueSkipCheckCount > 0);
            Assert.True(ContextPullBridge.RegisterCalled);
            Assert.True(ContextPushBridge.RegisterCalled);
            Assert.False(PersonaPushBridge.RegisterCalled);
        }

        [Fact]
        public void Register_ApiAvailableAndPushPersonality_RegistersAll()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = true;
            BridgeRimTalkSettings.Get().pushPersonality = true;

            RimTalkBridgeCoordinator.Register();

            Assert.True(RimMindAPI.DialogueSkipCheckCount > 0);
            Assert.True(ContextPullBridge.RegisterCalled);
            Assert.True(ContextPushBridge.RegisterCalled);
            Assert.True(PersonaPushBridge.RegisterCalled);
        }

        [Fact]
        public void Unregister_CallsUnregisterOnAllBridges()
        {
            RimTalkBridgeCoordinator.Unregister();

            Assert.True(ContextPullBridge.UnregisterCalled);
            Assert.True(ContextPushBridge.UnregisterCalled);
            Assert.True(PersonaPushBridge.UnregisterCalled);
        }
    }
}
