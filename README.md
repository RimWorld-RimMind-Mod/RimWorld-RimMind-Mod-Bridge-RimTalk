<div align="center">

# RimMind-Bridge-RimTalk 🎨
### Speech Bubble Pipeline Harmonization & Cognitive Bridge for RimWorld 1.6

**English** | [简体中文](README_zh.md)

<p>
  <a href="https://rimworldgame.com/"><img src="https://img.shields.io/badge/RimWorld-1.6-brightgreen.svg" alt="RimWorld 1.6"></a>
  <a href="https://github.com/mcocdaa/RimWorld-RimMind-Mod-Core"><img src="https://img.shields.io/badge/Dependency-RimMind--Core-blue.svg" alt="Dependency: RimMind-Core"></a>
  <a href="#"><img src="https://img.shields.io/badge/Unit%20Tests-6%2B%20Passing-success.svg" alt="Unit Tests"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
</p>

<p><em>Harmonize overhead dialogue bubbles and inject RimMind's deep personality into RimTalk.</em></p>

</div>

---

## 📖 Overview

**RimMind-Bridge-RimTalk** provides an aesthetic rendering bridge between **RimMind** and the renowned **RimTalk** dialogue mod. Rather than having two competing speech bubble renderers clash on screen, this bridge unifies the visual pipeline and injects RimMind's Big-Five personality profiles and episodic memories into RimTalk prompts.

### Key Capabilities
- **Visual Bubble Harmonization**: Shares the overhead mote rendering pipeline, preventing bubble flickering, text clipping, and positioning collisions.
- **Cognitive Prompt Injection**: Dynamically injects RimMind's multi-tier memories and Big-Five traits into RimTalk conversations for deeper, more characterful dialogue.
- **Mutual Cooldown Management**: Coordinates interaction timing between both mod engines to prevent chat spam.

---

## 🎮 In-Game Showcase

![RimMind-Bridge-RimTalk Showcase](docs/images/showcase.jpg)
*Aesthetic speech bubble rendering: Colonists conversing with RimTalk visual motes powered by RimMind's personality and memory context.*

---

## 🏛️ Pipeline Architecture

```mermaid
flowchart TD
    Mind["RimMind Cognitive Engine (Personality & Memory)"] --> Bridge["RimMind-Bridge-RimTalk"]
    Bridge --> Inject["Inject Context into RimTalk Prompt"]
    Inject --> RimTalk["RimTalk Processing Engine"]
    RimTalk --> BubblePipeline["Unified Overhead Bubble Pipeline"]
    BubblePipeline --> Screen["Flicker-Free Overhead Mote on Screen"]
```

---

## 🛠️ Installation & Load Order

```text
1. Harmony
2. Core (Vanilla RimWorld)
3. RimTalk (Optional third-party mod)
4. RimMind-Core
5. RimMind-Bridge-RimTalk
```

---

## 🧪 Developer Guide & Testing

Run unit tests directly:

```powershell
dotnet test RimMind-Bridge-RimTalk/Tests/RimMindBridgeRimTalk.Tests.csproj -c Release
```

---

## 📜 License

Licensed under the [MIT License](LICENSE).
