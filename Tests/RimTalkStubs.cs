using System;
using System.Collections.Generic;
using System.Linq;
using RimMind.Application.Common.Interfaces.Context;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Features.Bridges;
using RimMind.Domain.ValueObjects;

namespace Verse
{
    public static class Log
    {
        private static readonly HashSet<int> WarningKeys = new HashSet<int>();
        public static List<string> Warnings { get; } = new List<string>();

        public static void Warning(string msg) => Warnings.Add(msg);
        public static void WarningOnce(string msg, int key)
        {
            if (WarningKeys.Add(key))
                Warnings.Add(msg);
        }

        public static void Message(string msg) { }
        public static void Error(string msg) { }

        public static void Reset()
        {
            WarningKeys.Clear();
            Warnings.Clear();
        }
    }

    public static class ModsConfig
    {
        public static bool IsActive(string packageId) => false;
    }

    public sealed class Name
    {
        public Name(string shortName) => ToStringShort = shortName;
        public string ToStringShort { get; set; }
    }

    public class Pawn
    {
        public int thingIDNumber;
        public Name? Name;
        public string LabelShort = "Colonist";
        public Map? Map;
        public bool Dead;
        public bool Destroyed;
        public bool Spawned = true;
        public bool IsPrisoner;
    }

    public class Map
    {
        public List<Pawn> SpawnedColonists { get; } = new List<Pawn>();
    }

    public class TickManager
    {
        public int TicksGame;
    }

    public static class Find
    {
        public static TickManager? TickManager;
    }
}

namespace RimMind.Presentation.Api
{
    public static class RimMindAPI
    {
        public static class Bridges
        {
            private static readonly Action<string> DefaultLogWarn = msg =>
            {
                RimMindErrors.Warn(msg);
                Verse.Log.Warning(msg);
            };

            public static SoftDependencyDetector CreateDetector(
                string packageId,
                IReadOnlyList<string>? candidateTypeNames = null,
                int cacheIntervalTicks = SoftDependencyDetector.DefaultCacheIntervalTicks)
            {
                return new SoftDependencyDetector(
                    packageId,
                    candidateTypeNames,
                    Verse.ModsConfig.IsActive,
                    HarmonyLib.AccessTools.TypeByName,
                    GetCurrentTickSafe,
                    cacheIntervalTicks);
            }

            public static ReflectionAccessor CreateReflectionAccessor(
                string? logPrefix = null,
                Action<string>? warnCallback = null,
                Func<string, Type?>? resolveType = null)
            {
                return new ReflectionAccessor(
                    resolveType ?? HarmonyLib.AccessTools.TypeByName,
                    warnCallback ?? DefaultLogWarn,
                    logPrefix);
            }

            public static ExternalModBridgeCoordinator CreateCoordinator(
                Func<bool> isModAvailable,
                IReadOnlyList<IBridgeModule> modules,
                IReadOnlyList<ISkipCheck>? gates = null,
                Action<string>? logInfo = null,
                Action<string>? logWarn = null,
                string? inactiveMessage = null)
            {
                return new ExternalModBridgeCoordinator(
                    isModAvailable,
                    modules,
                    gates,
                    logInfo ?? Verse.Log.Message,
                    logWarn ?? DefaultLogWarn,
                    gate => Ext.Get<ISkipCheck>().Register(gate),
                    gateId => Ext.Get<ISkipCheck>().Unregister(gateId),
                    inactiveMessage);
            }

            private static int GetCurrentTickSafe()
            {
                try
                {
                    return Verse.Find.TickManager?.TicksGame ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static class Ext
        {
            private static readonly Dictionary<Type, object> Registries = new Dictionary<Type, object>();

            public static IExtensionRegistry<T> Get<T>() where T : class, IExtension
            {
                if (!Registries.TryGetValue(typeof(T), out var registry))
                {
                    registry = new StubExtensionRegistry<T>();
                    Registries[typeof(T)] = registry;
                }
                return (IExtensionRegistry<T>)registry;
            }

            public static void Reset() => Registries.Clear();
        }

        public static class Text
        {
            public static string TrOr(string key, string fallback, params object[] args)
            {
                string template = fallback ?? key ?? string.Empty;
                return args == null || args.Length == 0 ? template : string.Format(template, args);
            }
        }

        public sealed class StubExtensionRegistry<T> : IExtensionRegistry<T> where T : class, IExtension
        {
            private readonly List<T> _items = new List<T>();

            public void Register(T extension)
            {
                _items.RemoveAll(item => item.Id == extension.Id);
                _items.Add(extension);
            }

            public bool Unregister(string id) => _items.RemoveAll(item => item.Id == id) > 0;

            public int UnregisterByOwner(string ownerModId) => _items.RemoveAll(item => item.OwnerModId == ownerModId);

            public IReadOnlyList<T> All => _items;

            public T? FindById(string id) => _items.FirstOrDefault(item => item.Id == id);
        }

        public static class MainThreadSnapshot
        {
            public static RimMind.Application.Common.Threading.MainThreadReader Reader { get; set; } =
                new RimMind.Application.Common.Threading.MainThreadReader(a => a(), () => true);

            public static System.Threading.Tasks.Task<T> GetAsync<T>(
                Func<T> readOnMainThread,
                T fallback = default!,
                TimeSpan? timeout = null,
                System.Threading.CancellationToken ct = default)
                => Reader.ReadAsync(
                    readOnMainThread,
                    fallback,
                    timeout ?? RimMind.Application.Common.Threading.MainThreadReader.DefaultTimeout,
                    ct);
        }

        public static class Context
        {
            public static StubContextKeyRegistry ContextKeys { get; } = new StubContextKeyRegistry();
        }

        public sealed class StubContextKeyRegistry
        {
            public List<ContextProviderDef> Definitions { get; } = new List<ContextProviderDef>();

            public void Register(ContextProviderDef def)
            {
                Definitions.RemoveAll(d => d.Key == def.Key);
                Definitions.Add(def);
            }

            public bool Unregister(string key) => Definitions.RemoveAll(d => d.Key == key) > 0;

            public void Clear() => Definitions.Clear();
        }

        public static class Providers
        {
            private static readonly Dictionary<string, Result<string?, RimMindError>> PawnResults =
                new Dictionary<string, Result<string?, RimMindError>>();
            private static readonly Dictionary<string, Result<string?, RimMindError>> StaticResults =
                new Dictionary<string, Result<string?, RimMindError>>();

            public static List<string> Categories { get; } = new List<string>();
            public static List<PawnProviderCall> PawnCalls { get; } =
                new List<PawnProviderCall>();
            public static List<string> StaticCalls { get; } = new List<string>();

            public static Result<string?, RimMindError> GetProviderData(
                string category,
                Verse.Pawn pawn)
            {
                PawnCalls.Add(new PawnProviderCall(category, pawn));
                return PawnResults.TryGetValue(category, out var result)
                    ? result
                    : Missing();
            }

            public static Result<string?, RimMindError> GetStaticProviderData(
                string category)
            {
                StaticCalls.Add(category);
                return StaticResults.TryGetValue(category, out var result)
                    ? result
                    : Missing();
            }

            public static List<string> GetRegisteredCategories() =>
                new List<string>(Categories);

            public static void SetPawn(string category, string? value)
            {
                PawnResults[category] = Result<string?, RimMindError>.Ok(value);
            }

            public static void SetPawnError(string category, RimMindError error)
            {
                PawnResults[category] = Result<string?, RimMindError>.Err(error);
            }

            public static void SetStatic(string category, string? value)
            {
                StaticResults[category] = Result<string?, RimMindError>.Ok(value);
            }

            public static void ClearCalls()
            {
                PawnCalls.Clear();
                StaticCalls.Clear();
            }

            public static void Reset()
            {
                PawnResults.Clear();
                StaticResults.Clear();
                Categories.Clear();
                ClearCalls();
            }

            private static Result<string?, RimMindError> Missing() =>
                Result<string?, RimMindError>.Err(RimMindErrors.Internal("missing"));

            public sealed class PawnProviderCall
            {
                public PawnProviderCall(string category, Verse.Pawn pawn)
                {
                    Category = category;
                    Pawn = pawn;
                }

                public string Category { get; }
                public Verse.Pawn Pawn { get; }
            }
        }
    }

    public static class RimMindPawnLookup
    {
        public static List<Verse.Pawn> KnownPawns { get; } = new List<Verse.Pawn>();

        public static Verse.Pawn? FindPawnByNumber(int num)
        {
            if (num <= 0) return null;
            return KnownPawns.FirstOrDefault(p => p.thingIDNumber == num);
        }

        public static bool IsActiveSpawnedColonist(Verse.Pawn? pawn)
        {
            return pawn != null && !pawn.Dead && !pawn.Destroyed && pawn.Spawned && !pawn.IsPrisoner;
        }

        public static IReadOnlyList<Verse.Pawn> GetSpawnedColonistsSafe(Verse.Map? map)
        {
            if (map == null) return Array.Empty<Verse.Pawn>();
            return map.SpawnedColonists.Where(IsActiveSpawnedColonist).ToList();
        }
    }
}

namespace RimTalk.API
{
    using RimTalk.Prompt;
    using Verse;

    public static class FakeContextCategories
    {
        public static class Pawn
        {
            public static readonly string Traits = "Traits";
            public static readonly string Mood = "Mood";
        }
    }

    public static class FakeContextHookRegistry
    {
        public enum HookOperation
        {
            Append = 0,
            Prepend = 1,
            Override = 2
        }
    }

    public static class FakeRimTalkPromptAPI
    {
        public static List<PawnVariableRegistration> PawnVariables { get; } =
            new List<PawnVariableRegistration>();
        public static List<EnvironmentVariableRegistration> EnvironmentVariables { get; } =
            new List<EnvironmentVariableRegistration>();
        public static List<PawnHookRegistration> PawnHooks { get; } =
            new List<PawnHookRegistration>();
        public static List<FakePromptEntry> PromptEntries { get; } =
            new List<FakePromptEntry>();

        public static void RegisterPawnVariable(
            string modId,
            string name,
            Func<Pawn, string> provider,
            string? description,
            int priority)
        {
            PawnVariables.Add(new PawnVariableRegistration(
                modId,
                name,
                provider,
                description,
                priority));
        }

        public static void RegisterEnvironmentVariable(
            string modId,
            string name,
            Func<Map, string> provider,
            string? description,
            int priority)
        {
            EnvironmentVariables.Add(new EnvironmentVariableRegistration(
                modId,
                name,
                provider,
                description,
                priority));
        }

        public static void RegisterPawnHook(
            string modId,
            string category,
            FakeContextHookRegistry.HookOperation operation,
            Func<Pawn, string, string> handler,
            int priority)
        {
            PawnHooks.Add(new PawnHookRegistration(
                modId,
                category,
                operation,
                handler,
                priority));
        }

        public static FakePromptEntry CreatePromptEntry(
            string name,
            string content,
            FakePromptRole role,
            FakePromptPosition position,
            int inChatDepth,
            string? sourceModId)
        {
            return new FakePromptEntry(
                name,
                content,
                role,
                position,
                inChatDepth,
                sourceModId);
        }

        public static bool AddPromptEntry(FakePromptEntry entry)
        {
            PromptEntries.Add(entry);
            return true;
        }

        public static void UnregisterAllHooks(string modId)
        {
            PawnHooks.RemoveAll(hook => hook.ModId == modId);
        }

        public static int RemovePromptEntriesByModId(string modId)
        {
            return PromptEntries.RemoveAll(entry => entry.SourceModId == modId);
        }

        public static void Reset()
        {
            PawnVariables.Clear();
            EnvironmentVariables.Clear();
            PawnHooks.Clear();
            PromptEntries.Clear();
        }

        public sealed class PawnVariableRegistration
        {
            public PawnVariableRegistration(
                string modId,
                string name,
                Func<Pawn, string> provider,
                string? description,
                int priority)
            {
                ModId = modId;
                Name = name;
                Provider = provider;
                Description = description;
                Priority = priority;
            }

            public string ModId { get; }
            public string Name { get; }
            public Func<Pawn, string> Provider { get; }
            public string? Description { get; }
            public int Priority { get; }
        }

        public sealed class EnvironmentVariableRegistration
        {
            public EnvironmentVariableRegistration(
                string modId,
                string name,
                Func<Map, string> provider,
                string? description,
                int priority)
            {
                ModId = modId;
                Name = name;
                Provider = provider;
                Description = description;
                Priority = priority;
            }

            public string ModId { get; }
            public string Name { get; }
            public Func<Map, string> Provider { get; }
            public string? Description { get; }
            public int Priority { get; }
        }

        public sealed class PawnHookRegistration
        {
            public PawnHookRegistration(
                string modId,
                string category,
                FakeContextHookRegistry.HookOperation operation,
                Func<Pawn, string, string> handler,
                int priority)
            {
                ModId = modId;
                Category = category;
                Operation = operation;
                Handler = handler;
                Priority = priority;
            }

            public string ModId { get; }
            public string Category { get; }
            public FakeContextHookRegistry.HookOperation Operation { get; }
            public Func<Pawn, string, string> Handler { get; }
            public int Priority { get; }
        }
    }
}

namespace RimTalk.Data
{
    public enum FakeTalkRole
    {
        System = 0,
        User = 1,
        AI = 2
    }
}

namespace RimTalk.Prompt
{
    public enum FakePromptRole
    {
        System = 0,
        User = 1,
        Assistant = 2
    }

    public enum FakePromptPosition
    {
        Relative = 0,
        InChat = 1
    }

    public sealed class FakePromptEntry
    {
        public FakePromptEntry(
            string name,
            string content,
            FakePromptRole role,
            FakePromptPosition position,
            int inChatDepth,
            string? sourceModId)
        {
            Name = name;
            Content = content;
            Role = role;
            Position = position;
            InChatDepth = inChatDepth;
            SourceModId = sourceModId;
        }

        public string Name { get; }
        public string Content { get; }
        public FakePromptRole Role { get; }
        public FakePromptPosition Position { get; }
        public int InChatDepth { get; }
        public string? SourceModId { get; }
    }
}

namespace HarmonyLib
{
    public static class AccessTools
    {
        public static System.Type? TypeByName(string name) => null;
    }
}
