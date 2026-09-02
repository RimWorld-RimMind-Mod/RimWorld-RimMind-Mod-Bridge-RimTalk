# RimMind Bridge RimTalk contracts

The suite has two active contract groups:

- `RimTalkGateContextContracts` — dialogue gate, coordinator lifecycle, persona formatting, Core Provider reads and child-mod dependency boundaries.
- `RimTalkSettingsCompatibilityContracts` — safe defaults, dirty tracking, detector cache and reflection API compatibility.

The fixed test budget is 6 Facts. `RimTalkStubs.cs` supplies the minimal Verse,
Harmony and `RimMindAPI.Providers` seams used by those contracts.
