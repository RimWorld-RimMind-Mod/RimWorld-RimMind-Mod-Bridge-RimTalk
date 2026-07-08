using System;
using System.Reflection;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class RimTalkApiShimTests
    {
        public RimTalkApiShimTests()
        {
            // 隔离每个测试：重置检测器与 shim 静态状态
            RimTalkDetector.IsRimTalkApiAvailable = false;
            ResetShimStaticState();
        }

        // ---- 正向路径：签名匹配时应继续执行并返回 true ----

        [Fact]
        public void AddPromptEntry_ReturnsTrue_WhenSignatureMatchesAndAddSucceeds()
        {
            RimTalkDetector.IsRimTalkApiAvailable = true;
            SetShimField("_apiType", typeof(StubApiWithCorrectSignature));
            SetShimField("_promptEntryType", typeof(object));
            SetShimField("_promptRoleType", null);
            SetShimField("_promptPositionType", null);

            bool result = RimTalkApiShim.AddPromptEntry("test", "content");

            Assert.True(result);
        }

        // ---- Task 8：精确签名匹配失败时不应回退到模糊重载 ----

        [Fact]
        public void AddPromptEntry_ReturnsFalse_WhenExactSignatureNotFound_EvenIfSameNameMethodExists()
        {
            // StubApiWithWrongCreateSignature 暴露了一个同名但签名不同的 CreatePromptEntry(string)。
            // 旧逻辑会回退到模糊 GetMethod 命中该重载并尝试 Invoke（参数数不匹配 → 异常 → false）。
            // 新逻辑：精确匹配失败即返回 false，不进行模糊回退。
            RimTalkDetector.IsRimTalkApiAvailable = true;
            SetShimField("_apiType", typeof(StubApiWithWrongCreateSignature));
            SetShimField("_promptEntryType", typeof(object));
            SetShimField("_promptRoleType", null);
            SetShimField("_promptPositionType", null);

            bool result = RimTalkApiShim.AddPromptEntry("test", "content");

            Assert.False(result);
        }

        [Fact]
        public void AddPromptEntry_ReturnsFalse_WhenCreateMethodMissing()
        {
            RimTalkDetector.IsRimTalkApiAvailable = true;
            SetShimField("_apiType", typeof(StubApiWithoutMethods));
            SetShimField("_promptEntryType", typeof(object));

            bool result = RimTalkApiShim.AddPromptEntry("test", "content");

            Assert.False(result);
        }

        // ---- Task 9.1：RegisterEnvironmentVariable 方法未找到 ----

        [Fact]
        public void RegisterEnvironmentVariable_ReturnsFalse_WhenMethodNotFound()
        {
            RimTalkDetector.IsRimTalkApiAvailable = true;
            SetShimField("_apiType", typeof(StubApiWithoutMethods));

            bool result = RimTalkApiShim.RegisterEnvironmentVariable("mod", "var", _ => "x");

            Assert.False(result);
        }

        // ---- Task 9.2：RegisterPawnHook 找不到 ContextCategories.Pawn 嵌套类型 ----

        [Fact]
        public void RegisterPawnHook_ReturnsFalse_WhenNestedPawnTypeMissing()
        {
            RimTalkDetector.IsRimTalkApiAvailable = true;
            SetShimField("_apiType", typeof(StubApiWithoutMethods));
            SetShimField("_hookRegistryType", typeof(StubHookRegistry));
            SetShimField("_contextCategoriesType", typeof(StubContextCategoriesWithoutPawn));

            bool result = RimTalkApiShim.RegisterPawnHook("mod", "Thoughts", 0, (p, s) => s);

            Assert.False(result);
        }

        // ---- Task 9.3：RegisterPawnHook 找不到指定 categoryKey 字段 ----

        [Fact]
        public void RegisterPawnHook_ReturnsFalse_WhenCategoryFieldMissing()
        {
            RimTalkDetector.IsRimTalkApiAvailable = true;
            SetShimField("_apiType", typeof(StubApiWithoutMethods));
            SetShimField("_hookRegistryType", typeof(StubHookRegistry));
            SetShimField("_contextCategoriesType", typeof(StubContextCategoriesWithPawn));

            bool result = RimTalkApiShim.RegisterPawnHook("mod", "NonExistentField", 0, (p, s) => s);

            Assert.False(result);
        }

        // ---- Task 9.4：RegisterPawnHook 方法未找到 ----

        [Fact]
        public void RegisterPawnHook_ReturnsFalse_WhenMethodNotFound()
        {
            // StubContextCategoriesWithPawn.Pawn.Thoughts 字段存在，故能越过字段检查，
            // 最终在 _apiType 上找不到 RegisterPawnHook 方法。
            RimTalkDetector.IsRimTalkApiAvailable = true;
            SetShimField("_apiType", typeof(StubApiWithoutMethods));
            SetShimField("_hookRegistryType", typeof(StubHookRegistry));
            SetShimField("_contextCategoriesType", typeof(StubContextCategoriesWithPawn));

            bool result = RimTalkApiShim.RegisterPawnHook("mod", "Thoughts", 0, (p, s) => s);

            Assert.False(result);
        }

        // ---- 辅助方法 ----

        private static void ResetShimStaticState()
        {
            // 置 _resolved=true 使 EnsureResolved 成为空操作，保留测试注入的字段值
            SetShimField("_resolved", true);
            SetShimField("_apiType", null);
            SetShimField("_hookRegistryType", null);
            SetShimField("_contextCategoriesType", null);
            SetShimField("_promptEntryType", null);
            SetShimField("_promptRoleType", null);
            SetShimField("_promptPositionType", null);
        }

        private static void SetShimField(string fieldName, object? value)
        {
            var field = typeof(RimTalkApiShim).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);
            field!.SetValue(null, value);
        }
    }

    // ---- Stub API 类型 ----

    /// <summary>
    /// 暴露与期望完全匹配的 CreatePromptEntry 签名，以及返回 true 的 AddPromptEntry。
    /// 用于正向路径验证。
    /// </summary>
    public static class StubApiWithCorrectSignature
    {
        public static object CreatePromptEntry(
            string name, string content, int role, int pos, int depth, string? source)
            => new object();

        public static bool AddPromptEntry(object entry) => true;
    }

    /// <summary>
    /// 暴露同名但签名不同的 CreatePromptEntry(string)（仅 1 个参数）。
    /// 用于验证 Task 8：精确匹配失败时不回退到模糊重载。
    /// </summary>
    public static class StubApiWithWrongCreateSignature
    {
        public static object? CreatePromptEntry(string name) => null;
    }

    /// <summary>
    /// 不包含任何相关方法。用于验证 method-not-found 路径。
    /// </summary>
    public static class StubApiWithoutMethods
    {
    }

    /// <summary>
    /// 不含 Pawn 嵌套类型的 ContextCategories stub。
    /// </summary>
    public static class StubContextCategoriesWithoutPawn
    {
    }

    /// <summary>
    /// 含 Pawn 嵌套类型且有 Thoughts 字段的 ContextCategories stub。
    /// </summary>
    public static class StubContextCategoriesWithPawn
    {
        public static class Pawn
        {
            public static object? Thoughts = new object();
        }
    }

    /// <summary>
    /// 含 HookOperation 嵌套枚举的 HookRegistry stub。
    /// </summary>
    public static class StubHookRegistry
    {
        public enum HookOperation { Pre = 0, Post = 1 }
    }
}
