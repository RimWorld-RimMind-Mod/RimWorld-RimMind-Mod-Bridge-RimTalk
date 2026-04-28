# AGENTS.md — RimMind-Bridge-RimTalk

RimMind 与 RimTalk 模组协调层，对话门控 + 上下文双向推送/拉取。

## 项目定位

通过 `RimTalkApiShim` 反射封装调用RimTalk API(无编译期依赖):
- **DialogueGate**: SkipCheck防止与RimTalk重复触发Chitchat/Auto/PlayerInput对话
- **ContextPushBridge**: 将RimMind人格/记忆/叙事者/顾问/塑造数据推送到RimTalk变量+PromptEntry
- **PersonaPushBridge**: 细粒度人格推送(4个变量+Traits/Mood Hook)
- **ContextPullBridge**: 拉取RimTalk对话历史注册为RimMind Provider(rimtalk_history)

## 构建

| 项 | 值 |
|----|-----|
| Target | net48, C#9.0, Nullable enable |
| Output | `../1.6/Assemblies/` |
| Assembly | RimMindBridgeRimTalk |
| 依赖 | RimMindCore, RimMindAdvisor, RimMindPersonality, RimMindMemory.dll; Krafs.Rimworld.Ref, Lib.Harmony.Ref |
| 无编译期引用 | RimTalk(纯反射), RimMind-Dialogue(通过Core API间接), RimMind-Storyteller(不使用) |

## 源码结构

```
Source/
├── RimMindBridgeRimTalkMod.cs    Mod入口
├── Bridge/
│   ├── DialogueGate.cs           对话门控(ShouldSkipDialogue/ShouldSkipFloatMenuOption)
│   ├── ContextPushBridge.cs      推送RimMind数据→RimTalk变量(5个)+PromptEntry
│   ├── PersonaPushBridge.cs      细粒度人格推送(4个变量+Traits/Mood Hook)
│   ├── ContextPullBridge.cs      拉取RimTalk对话→RimMind Provider(rimtalk_history)
│   └── RimTalkApiShim.cs         反射封装层
├── Detection/RimTalkDetector.cs  RimTalk激活检测(6000tick缓存+IsApiAvailable)
└── Settings/BridgeRimTalkSettings.cs  15项设置
```

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
| ContextPullBridge | `BridgeRimTalk` | RimMind Provider: rimtalk_history(Pawn, PriorityMemory, 6条) |

## 已知限制

- Cleanup不清理Variables(RimTalk API不提供Unregister)
- 设置变更需重启(Push/Pull注册仅在启动时执行)
- Tuple反射脆弱(ContextPullBridge依赖Item1/Item2字段名)
- DialogueGate全局门控(pawn参数未使用)

## 代码约定

- 所有RimTalk调用通过 `RimTalkApiShim` 封装，反射包裹try-catch
- 各桥接模块使用独立ModId确保Cleanup互不干扰
- Harmony ID: `mcocdaa.RimMindBridgeRimTalk`
- 翻译前缀: `RimMind.Bridge.RimTalk.*`

## 操作边界

### ✅ 必须做
- 所有RimTalk调用通过RimTalkApiShim封装
- 反射调用包裹try-catch
- 新设置项在ExposeData + UI + 翻译XML三处同步

### ⚠️ 先询问
- 修改DialogueGate门控逻辑
- 修改推送条目上限(当前5)
- 修改ContextPullBridge Tuple反射字段名依赖

### 🚫 绝对禁止
- 对RimTalk编译期引用
- Cleanup不调用UnregisterAllHooks+RemovePromptEntriesByModId
- 反射访问RimTalk内部类型不包裹try-catch
- 设置变更后未重启就期望Push/Pull生效
