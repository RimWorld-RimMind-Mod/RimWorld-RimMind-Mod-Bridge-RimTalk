using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class RimTalkDialogueGateTests
    {
        public RimTalkDialogueGateTests()
        {
            RimTalkDetector.IsRimTalkActive = false;
            BridgeRimTalkSettings.Reset();
        }

        [Fact]
        public void ShouldSkipDialogue_RimTalkInactive_ReturnsFalse()
        {
            RimTalkDetector.IsRimTalkActive = false;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipDialogue_GateDisabled_ReturnsFalse()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.enableDialogueGate = false;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipDialogue_ChibatEnabled_Skips()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipChitchat = true;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipDialogue_ChibatDisabled_DoesNotSkip()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipChitchat = false;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipDialogue_AutoEnabled_Skips()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipAutoDialogue = true;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Auto"));
        }

        [Fact]
        public void ShouldSkipDialogue_PlayerInputEnabled_Skips()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipPlayerDialogue = true;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "PlayerInput"));
        }

        [Fact]
        public void ShouldSkipDialogue_UnknownTrigger_DoesNotSkip()
        {
            RimTalkDetector.IsRimTalkActive = true;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Unknown"));
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_RimTalkInactive_ReturnsFalse()
        {
            RimTalkDetector.IsRimTalkActive = false;
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_SkipPlayerNoForce_Skips()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipPlayerDialogue = true;
            settings.forceRimMindPlayerDialogue = false;
            Assert.True(DialogueGate.ShouldSkipFloatMenuOption());
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_ForceRimMind_DoesNotSkip()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipPlayerDialogue = true;
            settings.forceRimMindPlayerDialogue = true;
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_GateDisabled_DoesNotSkip()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.enableDialogueGate = false;
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }
    }
}
