# RimMind - Bridge: RimTalk

RimMind 套件与 RimTalk 模组的协调层，当两个模组同时激活时实现数据互通与对话互斥。

## RimMind 是什么

RimMind 是一套 AI 驱动的 RimWorld 模组套件，通过接入大语言模型（LLM），让殖民者拥有人格、记忆、对话和自主决策能力。

## 本模组的作用

RimTalk 是另一个流行的 RimWorld AI 对话模组。当 RimMind 和 RimTalk 同时安装时，两个 AI 对话系统可能产生冲突（重复触发对话、上下文不互通）。本模组作为协调层解决这些问题：

- **避免重复对话**：当 RimTalk 激活时，自动跳过 RimMind 的对话触发
- **数据互通**：将 RimMind 的人格、记忆、叙述者等数据注入 RimTalk 的 Prompt
- **反向数据流**：可选地将 RimTalk 对话历史注入 RimMind 上下文

## 子模组列表与依赖关系

| 模组 | 职责 | 依赖 | GitHub |
|------|------|------|--------|
| **RimMind-Core** | API 客户端、请求调度、上下文打包 | Harmony | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Core) |
| RimMind-Actions | AI 控制小人的动作执行库 | Core | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Actions) |
| RimMind-Advisor | AI 扮演小人做出工作决策 | Core, Actions | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Advisor) |
| RimMind-Dialogue | AI 驱动的对话系统 | Core | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Dialogue) |
| RimMind-Memory | 记忆采集与上下文注入 | Core | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Memory) |
| RimMind-Personality | AI 生成人格与想法 | Core | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Personality) |
| RimMind-Storyteller | AI 叙事者，智能选择事件 | Core | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Storyteller) |
| **RimMind-Bridge-RimTalk** | RimTalk 协调层 | Core, RimTalk(可选) | [链接](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Bridge-RimTalk) |

```
Core ── Actions ── Advisor
  ├── Dialogue
  ├── Memory
  ├── Personality
  ├── Storyteller
  └── Bridge-RimTalk ←── RimTalk (可选)
```

## 安装步骤

### 从源码安装

**Linux/macOS:**
```bash
git clone git@github.com:RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Bridge-RimTalk.git
cd RimWorld-RimMind-Mod-Bridge-RimTalk
./script/deploy-single.sh <your RimWorld path>
```

**Windows:**
```powershell
git clone git@github.com:RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Bridge-RimTalk.git
cd RimWorld-RimMind-Mod-Bridge-RimTalk
./script/deploy-single.ps1 <your RimWorld path>
```

### 从 Steam 安装

1. 安装 [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) 前置模组
2. 安装 RimMind-Core
3. 按需安装其他 RimMind 子模组
4. 安装 RimTalk（可选，但推荐）
5. 安装 RimMind-Bridge-RimTalk
6. 在模组管理器中确保加载顺序：Harmony → RimTalk → Core → 其他子模组 → Bridge-RimTalk

<!-- ![安装步骤](images/install-steps.png) -->

## 快速开始

本模组无需额外配置即可工作。安装后，当 RimTalk 激活时自动启用桥接功能。

### 调整设置

1. 启动游戏，进入主菜单
2. 点击 **选项 → 模组设置 → RimMind-Core**
3. 切换到 **Bridge (RimTalk)** 标签页
4. 按需调整对话门控、上下文推送、人格推送选项

<!-- ![设置界面](images/screenshot-settings.png) -->

## 核心功能

### 对话门控

当 RimTalk 激活时，自动跳过 RimMind-Dialogue 的重复对话触发，避免两个 AI 对话系统冲突：

- **跳过闲聊**：RimTalk 激活时跳过 RimMind 的 Chitchat 对话触发（默认开启）
- **跳过自动对话**：跳过 RimMind 的自动对话触发（默认开启）
- **跳过玩家对话**：移除 RimMind 的"与X对话"浮动菜单选项（默认开启）
- **保留 RimMind 玩家对话**：即使跳过玩家对话，仍保留 RimMind 的浮动菜单选项（默认关闭）

### 上下文推送

将 RimMind 的上下文数据注册到 RimTalk 的 Prompt 系统中，让 RimTalk 的对话也能感知 RimMind 的 AI 数据：

- **人格数据**：殖民者的 AI 人格档案（描述、工作倾向、社交倾向、AI 叙事）
- **叙述者状态**：RimMind 叙事者的活跃事件
- **记忆数据**：殖民者的短期和长期记忆（默认关闭）
- **塑造历史**：玩家对殖民者人格的塑造记录（默认关闭）
- **顾问日志**：RimMind Advisor 的历史决策记录

### 人格推送

将 RimMind 人格数据以更细粒度注入 RimTalk 的上下文分类（默认关闭，需在上下文推送中开启推送人格数据后可见）：

- **注入到特质上下文**：将人格描述追加到 RimTalk 的 Traits 分类
- **注入到情绪上下文**：将 AI 叙事追加到 RimTalk 的 Mood 分类

### 对话历史拉取

可选地将 RimTalk 的对话历史注册为 RimMind 的上下文 Provider，实现 RimTalk → RimMind 的反向数据流。开启后，RimMind 的 AI 请求也能感知 RimTalk 的对话内容。

## 设置项

### 对话门控

| 设置 | 默认值 | 说明 |
|------|--------|------|
| 启用对话门控 | 开启 | RimTalk 激活时跳过 RimMind 重复对话触发 |
| 跳过闲聊触发 | 开启 | 跳过 Chitchat 对话触发 |
| 跳过自动对话 | 开启 | 跳过自动对话触发 |
| 跳过玩家对话 | 开启 | 移除"与X对话"浮动菜单 |
| 保留 RimMind 玩家对话 | 关闭 | 跳过玩家对话时仍保留 RimMind 选项 |

### 上下文推送

| 设置 | 默认值 | 说明 |
|------|--------|------|
| 启用上下文推送 | 开启 | 将 RimMind 数据注册到 RimTalk Prompt |
| 推送人格数据 | 开启 | 注册 rimmind_personality 变量及细粒度人格变量 |
| 推送叙述者状态 | 开启 | 注册 rimmind_storyteller 环境变量 |
| 推送记忆数据 | 关闭 | 注册 rimmind_memory 变量 |
| 推送塑造历史 | 关闭 | 注册 rimmind_shaping 变量 |
| 推送顾问日志 | 开启 | 注册 rimmind_advisor_log 变量 |
| 注入到特质上下文 | 关闭 | 追加人格描述到 RimTalk Traits 分类（需开启推送人格数据） |
| 注入到情绪上下文 | 关闭 | 追加 AI 叙事到 RimTalk Mood 分类（需开启推送人格数据） |

### 上下文拉取

| 设置 | 默认值 | 说明 |
|------|--------|------|
| 启用上下文拉取 | 开启 | 从 RimTalk 拉取数据注册为 RimMind 上下文 Provider |
| 拉取 RimTalk 对话历史 | 开启 | 将 RimTalk 对话历史注入 RimMind 上下文 |

## 常见问题

**Q: 不安装 RimTalk 可以用吗？**
A: 可以。本模组通过反射检测 RimTalk，未安装时所有桥接功能静默跳过，不会报错。

**Q: 只安装 RimTalk 不安装其他 RimMind 子模组可以吗？**
A: 可以，但上下文推送和人格推送功能需要对应的 RimMind 子模组（Personality、Memory、Advisor、Storyteller）才能推送数据。对话门控功能只需 RimMind-Dialogue。

**Q: 对话门控会不会导致完全没有对话？**
A: 不会。门控只是跳过 RimMind 的重复触发，RimTalk 的对话系统正常工作。你可以通过设置精确控制跳过哪些类型的触发。

**Q: 推送的数据会占用很多 Token 吗？**
A: 数据量可控。每类推送最多显示 5 条记录，且可通过设置独立开关控制。RimTalk 的 Prompt 模板系统也会按优先级裁剪。

**Q: RimTalk 更新后 API 变了怎么办？**
A: 本模组通过反射调用 RimTalk API，所有调用都有 try-catch 保护。API 变更只会导致对应功能静默失效，不会崩溃。

## 致谢

本项目开发过程中参考了以下优秀的 RimWorld 模组：

- [RimTalk](https://github.com/jlibrary/RimTalk.git) - 对话系统与 API 设计参考
- [RimTalk-ExpandActions](https://github.com/sanguodxj-byte/RimTalk-ExpandActions.git) - 动作扩展参考

## 贡献

欢迎提交 Issue 和 Pull Request！如果你有任何建议或发现 Bug，请通过 GitHub Issues 反馈。


---

# RimMind - Bridge: RimTalk (English)

The coordination layer between the RimMind suite and RimTalk mod, enabling data exchange and dialogue mutual exclusion when both mods are active.

## What is RimMind

RimMind is an AI-driven RimWorld mod suite that connects to Large Language Models (LLMs), giving colonists personality, memory, dialogue, and autonomous decision-making.

## What This Mod Does

RimTalk is another popular RimWorld AI dialogue mod. When both RimMind and RimTalk are installed, the two AI dialogue systems may conflict (duplicate dialogue triggers, no context sharing). This mod serves as a coordination layer to resolve these issues:

- **Prevent duplicate dialogues**: Automatically skip RimMind's dialogue triggers when RimTalk is active
- **Data sharing**: Inject RimMind's personality, memory, storyteller data into RimTalk's prompts
- **Reverse data flow**: Optionally inject RimTalk dialogue history into RimMind's context

## Sub-Modules & Dependencies

| Module | Role | Depends On | GitHub |
|--------|------|------------|--------|
| **RimMind-Core** | API client, request dispatch, context packaging | Harmony | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Core) |
| RimMind-Actions | AI-controlled pawn action execution | Core | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Actions) |
| RimMind-Advisor | AI role-plays colonists for work decisions | Core, Actions | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Advisor) |
| RimMind-Dialogue | AI-driven dialogue system | Core | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Dialogue) |
| RimMind-Memory | Memory collection & context injection | Core | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Memory) |
| RimMind-Personality | AI-generated personality & thoughts | Core | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Personality) |
| RimMind-Storyteller | AI storyteller, smart event selection | Core | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Storyteller) |
| **RimMind-Bridge-RimTalk** | RimTalk coordination layer | Core, RimTalk (optional) | [Link](https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Bridge-RimTalk) |

## Installation

### Install from Source

**Linux/macOS:**
```bash
git clone git@github.com:RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Bridge-RimTalk.git
cd RimWorld-RimMind-Mod-Bridge-RimTalk
./script/deploy-single.sh <your RimWorld path>
```

**Windows:**
```powershell
git clone git@github.com:RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Bridge-RimTalk.git
cd RimWorld-RimMind-Mod-Bridge-RimTalk
./script/deploy-single.ps1 <your RimWorld path>
```

### Install from Steam

1. Install [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
2. Install RimMind-Core
3. Install other RimMind sub-modules as needed
4. Install RimTalk (optional, but recommended)
5. Install RimMind-Bridge-RimTalk
6. Ensure load order: Harmony → RimTalk → Core → other sub-modules → Bridge-RimTalk

## Quick Start

This mod works out of the box with no additional configuration. After installation, bridge features are automatically enabled when RimTalk is active.

### Adjust Settings

1. Launch the game, go to main menu
2. Click **Options → Mod Settings → RimMind-Core**
3. Switch to the **Bridge (RimTalk)** tab
4. Adjust dialogue gate, context push, and persona push options as needed

## Key Features

### Dialogue Gate

When RimTalk is active, automatically skips RimMind-Dialogue's redundant triggers to prevent conflicts:

- **Skip chitchat**: Skip RimMind's Chitchat triggers when RimTalk is active (default: on)
- **Skip auto-dialogue**: Skip RimMind's auto-dialogue triggers (default: on)
- **Skip player-dialogue**: Remove RimMind's "Chat with X" float menu option (default: on)
- **Keep RimMind player dialogue**: Keep RimMind's float menu option even when skipping player dialogue (default: off)

### Context Push

Registers RimMind context data into RimTalk's prompt system, allowing RimTalk to perceive RimMind's AI data:

- **Personality data**: Colonist AI personality profiles (description, work tendencies, social tendencies, AI narrative)
- **Storyteller state**: RimMind storyteller's active events
- **Memory data**: Colonist short-term and long-term memories (default: off)
- **Shaping history**: Player's personality shaping records for colonists (default: off)
- **Advisor log**: RimMind Advisor's historical decision records

### Persona Push

Injects RimMind persona data into RimTalk's context categories with fine-grained control (default: off, visible after enabling Push personality data in Context Push):

- **Inject into traits context**: Append personality description to RimTalk's Traits category
- **Inject into mood context**: Append AI narrative to RimTalk's Mood category

### Dialogue History Pull

Optionally registers RimTalk's dialogue history as a RimMind context provider, enabling reverse data flow from RimTalk to RimMind. When enabled, RimMind's AI requests can also perceive RimTalk's conversation content.

## Settings

### Dialogue Gate

| Setting | Default | Description |
|---------|---------|-------------|
| Enable dialogue gate | On | Skip RimMind redundant triggers when RimTalk is active |
| Skip chitchat triggers | On | Skip Chitchat dialogue triggers |
| Skip auto-dialogue | On | Skip auto-dialogue triggers |
| Skip player-dialogue | On | Remove "Chat with X" float menu |
| Keep RimMind player dialogue | Off | Keep RimMind option when skipping player dialogue |

### Context Push

| Setting | Default | Description |
|---------|---------|-------------|
| Enable context push | On | Register RimMind data into RimTalk prompts |
| Push personality data | On | Register rimmind_personality variable and fine-grained persona variables |
| Push storyteller state | On | Register rimmind_storyteller environment variable |
| Push memory data | Off | Register rimmind_memory variable |
| Push shaping history | Off | Register rimmind_shaping variable |
| Push advisor log | On | Register rimmind_advisor_log variable |
| Inject into traits context | Off | Append personality description to RimTalk Traits category (requires Push personality data) |
| Inject into mood context | Off | Append AI narrative to RimTalk Mood category (requires Push personality data) |

### Context Pull

| Setting | Default | Description |
|---------|---------|-------------|
| Enable context pull | On | Pull data from RimTalk and register as RimMind context providers |
| Pull RimTalk dialogue history | On | Inject RimTalk dialogue history into RimMind context |

## FAQ

**Q: Can I use this without RimTalk?**
A: Yes. This mod detects RimTalk via reflection and silently skips all bridge features when RimTalk is not installed.

**Q: Can I use this with only RimTalk and no other RimMind sub-modules?**
A: Yes, but context push and persona push features require the corresponding RimMind sub-modules (Personality, Memory, Advisor, Storyteller) to push data. Dialogue gate only requires RimMind-Dialogue.

**Q: Will dialogue gate cause no dialogues at all?**
A: No. The gate only skips RimMind's redundant triggers; RimTalk's dialogue system works normally. You can precisely control which trigger types to skip via settings.

**Q: Will pushed data consume too many tokens?**
A: Data volume is controllable. Each push category shows at most 5 records, and each can be independently toggled via settings. RimTalk's prompt template system also trims by priority.

**Q: What if RimTalk's API changes after an update?**
A: This mod calls RimTalk API via reflection with try-catch protection. API changes will only cause affected features to silently fail without crashing.

## Acknowledgments

This project references the following excellent RimWorld mods:

- [RimTalk](https://github.com/jlibrary/RimTalk.git) - Dialogue system & API design reference
- [RimTalk-ExpandActions](https://github.com/sanguodxj-byte/RimTalk-ExpandActions.git) - Action expansion reference

## Contributing

Issues and Pull Requests are welcome! If you have any suggestions or find bugs, please feedback via GitHub Issues.
