# RimMind Bridge RimTalk compact contracts

Active contract sources are `Contracts/*.cs`. The compact suite contains:

- `RimTalkGateContextContracts` — dialogue gate, coordinator lifecycle and persona context formatting.
- `RimTalkSettingsCompatibilityContracts` — safe defaults, dirty tracking, detector cache and reflection API compatibility.

Expected discovery after cutover: 6 Facts, below the Bridge-RimTalk target of 36.

## Cutover handoff

- Active include: `Contracts/**/*.cs`
- Shared support include:

  ```xml
  <Compile Include="..\..\RimMind-Core\TestSupport\ContractCaseRunner.cs"
           Link="Support\ContractCaseRunner.cs" />
  ```

- Required retained stub include: `RimTalkStubs.cs` for Verse/Harmony and the
  optional Personality profile boundary only.
- Required production includes: `BridgeModuleCoordinator.cs`,
  `RimTalkContextPushPlan.cs`, `BridgeRimTalkSettings.State.cs`,
  `DialogueGate.cs`, `PersonaFormatter.cs`, `RimTalkApiShim.cs`, and
  `RimTalkDetector.cs`
- Legacy compile categories to remove from the project entry during cutover:
  coordinator base/extended matrices, dialogue gate base/extended matrices,
  bridge-stub matrices, persona formatter matrices, API shim matrices, detector
  cache matrices, settings/default/dirty matrices and source-shape architecture checks.

The active suite compiles the production settings state, registration
coordinator, context-push plan, detector and reflection shim. Module lifecycle
uses the same production coordinator with injected probes; one failing optional
module is verified not to block the others.

## Retired legacy tests

Files outside `Contracts/` are retained on disk but excluded from compilation.
Their behavior mapping is recorded in the root contract mapping document.
Deletion requires explicit owner approval for each exact file path; directories are never deleted.
