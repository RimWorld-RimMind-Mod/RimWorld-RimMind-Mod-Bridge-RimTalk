using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimMind.Application.Common.Interfaces.Context;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Features.Bridges;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public sealed class ContextPullBridge : IBridgeModule
    {
        private const string ModId = "RimMindBridgeRimTalk";
        private const string TalkHistoryTypeName = "RimTalk.Data.TalkHistory";
        private const string GetMessageHistoryMethodName = "GetMessageHistory";
        private const int MaxHistoryMessages = 6;

        internal static readonly ReflectionAccessor Accessor =
            RimMindAPI.Bridges.CreateReflectionAccessor("[RimMind-Bridge-RimTalk]");

        private static readonly ConcurrentDictionary<Type, MessageAccessors> _accessorCache =
            new ConcurrentDictionary<Type, MessageAccessors>();

        public string Id => "context_pull";
        public string OwnerModId => ModId;
        public bool IsRegistered { get; private set; }

        internal static void ConfigureTypesForTesting(Type? talkHistoryType)
        {
            Accessor.ResetForTesting();
            Accessor.SetCachedType(TalkHistoryTypeName, talkHistoryType);
            _accessorCache.Clear();
        }

        public void Register()
        {
            if (IsRegistered) return;
            if (!RimTalkDetector.IsRimTalkActive) return;
            var settings = BridgeRimTalkSettings.Get();
            if (!settings.enableContextPull || !settings.pullRimTalkHistory) return;

            Accessor.ResetStructuralFailure();

            if (RegisterRimTalkHistoryProvider())
                IsRegistered = true;
        }

        private bool RegisterRimTalkHistoryProvider()
        {
            if (ResolveGetMessageHistoryMethod() == null)
            {
                Accessor.WarnOnce(
                    "history_unavailable",
                    "[RimMind-Bridge-RimTalk] RimTalk history types not available, skipping provider registration.");
                return false;
            }
            RimMindAPI.Context.ContextKeys.Register(new ContextProviderDef(
                "rimtalk_history", ContextLayer.L4_History, 0.5f,
                async (ctx, ct) =>
                {
                    if (ctx.PawnId <= 0 || Accessor.IsStructurallyDisabled) return null;
                    return await RimMindAPI.MainThreadSnapshot.GetAsync<string?>(() =>
                    {
                        var pawn = RimMindPawnLookup.FindPawnByNumber(ctx.PawnId);
                        if (!RimMindPawnLookup.IsActiveSpawnedColonist(pawn)) return null;
                        return BuildRimTalkHistoryContext(pawn!);
                    }, null, ct: ct).ConfigureAwait(false);
                }, ModId, stalenessTicks: 3000)); // RimTalk history changes are external (no bus event): staleness only
            return true;
        }

        private static MethodInfo? ResolveGetMessageHistoryMethod()
        {
            var talkHistoryType = Accessor.GetCachedType(TalkHistoryTypeName, warnIfMissing: true);
            if (talkHistoryType == null) return null;

            return Accessor.GetCachedMethod(
                talkHistoryType,
                GetMessageHistoryMethodName,
                BindingFlags.Public | BindingFlags.Static,
                warnIfMissing: true);
        }

        private string? BuildRimTalkHistoryContext(Pawn pawn)
        {
            try
            {
                if (Accessor.IsStructurallyDisabled) return null;
                var getMessageHistoryMethod = ResolveGetMessageHistoryMethod();
                if (getMessageHistoryMethod == null) return null;

                string pawnName = pawn.Name?.ToStringShort ?? pawn.LabelShort ?? string.Empty;
                if (pawn.Map == null) return null;
                var spawnedColonists = RimMindPawnLookup.GetSpawnedColonistsSafe(pawn.Map);
                if (spawnedColonists.Count == 0) return null;

                var candidates = new List<RelevantMessageCandidate>();

                for (int cIdx = 0; cIdx < spawnedColonists.Count; cIdx++)
                {
                    var otherPawn = spawnedColonists[cIdx];
                    if (otherPawn == null) continue;

                    var messages = getMessageHistoryMethod.Invoke(null,
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
                            Accessor.MarkStructuralFailure(
                                "ContextPullBridge: RimTalk message tuple fields not found; disabling RimTalk history pull.");
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
            string? label = roleValue?.ToString();
            return string.IsNullOrWhiteSpace(label) ? "?" : label!;
        }

        public void Unregister()
        {
            if (!IsRegistered) return;
            RimMindAPI.Context.ContextKeys.Unregister("rimtalk_history");
            Accessor.ResetStructuralFailure();
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
                var roleField = Accessor.GetCachedField(type, "Item1", flags, warnIfMissing: false);
                var roleProp = roleField == null ? Accessor.GetCachedProperty(type, "Item1", flags, warnIfMissing: false) : null;
                var msgField = Accessor.GetCachedField(type, "Item2", flags, warnIfMissing: false);
                var msgProp = msgField == null ? Accessor.GetCachedProperty(type, "Item2", flags, warnIfMissing: false) : null;

                var tickField = Accessor.GetCachedField(type, "Item3", flags, warnIfMissing: false)
                    ?? Accessor.GetCachedField(type, "Tick", flags, warnIfMissing: false)
                    ?? Accessor.GetCachedField(type, "tick", flags, warnIfMissing: false)
                    ?? Accessor.GetCachedField(type, "Timestamp", flags, warnIfMissing: false);
                var tickProp = tickField == null
                    ? (Accessor.GetCachedProperty(type, "Item3", flags, warnIfMissing: false)
                        ?? Accessor.GetCachedProperty(type, "Tick", flags, warnIfMissing: false)
                        ?? Accessor.GetCachedProperty(type, "Timestamp", flags, warnIfMissing: false))
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
