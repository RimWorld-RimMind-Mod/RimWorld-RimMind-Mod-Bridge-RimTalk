using System;
using LudeonTK;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Presentation.Api;
using RimWorld;
using Verse;

namespace RimMind.Bridge.RimTalk.Debug
{
    /// <summary>
    /// In-game behavioral autotest suite for RimMind-Bridge-RimTalk.
    /// Discovered automatically by Core's BehaviorAutotestRunner and exposed to Dev menu.
    /// </summary>
    public sealed class BridgeRimTalkBehaviorAutotestSuite : BehaviorAutotestSuiteBase
    {
        public override string ModId => "BridgeRimTalk";
        public override string SuiteId => "Behavior.BridgeRimTalk";

        [DebugAction("Autotests", "Run Bridge-RimTalk In-Game Behavior Test", actionType = DebugActionType.Action)]
        public static void RunFromDevMenu() => RunSuiteFromDevMenu<BridgeRimTalkBehaviorAutotestSuite>();

        public override void RunSuite(IInGameBehaviorSuiteContext context)
        {
            // 1. Detection Safety Check
            try
            {
                bool isActive = RimTalkDetector.IsRimTalkActive;
                bool isApiAvailable = RimTalkDetector.IsRimTalkApiAvailable;
                context.Assert(true, $"RimTalkDetector executed safely (Active: {isActive}, ApiAvailable: {isApiAvailable})");
            }
            catch (Exception ex)
            {
                context.Assert(false, $"RimTalkDetector threw exception: {ex.Message}");
            }

            // 2. DialogueGate Logic Verification
            Pawn? pawn = context.ActiveColonist;
            try
            {
                bool skipChitchat = DialogueGate.ShouldSkipDialogue(pawn, "Chitchat");
                bool skipAuto = DialogueGate.ShouldSkipDialogue(pawn, "Auto");
                bool skipPlayer = DialogueGate.ShouldSkipDialogue(pawn, "PlayerInput");
                bool skipMenu = DialogueGate.ShouldSkipFloatMenuOption();

                if (!RimTalkDetector.IsRimTalkActive)
                {
                    context.Assert(!skipChitchat && !skipAuto && !skipPlayer && !skipMenu, "DialogueGate correctly passes through all events when RimTalk mod is inactive");
                }
                else
                {
                    context.Assert(true, $"DialogueGate active evaluation completed (skipChitchat={skipChitchat}, skipMenu={skipMenu})");
                }
            }
            catch (Exception ex)
            {
                context.Assert(false, $"DialogueGate evaluation threw exception: {ex.Message}");
            }

            // 3. PersonaFormatter Text Construction
            try
            {
                string desc = "Helpful doctor";
                string work = "Medicine 14, passionate";
                string social = "Kind and calm";

                string formatted = PersonaFormatter.BuildFullProfile(desc, work, social);
                context.Assert(!string.IsNullOrEmpty(formatted), "PersonaFormatter generated non-empty persona profile");
                context.Assert(formatted.Contains(desc) && formatted.Contains("[Work]") && formatted.Contains("[Social]"), "PersonaFormatter correctly formatted description, work, and social sections");
            }
            catch (Exception ex)
            {
                context.Assert(false, $"PersonaFormatter threw exception: {ex.Message}");
            }

            // 4. Bridge Settings Accessibility
            var settings = BridgeRimTalkSettings.Get();
            context.Assert(settings != null, "BridgeRimTalkSettings accessible and loaded");
        }
    }
}
