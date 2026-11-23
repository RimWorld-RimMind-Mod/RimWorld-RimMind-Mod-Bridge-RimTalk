# RimMind-Bridge-RimTalk Tests

Active contract suites live in `Contracts/*.cs` (6 xUnit Facts total):

- `RimTalkGateContextContracts` — dialogue gate, coordinator lifecycle, persona formatting, Core Provider reads (incl. null-pawn & missing-provider silence), context push/pull registration (real enum names & `IsRegistered` boundaries), and child-mod dependency isolation.
- `RimTalkSettingsCompatibilityContracts` — safe defaults, push plan filtering, detector cache invalidation, and `RimTalkApiShim` signature/enum/missing-nested-type contracts.

`RimTalkStubs.cs` supplies only external Verse/Harmony/Core/RimTalk seams (`HookOperation.Append`, `PromptRole`, `PromptPosition`, `FakeTalkRole`) needed to exercise production bridge sources on `net10.0` (`LangVersion 9.0`).

## Run

```powershell
dotnet test RimMind-Bridge-RimTalk/Tests/RimMindBridgeRimTalk.Tests.csproj -c Release
```
