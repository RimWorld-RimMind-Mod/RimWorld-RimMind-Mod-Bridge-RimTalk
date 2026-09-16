using UnityEngine;
using Verse;
using RimMind.Presentation.UI;

namespace RimMind.Bridge.RimTalk.Settings
{
    public partial class BridgeRimTalkSettings : ModSettings
    {
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
            Scribe_Values.Look(ref injectPersonaToTraits, "injectPersonaToTraits", false);
            Scribe_Values.Look(ref injectPersonaToMood, "injectPersonaToMood", false);

            Scribe_Values.Look(ref enableContextPull, "enableContextPull", true);
            Scribe_Values.Look(ref pullRimTalkHistory, "pullRimTalkHistory", true);
        }

        private static Vector2 _scrollPos = Vector2.zero;
        public static void DrawSettingsContent(Rect inRect)
        {
            var s = Get();
            var snapshot = CaptureSnapshot();

            Rect contentArea = SettingsUIDrawer.SplitContentArea(inRect);
            Rect bottomBar = SettingsUIDrawer.SplitBottomBar(inRect);

            float contentH = EstimateHeight(s);
            Rect viewRect = new Rect(0f, 0f, contentArea.width - 16f, contentH);
            Widgets.BeginScrollView(contentArea, ref _scrollPos, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            SettingsUIDrawer.DrawSectionHeader(listing, "RimMind.BridgeRimTalk.Settings.Section.DialogueGate".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.EnableDialogueGate".Translate(),
                ref s.enableDialogueGate,
                "RimMind.BridgeRimTalk.Settings.EnableDialogueGate.Desc".Translate());
            if (s.enableDialogueGate)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.SkipChitchat".Translate(),
                    ref s.skipChitchat,
                    "RimMind.BridgeRimTalk.Settings.SkipChitchat.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.SkipAutoDialogue".Translate(),
                    ref s.skipAutoDialogue,
                    "RimMind.BridgeRimTalk.Settings.SkipAutoDialogue.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.SkipPlayerDialogue".Translate(),
                    ref s.skipPlayerDialogue,
                    "RimMind.BridgeRimTalk.Settings.SkipPlayerDialogue.Desc".Translate());
                if (s.skipPlayerDialogue)
                {
                    listing.CheckboxLabeled("    " + "RimMind.BridgeRimTalk.Settings.ForceRimMindPlayerDialogue".Translate(),
                        ref s.forceRimMindPlayerDialogue,
                        "RimMind.BridgeRimTalk.Settings.ForceRimMindPlayerDialogue.Desc".Translate());
                }
            }

            SettingsUIDrawer.DrawSectionHeader(listing, "RimMind.BridgeRimTalk.Settings.Section.ContextPush".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.EnableContextPush".Translate(),
                ref s.enableContextPush,
                "RimMind.BridgeRimTalk.Settings.EnableContextPush.Desc".Translate());
            if (s.enableContextPush)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushPersonality".Translate(),
                    ref s.pushPersonality,
                    "RimMind.BridgeRimTalk.Settings.PushPersonality.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushStoryteller".Translate(),
                    ref s.pushStoryteller,
                    "RimMind.BridgeRimTalk.Settings.PushStoryteller.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushMemory".Translate(),
                    ref s.pushMemory,
                    "RimMind.BridgeRimTalk.Settings.PushMemory.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushAdvisorLog".Translate(),
                    ref s.pushAdvisorLog,
                    "RimMind.BridgeRimTalk.Settings.PushAdvisorLog.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PushShaping".Translate(),
                    ref s.pushShaping,
                    "RimMind.BridgeRimTalk.Settings.PushShaping.Desc".Translate());
                if (s.pushPersonality)
                {
                    listing.CheckboxLabeled("    " + "RimMind.BridgeRimTalk.Settings.InjectPersonaToTraits".Translate(),
                        ref s.injectPersonaToTraits,
                        "RimMind.BridgeRimTalk.Settings.InjectPersonaToTraits.Desc".Translate());
                    listing.CheckboxLabeled("    " + "RimMind.BridgeRimTalk.Settings.InjectPersonaToMood".Translate(),
                        ref s.injectPersonaToMood,
                        "RimMind.BridgeRimTalk.Settings.InjectPersonaToMood.Desc".Translate());
                }
                GUI.color = Color.yellow;
                listing.Label("  " + "RimMind.BridgeRimTalk.Settings.RestartHint".Translate());
                GUI.color = Color.white;
            }

            SettingsUIDrawer.DrawSectionHeader(listing, "RimMind.BridgeRimTalk.Settings.Section.ContextPull".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimTalk.Settings.EnableContextPull".Translate(),
                ref s.enableContextPull,
                "RimMind.BridgeRimTalk.Settings.EnableContextPull.Desc".Translate());
            if (s.enableContextPull)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimTalk.Settings.PullRimTalkHistory".Translate(),
                    ref s.pullRimTalkHistory,
                    "RimMind.BridgeRimTalk.Settings.PullRimTalkHistory.Desc".Translate());
                GUI.color = Color.yellow;
                listing.Label("  " + "RimMind.BridgeRimTalk.Settings.RestartHint".Translate());
                GUI.color = Color.white;
            }

            listing.End();
            Widgets.EndScrollView();

            SettingsUIDrawer.DrawBottomBar(bottomBar, () =>
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
                s.injectPersonaToTraits = false;
                s.injectPersonaToMood = false;
                s.enableContextPull = true;
                s.pullRimTalkHistory = true;
            });

            MarkDirtyIfChanged(snapshot);
            if (_dirty)
            {
                Get().Write();
                _dirty = false;
            }
        }

        private static float EstimateHeight(BridgeRimTalkSettings s)
        {
            float h = 30f;
            h += 24f + 24f;
            if (s.enableDialogueGate)
                h += 24f * 3 + (s.skipPlayerDialogue ? 24f : 0f);
            h += 24f + 24f;
            if (s.enableContextPush)
            {
                h += 24f * 5;
                if (s.pushPersonality)
                    h += 24f * 2;
                h += 24f;
            }
            h += 24f + 24f;
            if (s.enableContextPull)
                h += 24f + 24f;
            return h + 40f;
        }
    }
}
