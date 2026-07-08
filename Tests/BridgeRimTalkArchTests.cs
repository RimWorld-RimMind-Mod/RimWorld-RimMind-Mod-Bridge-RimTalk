using System.Collections.Generic;
using System.Reflection;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Bridge.RimTalk.Bridge;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    /// <summary>
    /// 架构守卫测试：防止已完成的修复回归。
    /// 1. 验证 RimTalkApiShim 不含已移除的死代码字段 _registeredVariableIds
    /// 2. 验证三个 bridge 类型实现 IBridgeModule 接口
    /// 3. 验证 Coordinator.Modules 属性类型为 IReadOnlyList{IBridgeModule}，
    ///    且默认模块列表全部实现 IBridgeModule
    /// 注意：bridge 类型在测试项目中为 stub（RimTalkStubs.cs），stub 同样实现 IBridgeModule，
    /// 因此接口契约测试仍有效。RimTalkApiShim 为源码编译，死代码字段测试直接验证源码。
    /// </summary>
    [Collection("RimTalk")]
    public class BridgeRimTalkArchTests
    {
        [Fact]
        public void RimTalkApiShim_HasNo_RegisteredVariableIds_Field()
        {
            // 死代码守卫：_registeredVariableIds 字段已移除（commit 5539401），
            // 该字段仅写入从不读取。防止回归。
            var field = typeof(RimTalkApiShim).GetField("_registeredVariableIds",
                BindingFlags.NonPublic | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.Instance);
            Assert.Null(field);
        }

        [Fact]
        public void ContextPullBridge_Implements_IBridgeModule()
        {
            Assert.True(typeof(IBridgeModule).IsAssignableFrom(typeof(ContextPullBridge)));
        }

        [Fact]
        public void ContextPushBridge_Implements_IBridgeModule()
        {
            Assert.True(typeof(IBridgeModule).IsAssignableFrom(typeof(ContextPushBridge)));
        }

        [Fact]
        public void PersonaPushBridge_Implements_IBridgeModule()
        {
            Assert.True(typeof(IBridgeModule).IsAssignableFrom(typeof(PersonaPushBridge)));
        }

        [Fact]
        public void Coordinator_Modules_PropertyReturnsIReadOnlyListOfIBridgeModule()
        {
            // 验证 Modules 属性的编译期类型为 IReadOnlyList<IBridgeModule>，
            // 确保 Coordinator 以接口集合驱动，而非具体类型列表。
            var property = typeof(RimTalkBridgeCoordinator)
                .GetProperty(nameof(RimTalkBridgeCoordinator.Modules));
            Assert.NotNull(property);
            Assert.Equal(typeof(IReadOnlyList<IBridgeModule>), property!.PropertyType);
        }

        [Fact]
        public void Coordinator_DefaultModules_AllImplementIBridgeModule()
        {
            // 重置注入，验证默认模块列表全部实现 IBridgeModule。
            RimTalkBridgeCoordinator.SetModulesForTesting(null);

            foreach (var m in RimTalkBridgeCoordinator.Modules)
            {
                Assert.IsAssignableFrom<IBridgeModule>(m);
            }
        }
    }
}
