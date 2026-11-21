using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimMind.Application.Features.Bridges;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimTalk.Bridge
{
    public static class RimTalkApiShim
    {
        private const string ApiTypeName = "RimTalk.API.RimTalkPromptAPI";
        private const string HookRegistryTypeName = "RimTalk.API.ContextHookRegistry";
        private const string ContextCategoriesTypeName = "RimTalk.API.ContextCategories";
        private const string PromptEntryTypeName = "RimTalk.Prompt.PromptEntry";
        private const string PromptRoleTypeName = "RimTalk.Prompt.PromptRole";
        private const string PromptPositionTypeName = "RimTalk.Prompt.PromptPosition";

        private static readonly Type[] RegisterPawnVariableSignature =
            { typeof(string), typeof(string), typeof(Func<Pawn, string>), typeof(string), typeof(int) };

        private static readonly Type[] RegisterEnvironmentVariableSignature =
            { typeof(string), typeof(string), typeof(Func<Map, string>), typeof(string), typeof(int) };

        internal static readonly ReflectionAccessor Accessor =
            RimMindAPI.Bridges.CreateReflectionAccessor("[RimMind-Bridge-RimTalk]");

        private static readonly ConcurrentDictionary<(Type Parent, string Name), Type?> NestedTypeCache =
            new ConcurrentDictionary<(Type Parent, string Name), Type?>();

        private static Type? _apiType;
        private static Type? _hookRegistryType;
        private static Type? _contextCategoriesType;
        private static Type? _promptEntryType;
        private static Type? _promptRoleType;
        private static Type? _promptPositionType;
        private static bool _resolved;

        internal static void ConfigureTypesForTesting(
            Type? apiType,
            Type? hookRegistryType = null,
            Type? contextCategoriesType = null,
            Type? promptEntryType = null,
            Type? promptRoleType = null,
            Type? promptPositionType = null)
        {
            Accessor.ResetForTesting();
            NestedTypeCache.Clear();

            Accessor.SetCachedType(ApiTypeName, apiType);
            Accessor.SetCachedType(HookRegistryTypeName, hookRegistryType);
            Accessor.SetCachedType(ContextCategoriesTypeName, contextCategoriesType);
            Accessor.SetCachedType(PromptEntryTypeName, promptEntryType);
            Accessor.SetCachedType(PromptRoleTypeName, promptRoleType);
            Accessor.SetCachedType(PromptPositionTypeName, promptPositionType);

            _apiType = apiType;
            _hookRegistryType = hookRegistryType;
            _contextCategoriesType = contextCategoriesType;
            _promptEntryType = promptEntryType;
            _promptRoleType = promptRoleType;
            _promptPositionType = promptPositionType;
            _resolved = true;
        }

        public static bool IsAvailable => RimTalkDetector.IsRimTalkApiAvailable;

        private static void EnsureResolved()
        {
            if (_resolved) return;
            _resolved = true;
            ResolveTypes();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ResolveTypes()
        {
            try
            {
                _apiType = Accessor.GetCachedType(ApiTypeName);
                _hookRegistryType = Accessor.GetCachedType(HookRegistryTypeName);
                _contextCategoriesType = Accessor.GetCachedType(ContextCategoriesTypeName);
                _promptEntryType = Accessor.GetCachedType(PromptEntryTypeName);
                _promptRoleType = Accessor.GetCachedType(PromptRoleTypeName);
                _promptPositionType = Accessor.GetCachedType(PromptPositionTypeName);
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] Failed to resolve RimTalk types: {ex.Message}");
            }
        }

        private static Type? GetCachedNestedType(Type parentType, string nestedName)
        {
            return NestedTypeCache.GetOrAdd(
                (parentType, nestedName),
                static key => key.Parent.GetNestedType(key.Name));
        }

        public static bool RegisterPawnVariable(string modId, string variableName,
            Func<Pawn, string> provider, string? description = null, int priority = 100)
        {
            if (!IsAvailable) return false;
            EnsureResolved();
            if (_apiType == null) return false;

            try
            {
                var method = Accessor.GetCachedMethod(
                    _apiType,
                    "RegisterPawnVariable",
                    BindingFlags.Public | BindingFlags.Static,
                    RegisterPawnVariableSignature);

                if (method == null)
                    return false;

                method.Invoke(null, new object?[] { modId, variableName, provider, description, priority });
                return true;
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] RegisterPawnVariable failed: {ex.Message}");
                return false;
            }
        }

        public static bool RegisterEnvironmentVariable(string modId, string variableName,
            Func<Map, string> provider, string? description = null, int priority = 100)
        {
            if (!IsAvailable) return false;
            EnsureResolved();
            if (_apiType == null) return false;

            try
            {
                var method = Accessor.GetCachedMethod(
                    _apiType,
                    "RegisterEnvironmentVariable",
                    BindingFlags.Public | BindingFlags.Static,
                    RegisterEnvironmentVariableSignature);

                if (method == null)
                    return false;

                method.Invoke(null, new object?[] { modId, variableName, provider, description, priority });
                return true;
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] RegisterEnvironmentVariable failed: {ex.Message}");
                return false;
            }
        }

        public static bool RegisterPawnHook(string modId, string categoryKey,
            int hookOperation, Func<Pawn, string, string> handler, int priority = 100)
        {
            if (!IsAvailable) return false;
            EnsureResolved();
            if (_apiType == null || _hookRegistryType == null || _contextCategoriesType == null) return false;

            try
            {
                var nestedPawnType = GetCachedNestedType(_contextCategoriesType, "Pawn");
                if (nestedPawnType == null)
                {
                    Accessor.WarnOnce(
                        "nested:ContextCategories.Pawn",
                        "[RimMind-Bridge-RimTalk] ContextCategories.Pawn nested type not found");
                    return false;
                }

                var categoryField = Accessor.GetCachedField(
                    nestedPawnType,
                    categoryKey,
                    BindingFlags.Public | BindingFlags.Static);
                if (categoryField == null)
                    return false;

                object? categoryValue = categoryField.GetValue(null);
                if (categoryValue == null) return false;

                var hookOpEnum = GetCachedNestedType(_hookRegistryType, "HookOperation");
                if (hookOpEnum == null) return false;

                object? opValue = Enum.ToObject(hookOpEnum, hookOperation);

                var method = Accessor.GetCachedMethod(
                    _apiType,
                    "RegisterPawnHook",
                    BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                    return false;

                method.Invoke(null, new object?[] { modId, categoryValue, opValue, handler, priority });
                return true;
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] RegisterPawnHook failed: {ex.Message}");
                return false;
            }
        }

        public static bool AddPromptEntry(string name, string content,
            int roleValue = 0, int positionValue = 0,
            int inChatDepth = 0, string? sourceModId = null)
        {
            if (!IsAvailable) return false;
            EnsureResolved();
            if (_apiType == null || _promptEntryType == null) return false;

            try
            {
                var createMethod = Accessor.GetCachedMethod(
                    _apiType,
                    "CreatePromptEntry",
                    BindingFlags.Public | BindingFlags.Static,
                    new[]
                    {
                        typeof(string),
                        typeof(string),
                        _promptRoleType ?? typeof(int),
                        _promptPositionType ?? typeof(int),
                        typeof(int),
                        typeof(string)
                    });

                if (createMethod == null)
                    return false;

                object? roleObj = _promptRoleType != null ? Enum.ToObject(_promptRoleType, roleValue) : roleValue;
                object? posObj = _promptPositionType != null ? Enum.ToObject(_promptPositionType, positionValue) : positionValue;

                object? entry = createMethod.Invoke(null, new object?[] { name, content, roleObj, posObj, inChatDepth, sourceModId });
                if (entry == null) return false;

                var addMethod = Accessor.GetCachedMethod(
                    _apiType,
                    "AddPromptEntry",
                    BindingFlags.Public | BindingFlags.Static);
                if (addMethod == null) return false;

                object? result = addMethod.Invoke(null, new object?[] { entry });
                return result is bool b && b;
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] AddPromptEntry failed: {ex.Message}");
                return false;
            }
        }

        public static bool UnregisterAllHooks(string modId)
        {
            if (!IsAvailable) return false;
            EnsureResolved();
            if (_apiType == null) return false;

            try
            {
                var method = Accessor.GetCachedMethod(
                    _apiType,
                    "UnregisterAllHooks",
                    BindingFlags.Public | BindingFlags.Static,
                    warnIfMissing: false);
                if (method == null) return false;

                method.Invoke(null, new object?[] { modId });
                return true;
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] UnregisterAllHooks failed: {ex.Message}");
                return false;
            }
        }

        public static int RemovePromptEntriesByModId(string modId)
        {
            if (!IsAvailable) return 0;
            EnsureResolved();
            if (_apiType == null) return 0;

            try
            {
                var method = Accessor.GetCachedMethod(
                    _apiType,
                    "RemovePromptEntriesByModId",
                    BindingFlags.Public | BindingFlags.Static,
                    warnIfMissing: false);
                if (method == null) return 0;

                object? result = method.Invoke(null, new object?[] { modId });
                return result is int i ? i : 0;
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimTalk] RemovePromptEntriesByModId failed: {ex.Message}");
                return 0;
            }
        }

        public static void Cleanup(string modId)
        {
            UnregisterAllHooks(modId);
            RemovePromptEntriesByModId(modId);
        }
    }
}
