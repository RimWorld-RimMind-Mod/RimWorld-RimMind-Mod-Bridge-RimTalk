using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class BridgeRimTalkSettingsTests
    {
        public BridgeRimTalkSettingsTests()
        {
            BridgeRimTalkSettings.Reset();
        }

        [Fact]
        public void Get_ReturnsNonNullInstance()
        {
            var settings = BridgeRimTalkSettings.Get();
            Assert.NotNull(settings);
        }

        [Fact]
        public void Get_ReturnsSameInstance()
        {
            var s1 = BridgeRimTalkSettings.Get();
            var s2 = BridgeRimTalkSettings.Get();
            Assert.Same(s1, s2);
        }

        [Fact]
        public void Reset_CreatesNewInstance()
        {
            var s1 = BridgeRimTalkSettings.Get();
            BridgeRimTalkSettings.Reset();
            var s2 = BridgeRimTalkSettings.Get();
            Assert.NotSame(s1, s2);
        }

        [Fact]
        public void Default_EnableDialogueGate_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.enableDialogueGate);
        }

        [Fact]
        public void Default_SkipChitchat_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.skipChitchat);
        }

        [Fact]
        public void Default_SkipAutoDialogue_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.skipAutoDialogue);
        }

        [Fact]
        public void Default_SkipPlayerDialogue_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.skipPlayerDialogue);
        }

        [Fact]
        public void Default_ForceRimMindPlayerDialogue_IsFalse()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.False(settings.forceRimMindPlayerDialogue);
        }

        [Fact]
        public void Default_EnableContextPush_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.enableContextPush);
        }

        [Fact]
        public void Default_PushPersonality_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.pushPersonality);
        }

        [Fact]
        public void Default_PushStoryteller_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.pushStoryteller);
        }

        [Fact]
        public void Default_PushMemory_IsFalse()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.False(settings.pushMemory);
        }

        [Fact]
        public void Default_PushAdvisorLog_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.pushAdvisorLog);
        }

        [Fact]
        public void Default_PushShaping_IsFalse()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.False(settings.pushShaping);
        }

        [Fact]
        public void Default_EnableContextPull_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.enableContextPull);
        }

        [Fact]
        public void Default_PullRimTalkHistory_IsTrue()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.True(settings.pullRimTalkHistory);
        }

        [Fact]
        public void Constructor_SetsSingletonInstance()
        {
            var settings = new BridgeRimTalkSettings();
            Assert.Same(settings, BridgeRimTalkSettings.Get());
        }
    }
}
