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
│   ├── RimTalkBridgeCoordinator.cs  统一注册/注销入口(委托 BridgeModuleCoordinator)
│   ├── BridgeModuleCoordinator.cs   IBridgeModule 集合对称 RegisterAll/UnregisterAll 调度器
│   ├── DialogueGate.cs           对话门控(ShouldSkipDialogue/ShouldSkipFloatMenuOption)
│   ├── RimMindProviderReader.cs  Core Provider 字符串读取与可选 Provider 失败告警边界
│   ├── PersonaFormatter.cs       共享人格字符串构建器(BuildFullProfile,供Push/PersonaPush复用)
│   ├── RimTalkContextPushPlan.cs 按设置规划 ContextPush 变量与 PromptEntry 模板
│   ├── ContextPushBridge.cs      [IBridgeModule] 推送RimMind数据→RimTalk变量(5个)+PromptEntry(与PersonaPush Hook自动去重)
│   ├── PersonaPushBridge.cs      [IBridgeModule] 细粒度人格推送(4个变量+Traits/Mood Hook)
│   ├── ContextPullBridge.cs      [IBridgeModule] 拉取RimTalk对话→RimMind Provider(rimtalk_history, MainThreadSnapshot+MessageAccessors缓存)
│   └── RimTalkApiShim.cs         反射封装层(延迟类型解析+异常隔离告警)
├── Detection/RimTalkDetector.cs  RimTalk激活检测(6000 tick TTL 缓存 + IsRimTalkApiAvailable 一次性类型探测, InvalidateCache 重置全部状态)
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

- **Push（RimMind → RimTalk）**：Personality、Memory 和 Advisor 等子模组将字符串发布到 Core `RimMindAPI.Providers`；Bridge 只同步读取 Core Provider 结果，再通过 `RimTalkApiShim` 注册到 RimTalk（当启用 `injectPersonaToTraits` / `injectPersonaToMood` 时，`rimmind_personality` 自动跳过已由 `PersonaPushBridge` Hook 注入的段落以防重复）。Bridge 不编译引用任何子模组的 DTO、WorldComponent 或存储单例。
- **Pull（RimTalk → RimMind）**：`ContextPullBridge` 注册 `rimtalk_history` Context Provider 时，通过 `RimMindAPI.MainThreadSnapshot.GetAsync` 将 Pawn 查找与存活校验（`RimMindPawnLookup.FindPawnByNumber` + `IsActiveSpawnedColonist`）及 `RimTalk.Data.TalkHistory` 反射读取统一封送到主线程快照执行，后台请求线程绝不直接触碰 Verse 或 RimTalk 实时集合。消息条目通过 `ConcurrentDictionary<Type, MessageAccessors>` 缓存 `Item1`/`Item2`（及可选 `Item3`/`Tick`/`Timestamp`）字段或属性访问器；若结构不兼容则触发单次告警（warn-once: `_warnedMissingTupleFields`）并置位 `_disabledDueToStructuralFailure` 自动停用拉取，避免重复反射开销与日志刷屏。

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
- ContextPullBridge 反射容错：通过 `MessageAccessors` 缓存 `Item1`/`Item2` 字段或属性访问器；若 RimTalk 消息结构变化导致字段缺失，仅打印一次告警（`_warnedMissingTupleFields`）并置位 `_disabledDueToStructuralFailure` 熔断停用，直至重新 `Register()`
- DialogueGate全局门控(pawn参数已保留用于未来按pawn门控扩展,当前为全局门控)
- Unregister无自动调用点(RimWorld Mod生命周期无Dispose,需通过DebugAction"Force Unregister Bridges"手动清理)

## 代码约定

- 所有RimTalk调用通过 `RimTalkApiShim` 封装，反射包裹try-catch
- 各桥接模块使用独立ModId确保Cleanup互不干扰
- 新桥接模块实现 `IBridgeModule` 接口并加入 `RimTalkBridgeCoordinator._modules` 列表(由Coordinator统一foreach调度Register/Unregister)
- 人格字符串构建统一通过 `PersonaFormatter.BuildFullProfile`(供ContextPushBridge/PersonaPushBridge复用,避免重复StringBuilder)
- 子模组数据只通过 `RimMindProviderReader` 消费 Core Provider 字符串；Pawn 查找与遍历统一走 `RimMindPawnLookup.FindPawnByNumber` 与 `RimMindPawnLookup.GetSpawnedColonistsSafe`
- `DialogueGate.ShouldSkipDialogue` 对 `"PlayerInput"` 与 `ShouldSkipFloatMenuOption` 统一尊重 `!settings.forceRimMindPlayerDialogue`
- 注册/注销通过 `RimTalkBridgeCoordinator` 统一调度
- Harmony ID: `mcocdaa.RimMindBridgeRimTalk`
- 翻译前缀: `RimMind.BridgeRimTalk.*`

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
