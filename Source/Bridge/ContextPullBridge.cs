using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Application.Common.Interfaces.Context;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public sealed class ContextPullBridge : IBridgeModule
    {
        private const string ModId = "RimMind.BridgeRimTalk";
        private const int MaxHistoryMessages = 6;

        // Type-resolution cache is write-once and shared across instances (safe).
        private static Type? _talkHistoryType;
        private static MethodInfo? _getMessageHistoryMethod;
        private static bool _typeResolved;
        private static bool _disabledDueToStructuralFailure;
        private static bool _warnedMissingTupleFields;

        private static readonly ConcurrentDictionary<Type, MessageAccessors> _accessorCache =
            new ConcurrentDictionary<Type, MessageAccessors>();

        public string Id => "context_pull";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public bool IsRegistered { get; private set; }

        internal static void ConfigureTypesForTesting(Type? talkHistoryType)
        {
            _talkHistoryType = talkHistoryType;
            _getMessageHistoryMethod = talkHistoryType?.GetMethod(
                "GetMessageHistory",
                BindingFlags.Public | BindingFlags.Static);
            _typeResolved = true;
            _disabledDueToStructuralFailure = false;
            _warnedMissingTupleFields = false;
            _accessorCache.Clear();
        }

        public void Register()
        {
            if (IsRegistered) return;
            if (!RimTalkDetector.IsRimTalkActive) return;
            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enableContextPull) return;

            _disabledDueToStructuralFailure = false;
            _warnedMissingTupleFields = false;

            if (settings.pullRimTalkHistory)
                RegisterRimTalkHistoryProvider();

            IsRegistered = true;
        }

        private void RegisterRimTalkHistoryProvider()
        {
            if (!ResolveTypes())
            {
                RimMindErrors.Warn("[RimMind-Bridge-RimTalk] RimTalk history types not available, skipping provider registration.");
                return;
            }
            RimMindAPI.Context.ContextKeys.Register(new ContextProviderDef(
                "rimtalk_history", ContextLayer.L4_History, 0.5f,
                async (ctx, ct) =>
                {
                    if (ctx.PawnId <= 0 || _disabledDueToStructuralFailure) return null;
                    return await RimMindAPI.MainThreadSnapshot.GetAsync<string?>(() =>
                    {
                        var pawn = RimMindPawnLookup.FindPawnByNumber(ctx.PawnId);
                        if (!RimMindPawnLookup.IsActiveSpawnedColonist(pawn)) return null;
                        return BuildRimTalkHistoryContext(pawn!);
                    }, null, ct: ct).ConfigureAwait(false);
                }, ModId, stalenessTicks: 3000, invalidationTriggers: new[] { "RimTalkEvent" }));
        }

        private bool ResolveTypes()
        {
            if (_typeResolved) return _talkHistoryType != null && _getMessageHistoryMethod != null;
            _typeResolved = true;

            _talkHistoryType = AccessTools.TypeByName("RimTalk.Data.TalkHistory");
            if (_talkHistoryType == null)
            {
                RimMindErrors.Warn("[RimMind-Bridge-RimTalk] RimTalk.Data.TalkHistory type not found.");
                return false;
            }

            _getMessageHistoryMethod = _talkHistoryType.GetMethod("GetMessageHistory",
                BindingFlags.Public | BindingFlags.Static);
            if (_getMessageHistoryMethod == null)
            {
                RimMindErrors.Warn("[RimMind-Bridge-RimTalk] TalkHistory.GetMessageHistory method not found.");
                return false;
            }

            return true;
        }

        private string? BuildRimTalkHistoryContext(Pawn pawn)
        {
            try
            {
                if (_disabledDueToStructuralFailure || !ResolveTypes()) return null;

                string pawnName = pawn.Name?.ToStringShort ?? pawn.LabelShort ?? string.Empty;
                if (pawn.Map == null) return null;
                var spawnedColonists = RimMindPawnLookup.GetSpawnedColonistsSafe(pawn.Map);
                if (spawnedColonists.Count == 0) return null;

                var candidates = new List<RelevantMessageCandidate>();

                for (int cIdx = 0; cIdx < spawnedColonists.Count; cIdx++)
                {
                    var otherPawn = spawnedColonists[cIdx];
                    if (otherPawn == null) continue;

                    var messages = _getMessageHistoryMethod!.Invoke(null,
                        new object?[] { otherPawn, true }) as IList;
                    if (messages == null || messages.Count == 0) continue;

                    bool isSelf = ReferenceEquals(otherPawn, pawn) || otherPawn.thingIDNumber == pawn.thingIDNumber;
                    int distanceFromTail = 0;

                    for (int mIdx = messages.Count - 1; mIdx >= 0; mIdx--)
                    {
                        var msg = messages[mIdx];
                        if (msg == null) continue;

                        var accessors = _accessorCache.GetOrAdd(msg.GetType(), static t => MessageAccessors.Resolve(t));
                        if (!accessors.IsValid)
                        {
                            if (!_warnedMissingTupleFields)
                            {
                                _warnedMissingTupleFields = true;
                                const string warnMsg = "ContextPullBridge: RimTalk message tuple fields not found; disabling RimTalk history pull.";
                                RimMindErrors.Warn(warnMsg);
                                Log.Warning(warnMsg);
                            }
                            _disabledDueToStructuralFailure = true;
                            return null;
                        }

                        var roleValue = accessors.GetRole(msg);
                        var content = accessors.GetMessage(msg)?.ToString() ?? "";
                        if (string.IsNullOrEmpty(content)) continue;

                        bool isRelevant = isSelf
                            || (pawnName.Length >= 3 && content.Contains(pawnName))
                            || (!string.IsNullOrEmpty(pawnName) && (
                                content.Contains($"[{pawnName}]")
                                || content.Contains($"{pawnName}:")
                                || content.Contains($"{pawnName},")));

                        if (!isRelevant) continue;

                        string roleLabel = FormatRoleLabel(roleValue);
                        long? tick = accessors.TryGetTick(msg);

                        candidates.Add(new RelevantMessageCandidate(
                            roleLabel,
                            content,
                            tick,
                            distanceFromTail,
                            isSelf,
                            cIdx,
                            mIdx));

                        distanceFromTail++;
                        if (distanceFromTail >= MaxHistoryMessages)
                            break;
                    }
                }

                if (candidates.Count == 0) return null;

                var selected = candidates
                    .OrderBy(c => c.Tick.HasValue ? 0 : 1)
                    .ThenByDescending(c => c.Tick ?? long.MinValue)
                    .ThenBy(c => c.DistanceFromTail)
                    .ThenBy(c => c.IsSelf ? 0 : 1)
                    .ThenBy(c => c.ColonistOrder)
                    .Take(MaxHistoryMessages)
                    .OrderBy(c => c.Tick.HasValue ? 0 : 1)
                    .ThenBy(c => c.Tick ?? long.MaxValue)
                    .ThenByDescending(c => c.DistanceFromTail)
                    .ThenBy(c => c.IsSelf ? 0 : 1)
                    .ThenBy(c => c.ColonistOrder)
                    .ThenBy(c => c.MessageIndex)
                    .ToList();

                var sb = new StringBuilder();
                sb.AppendLine("[RimTalk Dialog History]");
                for (int i = 0; i < selected.Count; i++)
                {
                    var m = selected[i];
                    sb.AppendLine($"[{m.RoleLabel}] {m.Content}");
                }
                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                string fallbackName = pawn.Name?.ToStringShort ?? pawn.LabelShort ?? "?";
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] BuildRimTalkHistoryContext failed for {fallbackName}: {ex.Message}");
                return null;
            }
        }

        private static string FormatRoleLabel(object? roleValue)
        {
            if (roleValue is int roleInt)
            {
                return roleInt switch
                {
                    0 => "System",
                    1 => "User",
                    2 => "AI",
                    _ => roleInt.ToString()
                };
            }
            return roleValue?.ToString() ?? "?";
        }

        public void Unregister()
        {
            if (!IsRegistered) return;
            RimMindAPI.Context.ContextKeys.Unregister("rimtalk_history");
            IsRegistered = false;
        }

        private readonly struct RelevantMessageCandidate
        {
            public RelevantMessageCandidate(
                string roleLabel,
                string content,
                long? tick,
                int distanceFromTail,
                bool isSelf,
                int colonistOrder,
                int messageIndex)
            {
                RoleLabel = roleLabel;
                Content = content;
                Tick = tick;
                DistanceFromTail = distanceFromTail;
                IsSelf = isSelf;
                ColonistOrder = colonistOrder;
                MessageIndex = messageIndex;
            }

            public string RoleLabel { get; }
            public string Content { get; }
            public long? Tick { get; }
            public int DistanceFromTail { get; }
            public bool IsSelf { get; }
            public int ColonistOrder { get; }
            public int MessageIndex { get; }
        }

        private sealed class MessageAccessors
        {
            private readonly FieldInfo? _roleField;
            private readonly PropertyInfo? _roleProp;
            private readonly FieldInfo? _msgField;
            private readonly PropertyInfo? _msgProp;
            private readonly FieldInfo? _tickField;
            private readonly PropertyInfo? _tickProp;

            private MessageAccessors(
                FieldInfo? roleField,
                PropertyInfo? roleProp,
                FieldInfo? msgField,
                PropertyInfo? msgProp,
                FieldInfo? tickField,
                PropertyInfo? tickProp)
            {
                _roleField = roleField;
                _roleProp = roleProp;
                _msgField = msgField;
                _msgProp = msgProp;
                _tickField = tickField;
                _tickProp = tickProp;
            }

            public bool IsValid => (_roleField != null || _roleProp != null) && (_msgField != null || _msgProp != null);

            public static MessageAccessors Resolve(Type type)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                var roleField = type.GetField("Item1", flags);
                var roleProp = roleField == null ? type.GetProperty("Item1", flags) : null;
                var msgField = type.GetField("Item2", flags);
                var msgProp = msgField == null ? type.GetProperty("Item2", flags) : null;

                var tickField = type.GetField("Item3", flags)
                    ?? type.GetField("Tick", flags)
                    ?? type.GetField("tick", flags)
                    ?? type.GetField("Timestamp", flags);
                var tickProp = tickField == null
                    ? (type.GetProperty("Item3", flags)
                        ?? type.GetProperty("Tick", flags)
                        ?? type.GetProperty("Timestamp", flags))
                    : null;

                return new MessageAccessors(roleField, roleProp, msgField, msgProp, tickField, tickProp);
            }

            public object? GetRole(object instance)
                => _roleField != null ? _roleField.GetValue(instance) : _roleProp?.GetValue(instance);

            public object? GetMessage(object instance)
                => _msgField != null ? _msgField.GetValue(instance) : _msgProp?.GetValue(instance);

            public long? TryGetTick(object instance)
            {
                object? raw = _tickField != null ? _tickField.GetValue(instance) : _tickProp?.GetValue(instance);
                if (raw is int i) return i;
                if (raw is long l) return l;
                return null;
            }
        }
    }
}
