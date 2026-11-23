using System;
using System.Collections.Concurrent;
using System.Reflection;
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

        private static Type? ApiType => Accessor.GetCachedType(ApiTypeName);
        private static Type? HookRegistryType => Accessor.GetCachedType(HookRegistryTypeName);
        private static Type? ContextCategoriesType => Accessor.GetCachedType(ContextCategoriesTypeName);
        private static Type? PromptEntryType => Accessor.GetCachedType(PromptEntryTypeName);
        private static Type? PromptRoleType => Accessor.GetCachedType(PromptRoleTypeName);
        private static Type? PromptPositionType => Accessor.GetCachedType(PromptPositionTypeName);

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
        }

        public static bool IsAvailable => RimTalkDetector.IsRimTalkApiAvailable;

        private static Type? GetCachedNestedType(Type parentType, string nestedName)
        {
            return NestedTypeCache.GetOrAdd(
                (parentType, nestedName),
                static key => key.Parent.GetNestedType(key.Name));
        }

        private static bool TryResolveEnumValue(Type enumType, string memberName, string warnKey, out object? value)
        {
            value = null;
            if (!enumType.IsEnum || string.IsNullOrWhiteSpace(memberName))
                return false;

            string[] names = Enum.GetNames(enumType);
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], memberName, StringComparison.OrdinalIgnoreCase))
                {
                    value = Enum.Parse(enumType, names[i]);
                    return true;
                }
            }

            Accessor.WarnOnce(
                warnKey,
                $"[RimMind-Bridge-RimTalk] Enum value '{memberName}' not found on {enumType.Name}.");
            return false;
        }

        public static bool RegisterPawnVariable(string modId, string variableName,
            Func<Pawn, string> provider, string? description = null, int priority = 100)
        {
            if (!IsAvailable) return false;
            var apiType = ApiType;
            if (apiType == null) return false;

            try
            {
                var method = Accessor.GetCachedMethod(
                    apiType,
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
            var apiType = ApiType;
            if (apiType == null) return false;

            try
            {
                var method = Accessor.GetCachedMethod(
                    apiType,
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
            string hookOperationName, Func<Pawn, string, string> handler, int priority = 100)
        {
            if (!IsAvailable) return false;
            var apiType = ApiType;
            var hookRegistryType = HookRegistryType;
            var contextCategoriesType = ContextCategoriesType;
            if (apiType == null || hookRegistryType == null || contextCategoriesType == null) return false;

            try
            {
                var nestedPawnType = GetCachedNestedType(contextCategoriesType, "Pawn");
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

                var hookOpEnum = GetCachedNestedType(hookRegistryType, "HookOperation");
                if (hookOpEnum == null)
                {
                    Accessor.WarnOnce(
                        "nested:ContextHookRegistry.HookOperation",
                        "[RimMind-Bridge-RimTalk] ContextHookRegistry.HookOperation nested type not found");
                    return false;
                }

                if (!TryResolveEnumValue(hookOpEnum, hookOperationName, $"enum:HookOperation:{hookOperationName}", out object? opValue))
                    return false;

                var method = Accessor.GetCachedMethod(
                    apiType,
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
            string roleName = "System", string positionName = "Relative",
            int inChatDepth = 0, string? sourceModId = null)
        {
            if (!IsAvailable) return false;
            var apiType = ApiType;
            var promptEntryType = PromptEntryType;
            var promptRoleType = PromptRoleType;
            var promptPositionType = PromptPositionType;
            if (apiType == null || promptEntryType == null || promptRoleType == null || promptPositionType == null)
                return false;

            try
            {
                var createMethod = Accessor.GetCachedMethod(
                    apiType,
                    "CreatePromptEntry",
                    BindingFlags.Public | BindingFlags.Static,
                    new[]
                    {
                        typeof(string),
                        typeof(string),
                        promptRoleType,
                        promptPositionType,
                        typeof(int),
                        typeof(string)
                    });

                if (createMethod == null)
                    return false;

                if (!TryResolveEnumValue(promptRoleType, roleName, $"enum:PromptRole:{roleName}", out object? roleObj))
                    return false;
                if (!TryResolveEnumValue(promptPositionType, positionName, $"enum:PromptPosition:{positionName}", out object? posObj))
                    return false;

                object? entry = createMethod.Invoke(null, new object?[] { name, content, roleObj, posObj, inChatDepth, sourceModId });
                if (entry == null) return false;

                var addMethod = Accessor.GetCachedMethod(
                    apiType,
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
            var apiType = ApiType;
            if (apiType == null) return false;

            try
            {
                var method = Accessor.GetCachedMethod(
                    apiType,
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
            var apiType = ApiType;
            if (apiType == null) return 0;

            try
            {
                var method = Accessor.GetCachedMethod(
                    apiType,
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
