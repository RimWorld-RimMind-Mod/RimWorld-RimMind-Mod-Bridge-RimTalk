using UnityEngine;
using Verse;
using RimMind.Core.UI;

namespace RimMind.Bridge.RimTalk.Settings
{
    public class BridgeRimTalkSettings : ModSettings
    {
        public bool enableDialogueGate = true;
        public bool skipChitchat = true;
        public bool skipAutoDialogue = true;
        public bool skipPlayerDialogue = true;
        public bool forceRimMindPlayerDialogue = false;

        public bool enableContextPush = true;
        public bool pushPersonality = true;
        public bool pushStoryteller = true;
        public bool pushMemory = false;
        public bool pushAdvisorLog = true;
        public bool pushShaping = false;
        public bool enableRimTalkHistoryPull = true;

        public bool enablePersonaPush = false;
        public bool pushPersonaToTraits = true;
        public bool pushPersonaToMood = false;

        private static BridgeRimTalkSettings? _instance;
        public static BridgeRimTalkSettings Get() => _instance ?? new BridgeRimTalkSettings();

        public BridgeRimTalkSettings()
        {
            _instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableDialogueGate, "enableDialogueGate", true);
            Scribe_Values.Look(ref skipChitchat, "skipChitchat", true);
            Scribe_Values.Look(ref skipAutoDialogue, "skipAutoDialogue", true);
            Scribe_Values.Look(ref skipPlayerDialogue, "skipPlayerDialogue", true);
            Scribe_Values.Look(ref forceRimMindPlayerDialogue, "forceRimMindPlayerDialogue", false);

            Scribe_Values.Look(ref enableContextPush, "enableContextPush", true);
            Scribe_Values.Look(ref pushPersonality, "pushPersonality", true);
            Scribe_Values.Look(ref pushStoryteller, "pushStoryteller", true);
            Scribe_Values.Look(ref pushMemory, "pushMemory", false);
            Scribe_Values.Look(ref pushAdvisorLog, "pushAdvisorLog", true);
            Scribe_Values.Look(ref pushShaping, "pushShaping", false);
            Scribe_Values.Look(ref enableRimTalkHistoryPull, "enableRimTalkHistoryPull", true);

            Scribe_Values.Look(ref enablePersonaPush, "enablePersonaPush", false);
            Scribe_Values.Look(ref pushPersonaToTraits, "pushPersonaToTraits", true);
            Scribe_Values.Look(ref pushPersonaToMood, "pushPersonaToMood", false);
        }

        private static Vector2 _scrollPos = Vector2.zero;

        public static void DrawSettingsContent(Rect inRect)
        {
            var s = Get();

            Rect contentArea = SettingsUIHelper.SplitContentArea(inRect);
            Rect bottomBar = SettingsUIHelper.SplitBottomBar(inRect);

            float contentH = EstimateHeight(s);
            Rect viewRect = new Rect(0f, 0f, contentArea.width - 16f, contentH);
            Widgets.BeginScrollView(contentArea, ref _scrollPos, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            SettingsUIHelper.DrawSectionHeader(listing, "RimMind.BridgeRimTalk.Settings.Section.DialogueGate".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.EnableDialogueGate".Translate(),
                ref s.enableDialogueGate,
                "RimMind.BridgeRimTalk.Settings.EnableDialogueGate.Desc".Translate());
            if (s.enableDialogueGate)
            {
                listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.SkipChitchat".Translate(),
                    ref s.skipChitchat,
                    "RimMind.BridgeRimTalk.Settings.SkipChitchat.Desc".Translate());
                listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.SkipAutoDialogue".Translate(),
                    ref s.skipAutoDialogue,
                    "RimMind.BridgeRimTalk.Settings.SkipAutoDialogue.Desc".Translate());
                listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.SkipPlayerDialogue".Translate(),
                    ref s.skipPlayerDialogue,
                    "RimMind.BridgeRimTalk.Settings.SkipPlayerDialogue.Desc".Translate());
                if (s.skipPlayerDialogue)
                {
                    listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.ForceRimMindPlayerDialogue".Translate(),
                        ref s.forceRimMindPlayerDialogue,
                        "RimMind.BridgeRimTalk.Settings.ForceRimMindPlayerDialogue.Desc".Translate());
                }
            }

            SettingsUIHelper.DrawSectionHeader(listing, "RimMind.BridgeRimTalk.Settings.Section.ContextPush".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.EnableContextPush".Translate(),
                ref s.enableContextPush,
                "RimMind.BridgeRimTalk.Settings.EnableContextPush.Desc".Translate());
            if (s.enableContextPush)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushPersonality".Translate(),
                    ref s.pushPersonality);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushStoryteller".Translate(),
                    ref s.pushStoryteller);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushMemory".Translate(),
                    ref s.pushMemory);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushShaping".Translate(),
                    ref s.pushShaping);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushAdvisorLog".Translate(),
                    ref s.pushAdvisorLog);
                listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.EnableRimTalkHistoryPull".Translate(),
                    ref s.enableRimTalkHistoryPull,
                    "RimMind.BridgeRimTalk.Settings.EnableRimTalkHistoryPull.Desc".Translate());
            }

            SettingsUIHelper.DrawSectionHeader(listing, "RimMind.BridgeRimTalk.Settings.Section.PersonaPush".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.EnablePersonaPush".Translate(),
                ref s.enablePersonaPush,
                "RimMind.BridgeRimTalk.Settings.EnablePersonaPush.Desc".Translate());
            if (s.enablePersonaPush)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushPersonaToTraits".Translate(),
                    ref s.pushPersonaToTraits);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushPersonaToMood".Translate(),
                    ref s.pushPersonaToMood);
            }

            listing.End();
            Widgets.EndScrollView();

            SettingsUIHelper.DrawBottomBar(bottomBar, () =>
            {
                s.enableDialogueGate = true;
                s.skipChitchat = true;
                s.skipAutoDialogue = true;
                s.skipPlayerDialogue = true;
                s.forceRimMindPlayerDialogue = false;
                s.enableContextPush = true;
                s.pushPersonality = true;
                s.pushStoryteller = true;
                s.pushMemory = false;
                s.pushShaping = false;
                s.pushAdvisorLog = true;
                s.enableRimTalkHistoryPull = true;
                s.enablePersonaPush = false;
                s.pushPersonaToTraits = true;
                s.pushPersonaToMood = false;
            });

            Get().Write();
        }

        private static float EstimateHeight(BridgeRimTalkSettings s)
        {
            float h = 30f;
            h += 24f + 24f;
            if (s.enableDialogueGate)
                h += 24f * 3 + (s.skipPlayerDialogue ? 24f : 0f);
            h += 24f + 24f;
            if (s.enableContextPush)
                h += 24f * 6;
            h += 24f + 24f;
            if (s.enablePersonaPush)
                h += 24f * 2;
            return h + 40f;
        }
    }
}
