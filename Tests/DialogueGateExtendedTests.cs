using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class DialogueGateExtendedTests
    {
        public DialogueGateExtendedTests()
        {
            RimTalkDetector.IsRimTalkActive = false;
            BridgeRimTalkSettings.Reset();
        }

        [Fact]
        public void ShouldSkipDialogue_AllTriggersDisabled_NoneSkipped()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipChitchat = false;
            settings.skipAutoDialogue = false;
            settings.skipPlayerDialogue = false;

            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Auto"));
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "PlayerInput"));
        }

        [Fact]
        public void ShouldSkipDialogue_AllTriggersEnabled_AllSkipped()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipChitchat = true;
            settings.skipAutoDialogue = true;
            settings.skipPlayerDialogue = true;

            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Auto"));
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "PlayerInput"));
        }

        [Fact]
        public void ShouldSkipDialogue_EmptyTriggerType_DoesNotSkip()
        {
            RimTalkDetector.IsRimTalkActive = true;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, ""));
        }

        [Fact]
        public void ShouldSkipDialogue_NullPawn_StillWorks()
        {
            // pawn 参数在当前实现中未使用
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipChitchat = true;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_SkipPlayerDisabled_DoesNotSkip()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipPlayerDialogue = false;
            settings.forceRimMindPlayerDialogue = false;
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_ForceWithoutSkip_DoesNotSkip()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.skipPlayerDialogue = false;
            settings.forceRimMindPlayerDialogue = true;
            // skipPlayerDialogue 为 false，不跳过
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }

        [Fact]
        public void ShouldSkipDialogue_GateDisabled_IgnoresAllSettings()
        {
            RimTalkDetector.IsRimTalkActive = true;
            var settings = BridgeRimTalkSettings.Get();
            settings.enableDialogueGate = false;
            settings.skipChitchat = true;
            settings.skipAutoDialogue = true;
            settings.skipPlayerDialogue = true;

            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Auto"));
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "PlayerInput"));
        }
    }
}
