using RimMind.Presentation.Api;
using RimMind.Presentation.UI;
using UnityEngine;
using Verse;

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
        private static float _measuredHeight;

        public static void DrawSettingsContent(Rect inRect)
        {
            var s = Get();
            SettingsUIDrawer.DrawScrollableSettingsPage(
                inRect,
                ref _scrollPos,
                ref _measuredHeight,
                listing => DrawSettingsBody(listing, s),
                onReset: () => s.ApplyDefaults(),
                onDirtySave: () => s.Write());
        }

        private static void DrawSettingsBody(Listing_Standard listing, BridgeRimTalkSettings s)
        {
            SettingsUIDrawer.DrawSectionHeader(
                listing,
                RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.Section.DialogueGate", "Dialogue Gate"));
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.EnableDialogueGate", "Enable dialogue gate"),
                ref s.enableDialogueGate,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimTalk.Settings.EnableDialogueGate.Desc",
                    "When RimTalk is active, skip RimMind-Dialogue's redundant triggers to avoid duplicate conversations"));
            if (s.enableDialogueGate)
            {
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.SkipChitchat", "Skip chitchat triggers"),
                    ref s.skipChitchat,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.SkipChitchat.Desc",
                        "Skip RimMind's Chitchat dialogue triggers when RimTalk is active"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.SkipAutoDialogue", "Skip auto dialogue"),
                    ref s.skipAutoDialogue,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.SkipAutoDialogue.Desc",
                        "Skip RimMind's auto dialogue triggers when RimTalk is active"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.SkipPlayerDialogue", "Skip player dialogue"),
                    ref s.skipPlayerDialogue,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.SkipPlayerDialogue.Desc",
                        "Remove RimMind's \"Chat with X\" float menu option when RimTalk is active"));
                if (s.skipPlayerDialogue)
                {
                    listing.CheckboxLabeled(
                        "    " + RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimTalk.Settings.ForceRimMindPlayerDialogue",
                            "Keep RimMind player dialogue"),
                        ref s.forceRimMindPlayerDialogue,
                        RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimTalk.Settings.ForceRimMindPlayerDialogue.Desc",
                            "Keep RimMind's float menu option even when skipping player dialogue, allowing both chat systems to coexist"));
                }
            }

            SettingsUIDrawer.DrawSectionHeader(
                listing,
                RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.Section.ContextPush", "Context Push"));
            string restartHint = RimMindAPI.Text.TrOr(
                "RimMind.BridgeRimTalk.Settings.RestartHint",
                "Note: Changes to these settings require a game restart to take effect.");
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.EnableContextPush", "Enable context push"),
                ref s.enableContextPush,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimTalk.Settings.EnableContextPush.Desc",
                    "Push RimMind's data into RimTalk's prompt system as variables and prompt entries") + "\n\n" + restartHint);
            if (s.enableContextPush)
            {
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.PushPersonality", "Push personality data"),
                    ref s.pushPersonality,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.PushPersonality.Desc",
                        "Register RimMind personality profiles as RimTalk template variables (e.g. {{p.rimmind_personality}})"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.PushStoryteller", "Push storyteller state"),
                    ref s.pushStoryteller,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.PushStoryteller.Desc",
                        "Register storyteller narrative records as RimTalk environment variables"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.PushMemory", "Push memory data"),
                    ref s.pushMemory,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.PushMemory.Desc",
                        "Register colonist memory records as RimTalk pawn variables"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.PushAdvisorLog", "Push advisor log"),
                    ref s.pushAdvisorLog,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.PushAdvisorLog.Desc",
                        "Register advisor action history as RimTalk pawn variables"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.PushShaping", "Push shaping data"),
                    ref s.pushShaping,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.PushShaping.Desc",
                        "Register player shaping history as RimTalk pawn variables"));
                if (s.pushPersonality)
                {
                    listing.CheckboxLabeled(
                        "    " + RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimTalk.Settings.InjectPersonaToTraits",
                            "Inject into traits context"),
                        ref s.injectPersonaToTraits,
                        RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimTalk.Settings.InjectPersonaToTraits.Desc",
                            "Append personality description directly into RimTalk's Traits context section (may duplicate with personality variable)"));
                    listing.CheckboxLabeled(
                        "    " + RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimTalk.Settings.InjectPersonaToMood",
                            "Inject into mood context"),
                        ref s.injectPersonaToMood,
                        RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimTalk.Settings.InjectPersonaToMood.Desc",
                            "Append AI narrative directly into RimTalk's Mood context section"));
                }
            }

            SettingsUIDrawer.DrawSectionHeader(
                listing,
                RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.Section.ContextPull", "Context Pull"));
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.EnableContextPull", "Enable context pull"),
                ref s.enableContextPull,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimTalk.Settings.EnableContextPull.Desc",
                    "Pull dialogue history and other data from RimTalk, registering as RimMind context providers") + "\n\n" + restartHint);
            if (s.enableContextPull)
            {
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.PullRimTalkHistory",
                        "Pull RimTalk dialogue history"),
                    ref s.pullRimTalkHistory,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimTalk.Settings.PullRimTalkHistory.Desc",
                        "Register RimTalk's dialogue history as RimMind's context provider, so RimMind can read past RimTalk conversations") + "\n\n" + restartHint);
            }
        }
    }
}
