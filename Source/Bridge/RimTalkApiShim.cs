using System;
using System.Reflection;
using HarmonyLib;
using RimMind.Bridge.RimTalk.Detection;
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

        private static Type? _apiType;
        private static Type? _hookRegistryType;
        private static Type? _contextCategoriesType;
        private static Type? _promptEntryType;
        private static Type? _promptRoleType;
        private static Type? _promptPositionType;
        private static bool _resolved;

        public static bool IsAvailable => RimTalkDetector.IsRimTalkApiAvailable;

        private static void EnsureResolved()
        {
            if (_resolved) return;
            _resolved = true;

            try
            {
                _apiType = AccessTools.TypeByName(ApiTypeName);
                _hookRegistryType = AccessTools.TypeByName(HookRegistryTypeName);
                _contextCategoriesType = AccessTools.TypeByName(ContextCategoriesTypeName);
                _promptEntryType = AccessTools.TypeByName(PromptEntryTypeName);
                _promptRoleType = AccessTools.TypeByName(PromptRoleTypeName);
                _promptPositionType = AccessTools.TypeByName(PromptPositionTypeName);
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] Failed to resolve RimTalk types: {ex.Message}");
            }
        }

        public static bool RegisterPawnVariable(string modId, string variableName,
            Func<Pawn, string> provider, string? description = null, int priority = 100)
        {
            if (!IsAvailable) return false;
            EnsureResolved();
            if (_apiType == null) return false;

            try
            {
                var method = _apiType.GetMethod("RegisterPawnVariable",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(string), typeof(Func<Pawn, string>), typeof(string), typeof(int) },
                    null);

                if (method == null)
                {
                    Log.Warning($"[RimMind-Bridge-RimTalk] RegisterPawnVariable method not found");
                    return false;
                }

                method.Invoke(null, new object?[] { modId, variableName, provider, description, priority });
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] RegisterPawnVariable failed: {ex.Message}");
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
                var method = _apiType.GetMethod("RegisterEnvironmentVariable",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(string), typeof(Func<Map, string>), typeof(string), typeof(int) },
                    null);

                if (method == null) return false;

                method.Invoke(null, new object?[] { modId, variableName, provider, description, priority });
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] RegisterEnvironmentVariable failed: {ex.Message}");
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
                var nestedPawnType = _contextCategoriesType.GetNestedType("Pawn");
                if (nestedPawnType == null) return false;

                var categoryField = nestedPawnType.GetField(categoryKey,
                    BindingFlags.Public | BindingFlags.Static);
                if (categoryField == null) return false;

                object? categoryValue = categoryField.GetValue(null);
                if (categoryValue == null) return false;

                var hookOpEnum = _hookRegistryType.GetNestedType("HookOperation");
                if (hookOpEnum == null) return false;

                object? opValue = Enum.ToObject(hookOpEnum, hookOperation);

                var method = _apiType.GetMethod("RegisterPawnHook",
                    BindingFlags.Public | BindingFlags.Static);
                if (method == null) return false;

                method.Invoke(null, new object?[] { modId, categoryValue, opValue, handler, priority });
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] RegisterPawnHook failed: {ex.Message}");
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
                var createMethod = _apiType.GetMethod("CreatePromptEntry",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(string), _promptRoleType ?? typeof(int), _promptPositionType ?? typeof(int), typeof(int), typeof(string) },
                    null);

                if (createMethod == null)
                {
                    Log.Warning("[RimMind-Bridge-RimTalk] AddPromptEntry: exact method match failed, using fallback. This may match an incorrect overload.");
                    createMethod = _apiType.GetMethod("CreatePromptEntry",
                        BindingFlags.Public | BindingFlags.Static);
                }

                if (createMethod == null) return false;

                object? roleObj = _promptRoleType != null ? Enum.ToObject(_promptRoleType, roleValue) : roleValue;
                object? posObj = _promptPositionType != null ? Enum.ToObject(_promptPositionType, positionValue) : positionValue;

                object? entry = createMethod.Invoke(null, new object?[] { name, content, roleObj, posObj, inChatDepth, sourceModId });
                if (entry == null) return false;

                var addMethod = _apiType.GetMethod("AddPromptEntry",
                    BindingFlags.Public | BindingFlags.Static);
                if (addMethod == null) return false;

                object? result = addMethod.Invoke(null, new object?[] { entry });
                return result is bool b && b;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] AddPromptEntry failed: {ex.Message}");
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
                var method = _apiType.GetMethod("UnregisterAllHooks",
                    BindingFlags.Public | BindingFlags.Static);
                if (method == null) return false;

                method.Invoke(null, new object?[] { modId });
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] UnregisterAllHooks failed: {ex.Message}");
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
                var method = _apiType.GetMethod("RemovePromptEntriesByModId",
                    BindingFlags.Public | BindingFlags.Static);
                if (method == null) return 0;

                object? result = method.Invoke(null, new object?[] { modId });
                return result is int i ? i : 0;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimTalk] RemovePromptEntriesByModId failed: {ex.Message}");
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
