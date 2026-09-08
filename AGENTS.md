# AGENTS.md — RimMind-Bridge-RimTalk

RimMind 与 RimTalk 模组协调层，对话门控 + 上下文双向推送/拉取。

## 项目定位

依赖 RimMind Core 公共 API，并通过 `RimTalkApiShim` 反射封装调用 RimTalk API：
- **DialogueGate**: SkipCheck防止与RimTalk重复触发Chitchat/Auto/PlayerInput对话
- **ContextPushBridge**: 将 Core Provider 提供的人格/记忆/叙事者/顾问/塑造文本推送到 RimTalk 变量+PromptEntry
- **PersonaPushBridge**: 细粒度人格推送(4个变量+Traits/Mood Hook)
- **ContextPullBridge**: 拉取RimTalk对话历史注册为RimMind Provider(rimtalk_history)
- **RimTalkBridgeCoordinator**: 统一注册/注销入口

## 构建

| 项 | 值 |
|----|-----|
| Target | net48, C#9.0, Nullable enable |
| Output | `../1.6/Assemblies/` |
| Assembly | RimMindBridgeRimTalk |
| 依赖 | RimMind Core + 运行时反射的 RimTalk |

## 源码结构

```
Source/
├── RimMindBridgeRimTalkMod.cs    Mod入口(委托Coordinator)
├── Bridge/
│   ├── RimTalkBridgeCoordinator.cs  统一注册/注销入口(List<IBridgeModule>驱动,foreach对称Register/Unregister)
│   ├── DialogueGate.cs           对话门控(ShouldSkipDialogue/ShouldSkipFloatMenuOption)
│   ├── RimMindProviderReader.cs  Core Provider 字符串读取与可选 Provider 失败告警边界
│   ├── PersonaFormatter.cs       共享人格字符串构建器(BuildFullProfile,供Push/PersonaPush复用)
│   ├── ContextPushBridge.cs      [IBridgeModule] 推送RimMind数据→RimTalk变量(5个)+PromptEntry
│   ├── PersonaPushBridge.cs      [IBridgeModule] 细粒度人格推送(4个变量+Traits/Mood Hook)
│   ├── ContextPullBridge.cs      [IBridgeModule] 拉取RimTalk对话→RimMind Provider(rimtalk_history)
│   └── RimTalkApiShim.cs         反射封装层(所有失败路径含Log.Warning)
├── Detection/RimTalkDetector.cs  RimTalk激活检测(6000tick缓存+IsApiAvailable,InvalidateCache重置全部状态)
├── Extensions/
│   ├── RimTalkSettingsTab.cs     设置Tab UI
│   ├── RimTalkFloatMenuSkipCheck.cs  FloatMenu SkipCheck Harmony补丁
│   └── RimTalkDialogueSkipCheck.cs   Dialogue SkipCheck Harmony补丁
├── Debug/BridgeRimTalkDebugActions.cs  6个DebugAction(含Force Unregister Bridges)
└── Settings/BridgeRimTalkSettings.cs   15项设置(dirty-flag写盘,变更才标记WriteSettings)
Tests/
├── RimTalkStubs.cs
└── Contracts/
    ├── RimTalkGateContextContracts.cs
    └── RimTalkSettingsCompatibilityContracts.cs
```

## 数据流

Personality、Memory 和 Advisor 等子模组将字符串发布到 Core `RimMindAPI.Providers`；Bridge 只同步读取 Core Provider 结果，再通过 `RimTalkApiShim` 注册到 RimTalk。Bridge 不编译引用任何子模组的 DTO、WorldComponent 或存储单例。

## RimTalkApiShim 反射方法

```csharp
RegisterPawnVariable / RegisterEnvironmentVariable / RegisterPawnHook
AddPromptEntry / UnregisterAllHooks / RemovePromptEntriesByModId / Cleanup
```

反射目标类型: `RimTalk.API.RimTalkPromptAPI`, `ContextHookRegistry`, `ContextCategories.Pawn`, `PromptEntry`, `PromptRole`, `PromptPosition`

## Provider注册

| 模块 | ModId | 注册内容 |
|------|-------|---------|
| ContextPushBridge | `Push` | 5个RimTalk变量(rimmind_personality/storyteller/memory/shaping/advisor_log) + PromptEntry |
| PersonaPushBridge | `Persona` | 4个RimTalk变量(rimmind_persona_desc/work/social/narrative) + Traits/Mood Hook |
| ContextPullBridge | `BridgeRimTalk` | RimMind Provider: rimtalk_history(L4_History, 0.5f, 6条) |

## 已知限制

- DialogueGate无Unregister方法(skip check注册后无法清理)
- Cleanup不清理Variables(RimTalk API不提供Unregister;_registeredVariableIds死代码已归档至Refs/backup/RimMind-Bridge-RimTalk/,原仅写入从不读取)
- 设置变更需重启(Push/Pull注册仅在启动时执行)
- Tuple反射脆弱(ContextPullBridge依赖Item1/Item2字段名,已有WarningOnce)
- DialogueGate全局门控(pawn参数已保留用于未来按pawn门控扩展,当前为全局门控)
- Unregister无自动调用点(RimWorld Mod生命周期无Dispose,需通过DebugAction"Force Unregister Bridges"手动清理)

## 代码约定

- 所有RimTalk调用通过 `RimTalkApiShim` 封装，反射包裹try-catch
- 各桥接模块使用独立ModId确保Cleanup互不干扰
- 新桥接模块实现 `IBridgeModule` 接口并加入 `RimTalkBridgeCoordinator._modules` 列表(由Coordinator统一foreach调度Register/Unregister)
- 人格字符串构建统一通过 `PersonaFormatter.BuildFullProfile`(供ContextPushBridge/PersonaPushBridge复用,避免重复StringBuilder)
- 子模组数据只通过 `RimMindProviderReader` 消费 Core Provider 字符串
- 注册/注销通过 `RimTalkBridgeCoordinator` 统一调度
- Harmony ID: `mcocdaa.RimMindBridgeRimTalk`
- 翻译前缀: `RimMind.Bridge.RimTalk.*`

## 操作边界

### ✅ 必须做
- 所有RimTalk调用通过RimTalkApiShim封装
- 反射调用包裹try-catch
- 新设置项在ExposeData + UI + 翻译XML三处同步
- 新桥接模块实现IBridgeModule并加入Coordinator._modules列表(无需手动添加Register/Unregister调用)

### ⚠️ 先询问
- 修改DialogueGate门控逻辑
- 修改推送条目上限(当前5)
- 修改ContextPullBridge Tuple反射字段名依赖
- 修改Coordinator注册顺序

### 🚫 绝对禁止
- 对RimTalk编译期引用
- Cleanup不调用UnregisterAllHooks+RemovePromptEntriesByModId
- 反射访问RimTalk内部类型不包裹try-catch
- 设置变更后未重启就期望Push/Pull生效
