using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Bridge.RimTalk.Detection;
using RimMind.Bridge.RimTalk.Settings;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    [Collection("RimTalk")]
    public class RimTalkBridgeCoordinatorTests
    {
        private readonly ContextPullBridge _pullStub;
        private readonly ContextPushBridge _pushStub;
        private readonly PersonaPushBridge _personaStub;

        public RimTalkBridgeCoordinatorTests()
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
        public void Register_RimTalkNotActive_SkipsAllModules()
        {
            RimTalkDetector.IsRimTalkActive = false;

            RimTalkBridgeCoordinator.Register();

            Assert.False(_pullStub.RegisterCalled);
            Assert.False(_pushStub.RegisterCalled);
            Assert.False(_personaStub.RegisterCalled);
            Assert.False(_pullStub.IsRegistered);
            Assert.False(_pushStub.IsRegistered);
            Assert.False(_personaStub.IsRegistered);
        }

        [Fact]
        public void Register_RimTalkActive_RegistersAllModules()
        {
            // 新设计：Coordinator foreach 调用所有模块的 Register()，
            // 各模块内部自行决定是否实际注册（源码逻辑由 Autotester 验证）。
            // stub 总是注册成功，此处验证 Coordinator 不再按 settings/API 条件筛选模块。
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkDetector.IsRimTalkApiAvailable = false;

            RimTalkBridgeCoordinator.Register();

            Assert.True(_pullStub.RegisterCalled);
            Assert.True(_pushStub.RegisterCalled);
            Assert.True(_personaStub.RegisterCalled);
            Assert.True(_pullStub.IsRegistered);
            Assert.True(_pushStub.IsRegistered);
            Assert.True(_personaStub.IsRegistered);
        }

        [Fact]
        public void Register_CalledTwice_IsIdempotentPerModule()
        {
            // 幂等性：第二次 Register() 对已注册模块是 no-op（stub 的 IsRegistered 守卫）。
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkBridgeCoordinator.Register();
            // 重置标志以检测第二次调用
            _pullStub.RegisterCalled = false;
            _pushStub.RegisterCalled = false;
            _personaStub.RegisterCalled = false;

            RimTalkBridgeCoordinator.Register();

            // 第二次调用因 IsRegistered=true 而跳过，RegisterCalled 保持 false
            Assert.False(_pullStub.RegisterCalled);
            Assert.False(_pushStub.RegisterCalled);
            Assert.False(_personaStub.RegisterCalled);
        }

        [Fact]
        public void Unregister_AfterRegister_CallsUnregisterOnAllModules()
        {
            RimTalkDetector.IsRimTalkActive = true;
            RimTalkBridgeCoordinator.Register();

            RimTalkBridgeCoordinator.Unregister();

            Assert.True(_pullStub.UnregisterCalled);
            Assert.True(_pushStub.UnregisterCalled);
            Assert.True(_personaStub.UnregisterCalled);
            Assert.False(_pullStub.IsRegistered);
            Assert.False(_pushStub.IsRegistered);
            Assert.False(_personaStub.IsRegistered);
        }

        [Fact]
        public void Unregister_NeverRegistered_DoesNotCallUnregisterOnModules()
        {
            // 对称性回归测试：Unregister 对未注册模块是 no-op，
            // 修复原设计 Register 条件/Unregister 无条件的非对称问题。
            RimTalkBridgeCoordinator.Unregister();

            Assert.False(_pullStub.UnregisterCalled);
            Assert.False(_pushStub.UnregisterCalled);
            Assert.False(_personaStub.UnregisterCalled);
            Assert.False(_pullStub.IsRegistered);
        }

        [Fact]
        public void Modules_DefaultContainsThreeBridges()
        {
            // 重置注入，验证默认模块列表
            RimTalkBridgeCoordinator.SetModulesForTesting(null);

            var modules = RimTalkBridgeCoordinator.Modules;

            Assert.Equal(3, modules.Count);
            Assert.Equal("context_pull", modules[0].Id);
            Assert.Equal("context_push", modules[1].Id);
            Assert.Equal("persona_push", modules[2].Id);
            Assert.All(modules, m => Assert.Equal("RimMindBridgeRimTalk", m.OwnerModId));
        }
    }
}
