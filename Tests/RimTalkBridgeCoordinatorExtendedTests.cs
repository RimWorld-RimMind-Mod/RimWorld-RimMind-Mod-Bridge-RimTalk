using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class RimTalkBridgeCoordinatorExtendedTests
    {
        private readonly ContextPullBridge _pullStub;
        private readonly ContextPushBridge _pushStub;
        private readonly PersonaPushBridge _personaStub;

        public RimTalkBridgeCoordinatorExtendedTests()
        {
            RimTalkDetector.IsRimTalkActive = false;
            RimTalkDetector.IsRimTalkApiAvailable = false;
            BridgeRimTalkSettings.Reset();
            _pullStub = new ContextPullBridge();
            _pushStub = new ContextPushBridge();
            _personaStub = new PersonaPushBridge();
            RimTalkBridgeCoordinator.SetModulesForTesting(new List<IBridgeModule>
            {
                _pullStub,
                _pushStub,
                _personaStub
            });
        }

        [Fact]
        public void Register_RegardlessOfApiAvailability_CallsRegisterOnAllModules()
        {
            // 新设计：Coordinator 不再检查 IsRimTalkApiAvailable 来筛选模块。
            // API 可用性检查已移入各 bridge 的 Register() 内部（源码逻辑由 Autotester 验证）。
            // 此处验证 Coordinator 总是 foreach 调用所有模块的 Register()。
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(_pullStub.RegisterCalled);
            Assert.True(_pushStub.RegisterCalled);
            Assert.True(_personaStub.RegisterCalled);
        }

        [Fact]
        public void Register_RegardlessOfSettings_CallsRegisterOnAllModules()
        {
            // 新设计：Coordinator 不再检查 settings.pushPersonality 等来筛选模块。
            // settings 检查已移入各 bridge 的 Register() 内部（源码逻辑由 Autotester 验证）。
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = true;
            BridgeRimTalkSettings.Get().pushPersonality = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(_pullStub.RegisterCalled);
            Assert.True(_pushStub.RegisterCalled);
            Assert.True(_personaStub.RegisterCalled);
        }

        [Fact]
        public void Unregister_WithoutPriorRegister_IsNoOpOnAllModules()
        {
            // 对称性：Unregister 对未注册模块是 no-op（IsRegistered 守卫）。
            RimTalkBridgeCoordinator.Unregister();

            Assert.False(_pullStub.UnregisterCalled);
            Assert.False(_pushStub.UnregisterCalled);
            Assert.False(_personaStub.UnregisterCalled);
        }

        [Fact]
        public void Register_Unregister_Register_CycleResetsState()
        {
            // 周期测试：Register -> Unregister -> Register 应能重新注册。
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkBridgeCoordinator.Register();
            Assert.True(_pullStub.IsRegistered);

            RimTalkBridgeCoordinator.Unregister();
            Assert.False(_pullStub.IsRegistered);

            // 重置标志以验证第二次注册周期
            _pullStub.RegisterCalled = false;
            _pushStub.RegisterCalled = false;
            _personaStub.RegisterCalled = false;
            _pullStub.UnregisterCalled = false;
            _pushStub.UnregisterCalled = false;
            _personaStub.UnregisterCalled = false;

            RimTalkBridgeCoordinator.Register();
            Assert.True(_pullStub.RegisterCalled);
            Assert.True(_pullStub.IsRegistered);

            RimTalkBridgeCoordinator.Unregister();
            Assert.True(_pullStub.UnregisterCalled);
            Assert.False(_pullStub.IsRegistered);
        }

        [Fact]
        public void SetModulesForTesting_Null_RestoresDefaultList()
        {
            RimTalkBridgeCoordinator.SetModulesForTesting(null);

            var modules = RimTalkBridgeCoordinator.Modules;
            Assert.Equal(3, modules.Count);
        }

        [Fact]
        public void Modules_ExposesIReadOnlyList()
        {
            // 验证 Modules 属性返回 IReadOnlyList<IBridgeModule>，Coordinator 持有可注入列表。
            var modules = RimTalkBridgeCoordinator.Modules;
            Assert.IsType<System.Collections.Generic.List<IBridgeModule>>(modules);
            Assert.Equal(3, modules.Count);
        }
    }
}
