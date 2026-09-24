using System;
using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
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

    public class Pawn { }

    public class Map { }

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
            InsertBefore = 0
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

namespace RimTalk.Prompt
{
    public enum FakePromptRole
    {
        System = 0
    }

    public enum FakePromptPosition
    {
        BeforeHistory = 0
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
