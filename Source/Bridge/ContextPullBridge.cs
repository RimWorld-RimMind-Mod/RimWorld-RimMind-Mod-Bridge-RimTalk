using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Core;
using RimMind.Core.Prompt;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class ContextPullBridge
    {
        private const string ModId = "RimMind.BridgeRimTalk";

        private static System.Type? _talkHistoryType;
        private static MethodInfo? _getMessageHistoryMethod;
        private static bool _typeResolved;

        public static void Register()
        {
            if (!RimTalkDetector.IsRimTalkActive) return;
            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enableContextPull) return;

            if (settings.pullRimTalkHistory)
                RegisterRimTalkHistoryProvider();
        }

        private static void RegisterRimTalkHistoryProvider()
        {
            if (!ResolveTypes())
            {
                Log.Warning("[RimMind-Bridge-RimTalk] RimTalk history types not available, skipping provider registration.");
                return;
            }
            RimMindAPI.RegisterPawnContextProvider("rimtalk_history", pawn =>
            {
                return BuildRimTalkHistoryContext(pawn);
            }, PromptSection.PriorityMemory, ModId);
        }

        private static bool ResolveTypes()
        {
            if (_typeResolved) return _talkHistoryType != null;
            _typeResolved = true;

            _talkHistoryType = AccessTools.TypeByName("RimTalk.Data.TalkHistory");
            if (_talkHistoryType == null)
            {
                Log.WarningOnce("[RimMind-Bridge-RimTalk] RimTalk.Data.TalkHistory type not found.", 84231);
                return false;
            }

            _getMessageHistoryMethod = _talkHistoryType.GetMethod("GetMessageHistory",
                BindingFlags.Public | BindingFlags.Static);
            if (_getMessageHistoryMethod == null)
            {
                Log.WarningOnce("[RimMind-Bridge-RimTalk] TalkHistory.GetMessageHistory method not found.", 84232);
                return false;
            }

            return true;
        }

        private static string? BuildRimTalkHistoryContext(Pawn pawn)
        {
            try
            {
                if (!ResolveTypes()) return null;

                string pawnName = pawn.Name.ToStringShort;
                var mapPawns = pawn.Map?.mapPawns;
                if (mapPawns == null) return null;

                var relevantMessages = new List<(string roleLabel, string content)>();

                foreach (var otherPawn in mapPawns.FreeColonists)
                {
                    var messages = _getMessageHistoryMethod!.Invoke(null,
                        new object?[] { otherPawn, true }) as IList;
                    if (messages == null || messages.Count == 0) continue;

                    foreach (var msg in messages)
                    {
                        var msgType = msg.GetType();
                        var roleField = msgType.GetField("Item1",
                            BindingFlags.Public | BindingFlags.Instance);
                        var messageField = msgType.GetField("Item2",
                            BindingFlags.Public | BindingFlags.Instance);
                        if (roleField == null || messageField == null)
                        {
                            Log.WarningOnce("ContextPullBridge: RimTalk message tuple fields not found, messages may not be pulled correctly", 84233);
                            continue;
                        }

                        var roleValue = roleField.GetValue(msg);
                        var content = messageField.GetValue(msg)?.ToString() ?? "";
                        if (string.IsNullOrEmpty(content)) continue;

                        bool isRelevant = otherPawn == pawn
                            || (pawnName.Length >= 3 && content.Contains(pawnName))
                            || content.Contains($"[{pawnName}]")
                            || content.Contains($"{pawnName}:")
                            || content.Contains($"{pawnName},");

                        if (!isRelevant) continue;

                        string roleLabel = roleValue?.ToString() ?? "?";
                        if (roleValue is int roleInt)
                        {
                            roleLabel = roleInt switch
                            {
                                0 => "System",
                                1 => "User",
                                2 => "AI",
                                _ => roleInt.ToString()
                            };
                        }

                        relevantMessages.Add((roleLabel, content));
                    }
                }

                if (relevantMessages.Count == 0) return null;

                int skip = System.Math.Max(0, relevantMessages.Count - 6);
                var sb = new StringBuilder("[RimTalk Dialog History]");
                for (int i = skip; i < relevantMessages.Count; i++)
                {
                    var m = relevantMessages[i];
                    sb.AppendLine($"[{m.roleLabel}] {m.content}");
                }
                return sb.ToString().TrimEnd();
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] BuildRimTalkHistoryContext failed for {pawn.Name.ToStringShort}: {ex.Message}");
                return null;
            }
        }

        public static void Unregister()
        {
            RimMindAPI.UnregisterPawnContextProvider("rimtalk_history");
        }
    }
}
