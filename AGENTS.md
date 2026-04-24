# AGENTS.md — RimMind-Bridge-RimTalk

本文件供 AI 编码助手阅读，描述 RimMind-Bridge-RimTalk 的架构、代码约定和扩展模式。

## 项目定位

RimMind-Bridge-RimTalk 是 RimMind 套件与 RimTalk 模组之间的协调层。当两个模组同时激活时，本模组负责：

1. **对话门控**：避免 RimMind-Dialogue 和 RimTalk 重复触发对话
2. **上下文推送**：将 RimMind 的人格、记忆、叙述者、顾问日志、塑造历史等数据注入 RimTalk 的 Prompt 系统
3. **人格推送**：将 RimMind 人格数据作为变量和 Hook 注入 RimTalk 的上下文分类
4. **上下文拉取**：将 RimTalk 的对话历史注册为 RimMind 的上下文 Provider

本模组通过反射调用 RimTalk API（`RimTalkApiShim`），不依赖 RimTalk 的编译期引用，因此 RimTalk 未安装时不会报错。

## 源码结构

```
Source/
├── RimMindBridgeRimTalkMod.cs   Mod 入口，注册 Settings Tab，按条件注册桥接模块
├── Bridge/
│   ├── DialogueGate.cs          对话门控，注册 SkipCheck 防止重复触发
│   ├── ContextPushBridge.cs     上下文推送，将 RimMind 数据注册为 RimTalk 变量/PromptEntry
│   ├── PersonaPushBridge.cs     人格推送，将人格数据注册为 RimTalk 变量和 Hook
│   ├── ContextPullBridge.cs     上下文拉取，将 RimTalk 对话历史注册为 RimMind Provider
│   └── RimTalkApiShim.cs        反射封装层，无编译期依赖地调用 RimTalk API
├── Detection/
│   └── RimTalkDetector.cs       检测 RimTalk 是否激活及其 API 是否可用
└── Settings/
    └── BridgeRimTalkSettings.cs 模组设置（对话门控 + 上下文推送 + 上下文拉取）
```

## 关键类与 API

### RimTalkDetector

检测 RimTalk 模组状态，带缓存：

```csharp
static class RimTalkDetector {
    const string RimTalkPackageId = "cj.rimtalk";

    bool IsRimTalkActive       // RimTalk 模组是否激活（6000 tick 缓存）
    bool IsRimTalkApiAvailable // RimTalkPromptAPI 类型是否存在（启动时检测一次）
    void InvalidateCache()     // 手动刷新缓存（预留 API，当前未调用）
}
```

### RimTalkApiShim

反射封装层，所有对 RimTalk API 的调用都通过此类。不引用 RimTalk DLL，通过 `AccessTools.TypeByName` + `MethodInfo.Invoke` 调用：

```csharp
static class RimTalkApiShim {
    bool IsAvailable { get; }  // 等同 RimTalkDetector.IsRimTalkApiAvailable

    // 注册 Pawn 变量（可在 RimTalk 模板中通过 {{ p.variableName }} 引用）
    bool RegisterPawnVariable(string modId, string variableName,
        Func<Pawn, string> provider, string? description = null, int priority = 100)

    // 注册环境变量（可在 RimTalk 模板中通过 {{variableName}} 引用）
    bool RegisterEnvironmentVariable(string modId, string variableName,
        Func<Map, string> provider, string? description = null, int priority = 100)

    // 注册 Pawn Hook（拦截/增强 RimTalk 的上下文分类内容）
    bool RegisterPawnHook(string modId, string categoryKey,
        int hookOperation, Func<Pawn, string, string> handler, int priority = 100)

    // 添加 PromptEntry（向 RimTalk 的 Prompt 模板注入段落）
    bool AddPromptEntry(string name, string content,
        int roleValue = 0, int positionValue = 0,
        int inChatDepth = 0, string? sourceModId = null)

    // 清理（注意：仅清理 Hooks 和 PromptEntries，不清理 Variables）
    bool UnregisterAllHooks(string modId)
    int RemovePromptEntriesByModId(string modId)
    void Cleanup(string modId)  // UnregisterAllHooks + RemovePromptEntriesByModId
}
```

反射的目标类型：

| 常量 | 类型全名 | 用途 |
|------|---------|------|
| `ApiTypeName` | `RimTalk.API.RimTalkPromptAPI` | 主 API 入口 |
| `HookRegistryTypeName` | `RimTalk.API.ContextHookRegistry` | Hook 操作枚举 |
| `ContextCategoriesTypeName` | `RimTalk.API.ContextCategories` | 上下文分类（含嵌套 Pawn 类） |
| `PromptEntryTypeName` | `RimTalk.Prompt.PromptEntry` | Prompt 条目 |
| `PromptRoleTypeName` | `RimTalk.Prompt.PromptRole` | 角色枚举 |
| `PromptPositionTypeName` | `RimTalk.Prompt.PromptPosition` | 位置枚举 |

### DialogueGate

对话门控，防止 RimMind-Dialogue 和 RimTalk 同时触发对话：

```csharp
static class DialogueGate {
    bool ShouldSkipDialogue(Pawn pawn, string triggerType)
    // triggerType: "Chitchat" | "Auto" | "PlayerInput"

    bool ShouldSkipFloatMenuOption()
    // 判断是否跳过 RimMind 的"与X对话"浮动菜单

    void RegisterSkipChecks()
    // 注册到 RimMindAPI.RegisterDialogueSkipCheck / RegisterFloatMenuSkipCheck
}
```

门控逻辑：

| triggerType | 跳过条件 |
|-------------|---------|
| `"Chitchat"` | `enableDialogueGate && skipChitchat` |
| `"Auto"` | `enableDialogueGate && skipAutoDialogue` |
| `"PlayerInput"` | `enableDialogueGate && skipPlayerDialogue` |
| 浮动菜单 | `enableDialogueGate && skipPlayerDialogue && !forceRimMindPlayerDialogue` |

### ContextPushBridge

将 RimMind 数据推送到 RimTalk Prompt 系统：

```csharp
static class ContextPushBridge {
    const string ModId = "RimMind.Bridge.RimTalk.Push";
    void Register()    // 根据设置注册各推送模块
    void Unregister()  // 清理所有注册（RimTalkApiShim.Cleanup，预留 API）
}
```

注册的 RimTalk 变量：

| 变量名 | 类型 | 数据来源 | 优先级 | 设置开关 |
|--------|------|---------|--------|---------|
| `rimmind_personality` | Pawn | AIPersonalityWorldComponent | 50 | pushPersonality |
| `rimmind_storyteller` | Environment | RimMindMemoryWorldComponent.NarratorStore | 80 | pushStoryteller |
| `rimmind_memory` | Pawn | RimMindMemoryWorldComponent.PawnStore | 60 | pushMemory |
| `rimmind_shaping` | Pawn | AIPersonalityWorldComponent.playerShapingHistory | 70 | pushShaping |
| `rimmind_advisor_log` | Pawn | AdvisorHistoryStore | 80 | pushAdvisorLog |

注册的 PromptEntry：`RimMind Context`，包含人格和叙述者模板变量引用。

### PersonaPushBridge

将 RimMind 人格数据以更细粒度推送到 RimTalk：

```csharp
static class PersonaPushBridge {
    const string ModId = "RimMind.Bridge.RimTalk.Persona";
    void Register()    // 注册变量和 Hook
    void Unregister()  // 清理（RimTalkApiShim.Cleanup，预留 API）
}
```

注册的 RimTalk 变量：

| 变量名 | 类型 | 数据来源 | 优先级 |
|--------|------|---------|--------|
| `rimmind_persona_desc` | Pawn | profile.description | 40 |
| `rimmind_persona_work` | Pawn | profile.workTendencies | 45 |
| `rimmind_persona_social` | Pawn | profile.socialTendencies | 45 |
| `rimmind_persona_narrative` | Pawn | profile.aiNarrative | 55 |

注册的 RimTalk Hook：

| 分类 | 操作 | 说明 | 优先级 | 设置开关 |
|------|------|------|--------|---------|
| `Traits` | Append(0) | 追加人格描述到特质上下文 | 90 | injectPersonaToTraits |
| `Mood` | Append(0) | 追加 AI 叙事到情绪上下文 | 90 | injectPersonaToMood |

### ContextPullBridge

从 RimTalk 拉取对话历史，注册为 RimMind Provider：

```csharp
static class ContextPullBridge {
    const string ModId = "RimMind.BridgeRimTalk";
    void Register()    // 根据设置注册各 Provider
    void Unregister()  // 清理所有注册（RimMindAPI.UnregisterPawnContextProvider，预留 API）
}
```

注册的 Provider：

| Provider ID | 类型 | 数据来源 | 优先级 | 设置开关 |
|-------------|------|---------|--------|---------|
| `rimtalk_history` | PawnContextProvider | RimTalk.Data.TalkHistory（反射） | PriorityMemory | pullRimTalkHistory |

反射目标类型：

| 类型全名 | 用途 |
|---------|------|
| `RimTalk.Data.TalkHistory` | 对话历史数据类 |
| `TalkHistory.GetMessageHistory(Pawn, bool)` | 获取 Pawn 的消息历史 |

拉取逻辑：
- 遍历地图上所有自由殖民者的对话历史
- 筛选与目标 Pawn 相关的消息（Pawn 自身的对话 或 内容中包含 Pawn 名称的对话）
- 取最近 6 条相关消息
- 角色标签映射：0 → System, 1 → User, 2 → AI

### BridgeRimTalkSettings

```csharp
class BridgeRimTalkSettings : ModSettings {
    // 对话门控
    bool enableDialogueGate;          // 默认 true
    bool skipChitchat;                // 默认 true
    bool skipAutoDialogue;            // 默认 true
    bool skipPlayerDialogue;          // 默认 true
    bool forceRimMindPlayerDialogue;  // 默认 false

    // 上下文推送
    bool enableContextPush;            // 默认 true
    bool pushPersonality;             // 默认 true
    bool pushStoryteller;             // 默认 true
    bool pushMemory;                  // 默认 false
    bool pushAdvisorLog;              // 默认 true
    bool pushShaping;                 // 默认 false
    bool injectPersonaToTraits;       // 默认 false
    bool injectPersonaToMood;         // 默认 false

    // 上下文拉取
    bool enableContextPull;           // 默认 true
    bool pullRimTalkHistory;          // 默认 true

    static BridgeRimTalkSettings Get();
    static void DrawSettingsContent(Rect inRect);
}
```

## 数据流

```
RimMind 子模组数据                RimTalk Prompt 系统
┌──────────────────┐             ┌──────────────────┐
│ Personality      │──PushVar──→ │ {{ p.rimmind_personality }}
│ Storyteller      │──PushVar──→ │ {{ rimmind_storyteller }}
│ Memory           │──PushVar──→ │ {{ p.rimmind_memory }}
│ Shaping          │──PushVar──→ │ {{ p.rimmind_shaping }}
│ Advisor          │──PushVar──→ │ {{ p.rimmind_advisor_log }}
│ Persona (细粒度) │──PushVar──→ │ {{ p.rimmind_persona_desc }} 等
│ Persona          │──Hook─────→ │ Traits / Mood 上下文增强
└──────────────────┘             └──────────────────┘

RimTalk 对话历史  ──PullProvider──→  RimMind 上下文（rimtalk_history）

RimMind-Dialogue 触发  ──DialogueGate──→  跳过/放行
```

## 初始化流程

```
RimMindBridgeRimTalkMod 构造函数
    │
    ├── GetSettings<BridgeRimTalkSettings>()
    ├── RimMindAPI.RegisterSettingsTab("bridge_rimtalk", ...)
    │
    ├── RimTalkDetector.IsRimTalkActive?
    │       │
    │       ├── No  → Log + 跳过所有桥接模块
    │       │
    │       └── Yes → DialogueGate.RegisterSkipChecks()
    │               ContextPullBridge.Register()
    │               │
    │               ├── RimTalkDetector.IsRimTalkApiAvailable?
    │               │       │
    │               │       ├── No  → Log.Warning + 跳过 Push 模块
    │               │       │
    │               │       └── Yes → ContextPushBridge.Register()
    │               │               │
    │               │               └── pushPersonality?
    │               │                       ├── Yes → PersonaPushBridge.Register()
    │               │                       └── No  → 跳过
```

## 代码约定

### 命名空间

| 命名空间 | 目录 | 职责 |
|---------|------|------|
| `RimMind.Bridge.RimTalk` | Source/ 根目录 | Mod 入口 |
| `RimMind.Bridge.RimTalk.Bridge` | Bridge/ | 桥接模块 |
| `RimMind.Bridge.RimTalk.Detection` | Detection/ | RimTalk 检测 |
| `RimMind.Bridge.RimTalk.Settings` | Settings/ | 设置 |

### 反射安全

- 所有对 RimTalk API 的调用通过 `RimTalkApiShim` 封装
- 反射结果缓存到 `static Type?` 字段，`EnsureResolved()` 延迟解析，仅执行一次
- 所有反射调用包裹在 try-catch 中，失败时 `Log.Warning` 并返回 false/空值
- 不引用 RimTalk DLL，通过 `AccessTools.TypeByName` 动态解析类型
- `RimTalkApiShim.IsAvailable` 检查 `RimTalkDetector.IsRimTalkApiAvailable`，作为前置守卫

### ModId

各桥接模块使用独立 ModId，确保 Cleanup 互不干扰：

| 模块 | ModId |
|------|-------|
| ContextPushBridge | `RimMind.Bridge.RimTalk.Push` |
| PersonaPushBridge | `RimMind.Bridge.RimTalk.Persona` |
| ContextPullBridge | `RimMind.BridgeRimTalk` |

### 构建

| 配置项 | 值 |
|--------|-----|
| 目标框架 | `net48` |
| C# 语言版本 | 9.0 |
| Nullable | enable |
| RimWorld 版本 | 1.6 |
| 输出路径 | `../1.6/Assemblies/` |
| 部署 | 设置 `RIMWORLD_DIR` 环境变量后自动部署 |
| NuGet 依赖 | `Krafs.Rimworld.Ref 1.6.*-*`, `Lib.Harmony.Ref 2.*` |
| 编译期引用 | RimMindCore, RimMindAdvisor, RimMindPersonality, RimMindMemory（均为 Private=false） |
| 无编译期引用 | RimTalk（纯反射） |

### 加载顺序

```
Harmony → cj.rimtalk → RimMind-Core → RimMind 子模组 → RimMind-Bridge-RimTalk
```

### UI 本地化

所有 UI 文本通过 `Languages/{lang}/Keyed/RimMind_BridgeRimTalk.xml` 的 Keyed 翻译，禁止硬编码中文。英文为权威源。

### 设置 UI

通过 `RimMindAPI.RegisterSettingsTab` 注册到 Core 的多分页设置界面，Tab 标签为 "Bridge (RimTalk)"。使用 `SettingsUIHelper` 辅助工具类绘制。

## 扩展指南

### 新增 RimTalk 变量推送

1. 在 `ContextPushBridge` 或 `PersonaPushBridge` 中添加注册方法
2. 使用 `RimTalkApiShim.RegisterPawnVariable` / `RegisterEnvironmentVariable`
3. 在 `BridgeRimTalkSettings` 中添加对应开关
4. 在语言文件中添加翻译键

### 新增 RimTalk Hook

1. 使用 `RimTalkApiShim.RegisterPawnHook`
2. `categoryKey` 对应 `RimTalk.API.ContextCategories.Pawn` 的字段名（如 "Traits"、"Mood"）
3. `hookOperation`：0 = Append

### 新增反向数据流（RimTalk → RimMind）

1. 在 `ContextPullBridge` 中通过反射读取 RimTalk 数据
2. 使用 `RimMindAPI.RegisterPawnContextProvider` / `RegisterStaticProvider` 注册
3. 在 `Unregister` 中调用 `RimMindAPI.UnregisterPawnContextProvider` 清理
