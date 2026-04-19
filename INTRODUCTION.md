# RimMind - Bridge: RimTalk

RimMind 套件与 RimTalk 模组的协调层，当两个模组同时激活时实现数据互通与对话互斥。

## 核心能力

**对话门控** - 当 RimTalk 激活时，自动跳过 RimMind-Dialogue 的重复对话触发（闲聊、自动对话、玩家对话），避免两个 AI 对话系统冲突。

**上下文推送** - 将 RimMind 的人格档案、记忆数据、叙述者状态、顾问日志、塑造历史注册为 RimTalk 的 Prompt 变量，让 RimTalk 的对话也能感知 RimMind 的 AI 数据。

**人格推送** - 将 RimMind 人格数据以更细粒度注入 RimTalk 的上下文分类（特质、情绪），通过 Hook 机制增强 RimTalk 对殖民者的理解。

**对话历史拉取** - 可选地将 RimTalk 的对话历史注册为 RimMind 的上下文 Provider，实现 RimTalk → RimMind 的反向数据流。

## 技术亮点

- 纯反射调用 RimTalk API（RimTalkApiShim），无编译期依赖，RimTalk 未安装时静默跳过
- 双向数据流：RimMind → RimTalk（推送）+ RimTalk → RimMind（拉取）
- 可配置的对话门控策略，按触发类型独立控制
- 15 个可配置选项，精确控制推送内容

## 建议配图

1. 设置界面截图（展示对话门控、上下文推送、上下文拉取三个分区）
2. RimTalk 对话窗口截图（展示 RimMind 上下文变量的注入效果）

---

# RimMind - Bridge: RimTalk (English)

The coordination layer between the RimMind suite and RimTalk mod, enabling data exchange and dialogue mutual exclusion when both mods are active.

## Key Features

**Dialogue Gate** - When RimTalk is active, automatically skips RimMind-Dialogue's redundant triggers (chitchat, auto-dialogue, player dialogue), preventing conflicts between two AI dialogue systems.

**Context Push** - Registers RimMind's personality profiles, memory data, storyteller state, advisor logs, and shaping history as RimTalk prompt variables, allowing RimTalk to perceive RimMind's AI data during conversations.

**Persona Push** - Injects RimMind persona data into RimTalk's context categories (traits, mood) with fine-grained control via Hook mechanism, enhancing RimTalk's understanding of colonists.

**Dialogue History Pull** - Optionally registers RimTalk's dialogue history as a RimMind context provider, enabling reverse data flow from RimTalk to RimMind.

## Technical Highlights

- Pure reflection-based RimTalk API calls (RimTalkApiShim) with no compile-time dependency; silently skips when RimTalk is not installed
- Bidirectional data flow: RimMind → RimTalk (push) + RimTalk → RimMind (pull)
- Configurable dialogue gate policies per trigger type
- 15 configurable options for precise push content control

## Suggested Screenshots

1. Settings interface (showing Dialogue Gate, Context Push, Context Pull sections)
2. RimTalk conversation window (showing injected RimMind context variables)
