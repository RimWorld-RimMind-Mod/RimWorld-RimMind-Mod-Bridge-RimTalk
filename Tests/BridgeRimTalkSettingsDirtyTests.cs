using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class BridgeRimTalkSettingsDirtyTests
    {
        public BridgeRimTalkSettingsDirtyTests()
        {
            BridgeRimTalkSettings.Reset();
            BridgeRimTalkSettings.ResetDirtyForTesting();
        }

        [Fact]
        public void IsDirtyForTesting_DefaultFalse()
        {
            Assert.False(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void MarkDirtyIfChanged_SameSnapshot_DoesNotMarkDirty()
        {
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();

            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);

            Assert.False(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void MarkDirtyIfChanged_DifferentSnapshot_MarksDirty()
        {
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();
            BridgeRimTalkSettings.Get().enableDialogueGate = !BridgeRimTalkSettings.Get().enableDialogueGate;

            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);

            Assert.True(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void MarkDirtyIfChanged_SnapshotMatchesNonDefaultState_DoesNotMarkDirty()
        {
            // Set all fields to non-default values, snapshot, then compare — no change, no dirty.
            var fields = new (bool EnableDialogueGate, bool SkipChitchat, bool SkipAutoDialogue,
                bool SkipPlayerDialogue, bool ForceRimMindPlayerDialogue,
                bool EnableContextPush, bool PushPersonality, bool PushStoryteller,
                bool PushMemory, bool PushAdvisorLog, bool PushShaping,
                bool InjectPersonaToTraits, bool InjectPersonaToMood,
                bool EnableContextPull, bool PullRimTalkHistory)[]
            {
                (false, true, true, true, false, true, true, true, false, true, false, false, false, true, true),
                (true, false, true, true, false, true, true, true, false, true, false, false, false, true, true),
                (true, true, false, true, false, true, true, true, false, true, false, false, false, true, true),
                (true, true, true, false, false, true, true, true, false, true, false, false, false, true, true),
                (true, true, true, true, true, true, true, true, false, true, false, false, false, true, true),
                (true, true, true, true, false, false, true, true, false, true, false, false, false, true, true),
                (true, true, true, true, false, true, false, true, false, true, false, false, false, true, true),
                (true, true, true, true, false, true, true, false, false, true, false, false, false, true, true),
                (true, true, true, true, false, true, true, true, true, true, false, false, false, true, true),
                (true, true, true, true, false, true, true, true, false, false, false, false, false, true, true),
                (true, true, true, true, false, true, true, true, false, true, true, false, false, true, true),
                (true, true, true, true, false, true, true, true, false, true, false, true, false, true, true),
                (true, true, true, true, false, true, true, true, false, true, false, false, true, true, true),
                (true, true, true, true, false, true, true, true, false, true, false, false, false, false, true),
                (true, true, true, true, false, true, true, true, false, true, false, false, false, true, false),
            };

            foreach (var field in fields)
            {
                BridgeRimTalkSettings.Reset();
                BridgeRimTalkSettings.ResetDirtyForTesting();
                var s = BridgeRimTalkSettings.Get();
                s.enableDialogueGate = field.EnableDialogueGate;
                s.skipChitchat = field.SkipChitchat;
                s.skipAutoDialogue = field.SkipAutoDialogue;
                s.skipPlayerDialogue = field.SkipPlayerDialogue;
                s.forceRimMindPlayerDialogue = field.ForceRimMindPlayerDialogue;
                s.enableContextPush = field.EnableContextPush;
                s.pushPersonality = field.PushPersonality;
                s.pushStoryteller = field.PushStoryteller;
                s.pushMemory = field.PushMemory;
                s.pushAdvisorLog = field.PushAdvisorLog;
                s.pushShaping = field.PushShaping;
                s.injectPersonaToTraits = field.InjectPersonaToTraits;
                s.injectPersonaToMood = field.InjectPersonaToMood;
                s.enableContextPull = field.EnableContextPull;
                s.pullRimTalkHistory = field.PullRimTalkHistory;

                var snapshot = BridgeRimTalkSettings.CaptureSnapshot();
                // No change — should not be dirty
                BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);
                Assert.False(BridgeRimTalkSettings.IsDirtyForTesting);
            }
        }

        [Theory]
        [InlineData("enableDialogueGate")]
        [InlineData("skipChitchat")]
        [InlineData("skipAutoDialogue")]
        [InlineData("skipPlayerDialogue")]
        [InlineData("forceRimMindPlayerDialogue")]
        [InlineData("enableContextPush")]
        [InlineData("pushPersonality")]
        [InlineData("pushStoryteller")]
        [InlineData("pushMemory")]
        [InlineData("pushAdvisorLog")]
        [InlineData("pushShaping")]
        [InlineData("injectPersonaToTraits")]
        [InlineData("injectPersonaToMood")]
        [InlineData("enableContextPull")]
        [InlineData("pullRimTalkHistory")]
        public void MarkDirtyIfChanged_FlippingAnySingleField_MarksDirty(string fieldName)
        {
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();
            var s = BridgeRimTalkSettings.Get();

            switch (fieldName)
            {
                case "enableDialogueGate": s.enableDialogueGate = !s.enableDialogueGate; break;
                case "skipChitchat": s.skipChitchat = !s.skipChitchat; break;
                case "skipAutoDialogue": s.skipAutoDialogue = !s.skipAutoDialogue; break;
                case "skipPlayerDialogue": s.skipPlayerDialogue = !s.skipPlayerDialogue; break;
                case "forceRimMindPlayerDialogue": s.forceRimMindPlayerDialogue = !s.forceRimMindPlayerDialogue; break;
                case "enableContextPush": s.enableContextPush = !s.enableContextPush; break;
                case "pushPersonality": s.pushPersonality = !s.pushPersonality; break;
                case "pushStoryteller": s.pushStoryteller = !s.pushStoryteller; break;
                case "pushMemory": s.pushMemory = !s.pushMemory; break;
                case "pushAdvisorLog": s.pushAdvisorLog = !s.pushAdvisorLog; break;
                case "pushShaping": s.pushShaping = !s.pushShaping; break;
                case "injectPersonaToTraits": s.injectPersonaToTraits = !s.injectPersonaToTraits; break;
                case "injectPersonaToMood": s.injectPersonaToMood = !s.injectPersonaToMood; break;
                case "enableContextPull": s.enableContextPull = !s.enableContextPull; break;
                case "pullRimTalkHistory": s.pullRimTalkHistory = !s.pullRimTalkHistory; break;
            }

            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);
            Assert.True(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void MarkDirtyIfChanged_MultipleChanges_MarksDirty()
        {
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();
            var s = BridgeRimTalkSettings.Get();
            s.enableDialogueGate = !s.enableDialogueGate;
            s.pushMemory = !s.pushMemory;
            s.pullRimTalkHistory = !s.pullRimTalkHistory;

            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);

            Assert.True(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void ResetDirtyForTesting_ClearsDirtyFlag()
        {
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();
            BridgeRimTalkSettings.Get().pushMemory = !BridgeRimTalkSettings.Get().pushMemory;
            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);
            Assert.True(BridgeRimTalkSettings.IsDirtyForTesting);

            BridgeRimTalkSettings.ResetDirtyForTesting();

            Assert.False(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void CaptureSnapshot_ReflectsCurrentValues()
        {
            var s = BridgeRimTalkSettings.Get();
            s.enableDialogueGate = false;
            s.pushMemory = true;
            s.injectPersonaToTraits = true;
            s.pullRimTalkHistory = false;

            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();

            // Field order: enableDialogueGate, skipChitchat, skipAutoDialogue, skipPlayerDialogue,
            // forceRimMindPlayerDialogue, enableContextPush, pushPersonality, pushStoryteller,
            // pushMemory, pushAdvisorLog, pushShaping, injectPersonaToTraits, injectPersonaToMood,
            // enableContextPull, pullRimTalkHistory
            Assert.False(snapshot.Item1);   // enableDialogueGate
            Assert.True(snapshot.Item9);    // pushMemory
            Assert.True(snapshot.Item12);   // injectPersonaToTraits
            Assert.False(snapshot.Item15);  // pullRimTalkHistory
        }

        [Fact]
        public void DirtyCycle_SimulatesDrawSettingsContentFlow_NoChangeNoWrite()
        {
            // Simulate: snapshot -> no change -> no dirty -> no write
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();

            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);

            Assert.False(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void DirtyCycle_SimulatesDrawSettingsContentFlow_ChangeThenWrite()
        {
            // Simulate: snapshot -> change -> dirty -> write (clear) -> not dirty
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();
            BridgeRimTalkSettings.Get().skipChitchat = !BridgeRimTalkSettings.Get().skipChitchat;

            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);
            Assert.True(BridgeRimTalkSettings.IsDirtyForTesting);

            // Simulate Write() + _dirty = false
            BridgeRimTalkSettings.ResetDirtyForTesting();
            Assert.False(BridgeRimTalkSettings.IsDirtyForTesting);

            // Next frame: snapshot from current state, no further change -> not dirty
            var snapshot2 = BridgeRimTalkSettings.CaptureSnapshot();
            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot2);
            Assert.False(BridgeRimTalkSettings.IsDirtyForTesting);
        }

        [Fact]
        public void MarkDirtyIfChanged_RepeatedCalls_KeepsDirtyTrue()
        {
            var snapshot = BridgeRimTalkSettings.CaptureSnapshot();
            BridgeRimTalkSettings.Get().enableDialogueGate = false;

            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);
            Assert.True(BridgeRimTalkSettings.IsDirtyForTesting);

            // Calling again with original snapshot (state still differs) should keep dirty true
            BridgeRimTalkSettings.MarkDirtyIfChanged(snapshot);
            Assert.True(BridgeRimTalkSettings.IsDirtyForTesting);
        }
    }
}
