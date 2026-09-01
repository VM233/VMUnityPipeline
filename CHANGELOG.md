# Changelog

All notable changes to this package are documented here.

## [0.4.35] - 2026-09-01

### Fixed

- Use the package's runtime `RuntimePipelineManager` as the missing-script
  regression subject so Unity can attach it before the test clears its
  `m_Script`; remove the invalid Editor-only sentinel component.

## [0.4.34] - 2026-09-01

### Fixed

- Run the missing-script regression fixture in an isolated Editor preview
  scene addressed through the official instance-id ObjectRef, so an unsaved
  Test Runner scene cannot block setup or inherit test mutations.

## [0.4.33] - 2026-09-01

### Fixed

- Declare the official `Unity.Pipeline.Editor` assembly dependency required by
  `vm_remove_missing_scripts` for `ObjectResolver` instead of copying Pipeline
  object-reference resolution into the extension package.

## [0.4.32] - 2026-09-01

### Added

- Add `vm_remove_missing_scripts` as the typed, Undo-backed owner for removing
  missing MonoBehaviour slots from one loaded-scene GameObject. The command
  marks the scene dirty, leaves saving explicit, and returns stable domain
  errors for unstable Editor state and invalid targets.

## [0.4.31] - 2026-08-29

### Fixed

- Require VM Unity Automation 0.3.62 so clean-compilation evidence preserves
  dotted output identities and accepts Unity's missing per-assembly start and
  finish callbacks only with complete not-required terminal coverage.

## [0.4.30] - 2026-08-29

### Fixed

- Require VM Unity Automation 0.3.61 so durable clean compilation models
  Unity's UUM-95901 callback behavior with separate started, finished, and
  not-required assembly evidence instead of falsely reporting no compilation.

## [0.4.29] - 2026-08-29

### Fixed

- Require VM Unity Automation 0.3.60 so durable refresh, package-adoption,
  and code-asset transaction jobs can only report a clean compilation after
  every expected script assembly publishes completion evidence.

## [0.4.28] - 2026-08-22

### Fixed

- Require VM Unity Automation 0.3.41 so `vm_automation_call` reports a
  discovered project's invalid or duplicate tool registration and its exact
  validation error instead of returning `command_not_found`.
- Withdraw the 0.4.27 dynamic-catalog workaround after direct registry
  evidence showed the affected tool was already discovered and invalid.

## [0.4.27] - 2026-08-22

### Fixed

- Require VM Unity Automation 0.3.40 and rebuild the merged CLI contract
  catalog whenever the Automation catalog revision changes, so newly compiled
  or removed project tools become discoverable without restarting Unity.

## [0.4.26] - 2026-08-22

### Fixed

- Require VM Unity Automation 0.3.39 so durable Job starts retain their
  transport request identity and `vm_job_status --request_id` can recover the
  exact admission-queued Job without copying a wrapped token.

## [0.4.25] - 2026-08-22

### Changed

- Require VM Unity Automation 0.3.38 so VFX data-object transactions use the
  context-owned invalidation and subasset-aware publication path.

## [0.4.24] - 2026-08-22

### Changed

- Require VM Unity Automation 0.3.37 so VFX Graph contracts publish their
  route-specific stable domain error codes through the CLI catalog.

## [0.4.23] - 2026-08-22

### Changed

- Require VM Unity Automation 0.3.36 so VFX Graph data-object simulation
  spaces are inspectable and writable through the atomic Automation contract.

## [0.4.22] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.35 and document the durable admission
  handshake: reload-resumable workspace jobs stay queued until the first
  authorized `vm_job_status` read acknowledges that their token reached the
  client. This prevents Domain Reload from dropping the submission response.

## [0.4.21] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.23 so VFX Graph Block Activation Slots are
  exposed through the shared `$activation` selector for inspection and
  authoring.

## [0.4.20] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.22 so `component/move` publishes its actual
  loaded-scene Undo transaction and verification evidence.

## [0.4.19] - 2026-08-21

### Fixed

- Return the root Automation exception as the stable `command_exception`
  response instead of leaking an opaque Pipeline HTTP 500, even when the
  Automation catalog itself cannot initialize.
- Require VM Unity Automation 0.3.21 with generator-owned route audit
  fingerprints.

## [0.4.18] - 2026-08-21

### Added

- Require VM Unity Automation 0.3.20 so CLI callers can atomically move a
  component between loaded-scene GameObjects while preserving state and
  scene-local references.

## [0.4.17] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.19 so catalog error metadata distinguishes
  read-only tools from project-bound mutations and fully describes dynamic-code
  failures.

## [0.4.16] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.17 so already-satisfied Play Mode option
  requests do not rewrite or migrate `EditorSettings.asset`.

## [0.4.15] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.16 so dangerous command schemas expose their
  required acknowledgement and Play Mode option mutations verify durable
  `EditorSettings.asset` state instead of only the live Editor value.

## [0.4.14] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.15 so a reached Play/Stop target can finish
  stable-frame confirmation even when project initialization outlasted the
  pre-transition wall-clock timeout during Domain Reload.

## [0.4.13] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.14 so durable workspace contracts expose
  their structured errors, actual idempotency failures, and explicit
  admission-versus-completion evidence.
- Clarify that `vm_automation_call` completes the outer invocation while a
  returned durable Job still requires `vm_job_status` polling.

## [0.4.12] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.13 so `editor/play-mode` publishes durable
  play/stop jobs before Domain Reload, exposes their lifecycle through the
  background-safe poller, and lets `stop` unblock an Edit-Mode-dependent job.

## [0.4.11] - 2026-08-21

### Fixed

- Make catalog text queries separator-insensitive so callers can find a project
  tool by its route form such as `battle-sandbox`, its normalized CLI name such
  as `battle_sandbox`, or its human-readable description without guessing the
  internal separator convention.

## [0.4.10] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.12 so live Play Mode option changes follow
  Unity's documented `None` normalization when the feature is disabled and
  reject contradictory reload-skip requests with actionable guidance.

## [0.4.9] - 2026-08-21

### Fixed

- Require VM Unity Automation 0.3.11 with compile-safe package-mutation
  descriptions after the stable Edit Mode guidance was merged into the
  existing catalog cases.

## [0.4.8] - 2026-08-21

### Changed

- Require VM Unity Automation 0.3.10 so package-mutation discovery exposes its
  stable Edit Mode precondition, immediate mutations reject Play Mode clearly,
  and durable update/resolve jobs wait without crossing the mutation boundary.

## [0.4.7] - 2026-08-20

### Changed

- Require VM Unity Automation 0.3.9 so the bounded catalog exposes live
  Enter Play Mode Options configuration and complete Play Mode error metadata.

## [0.4.6] - 2026-08-20

### Fixed

- Distinguish reload-resumable inner-job submissions from long non-durable calls in
  command discovery and documentation. Durable submissions now explicitly stay attached
  until their job token is published, avoiding loss of the outer in-memory CLI job during
  a domain reload.

## [0.4.5] - 2026-08-20

### Changed

- Require VM Unity Automation 0.3.8 so prefab component commands can resolve
  assembly-qualified component type names without ambiguous cross-assembly fallback.

## [0.4.4] - 2026-08-20

### Added

- Add one background-safe `vm_job_status` command backed by Automation's immutable
  published snapshots, keeping durable package/import/build jobs observable while Unity's
  main thread is busy without expanding every automation route into a CLI command.

### Changed

- Require VM Unity Automation 0.3.7 for the thread-safe published-job read boundary.

## [0.4.3] - 2026-08-20

### Fixed

- Require VM Unity Automation 0.3.6 so durable Git package updates recover from a
  transient Package Manager cancellation and report actionable failure details.

## [0.4.2] - 2026-08-20

### Changed

- Guide long main-thread automation calls to the official Unity CLI detached-job flow in
  command discovery, argument metadata, and package documentation; clarify that
  `timeout_seconds` is an inner automation timeout rather than the outer CLI request bound.
- Require VM Unity Automation 0.3.5 with isolated VFX Graph dry-run transactions.

## [0.4.1] - 2026-08-20

### Fixed

- Require VM Unity Automation 0.3.1 so facade request identity remains outside
  strict owner argument objects unless the selected contract declares durable
  idempotency support.

## [0.4.0] - 2026-08-20

### Changed

- Require VM Unity Automation 0.3.0 and its transport-neutral public schema and
  debug-result names.

### Removed

- Delete the completed offline cutover ledger, generator, snapshots, and all
  remaining retired-transport names from current package documentation.

## [0.3.4] - 2026-08-20

### Fixed

- Read the live UPM package version for `vm_catalog_status` instead of
  maintaining a second hand-written version constant that could drift after a
  release.
- Run catalog status on the Unity main thread, where Package Manager metadata
  is authoritative and safe to query.

## [0.3.3] - 2026-08-20

### Added

- Reproducible 406-route predecessor migration ledger with a separate reviewed-decision source.
- Bounded snapshot of the 142 Unity-owned Pipeline 0.5.0-exp.1 commands.
- Compact ledger generator that rejects source drift and never prints the full ledger by default.
- Documented the verified warm NDJSON Agent session and consent-neutral startup.

### Fixed

- Require VM Unity Automation 0.2.4 after removing the last eleven retired
  transport-route metadata entries.
- Close all 406 migration decisions and validate the final 395 custom, 6 merged,
  and 5 deleted routes against the immutable Automation source revision.

## [0.3.2] - 2026-08-20

### Fixed

- Require VM Unity Automation 0.2.3 so the official CLI facade compiles only
  against the complete transport-neutral public contract surface.

## [0.3.1] - 2026-08-20

### Fixed

- Require VM Unity Automation 0.2.1 with the corrected split-contract compile
  surface.

## [0.3.0] - 2026-08-20

### Changed

- Require VM Unity Automation 0.2.0 and its transport-neutral public API after
  removing the retired route and type surface.

## [0.2.9] - 2026-08-20

### Fixed

- Consume VM Unity Automation 0.1.7 so bounded CLI package filters preserve the
  declared owner of framework and project-local tools.

## [0.2.8] - 2026-08-20

### Fixed

- Require VM Unity Automation 0.1.6 so background catalog commands resolve
  extension ownership without calling main-thread-only Package Manager APIs.

## [0.2.7] - 2026-08-20

### Fixed

- Preserve each project tool's owning UPM package or stable project module in
  bounded CLI discovery instead of attributing every extension to Automation.
- Require VM Unity Automation 0.1.5 for the package-ownership contract.

## [0.2.6] - 2026-08-20

### Fixed

- Keep the public catalog-status package version synchronized with the UPM
  manifest so live CLI evidence identifies the active release exactly.

## [0.2.5] - 2026-08-20

### Changed

- Require VM Unity Automation 0.1.4 so joined schemas use the transport-neutral
  `x-vmAutomationContract` data-product keyword.

## [0.2.4] - 2026-08-20

### Changed

- Require the thread-neutral Automation catalog identity contract.

## [0.2.3] - 2026-08-20

### Added

- Stable `catalog_initialization_failed` status output with the root catalog error instead
  of leaking a type-initializer exception through the official Pipeline envelope.

## [0.2.2] - 2026-08-20

### Changed

- Require the automation package revision with package-owned deterministic Unity GUIDs.

## [0.2.1] - 2026-08-20

### Fixed

- Declare the automation package by SemVer so Unity Package Manager can satisfy it
  from the consuming project's separately pinned immutable Git revision.

## [0.2.0] - 2026-08-20

### Added

- One `vm_automation_call` official CLI facade for the transport-neutral automation core.
- Bounded discovery of built-in automation and reflected project-tool contracts without
  registering hundreds of CLI commands.
- Full-fidelity owner input/output schemas and a SHA-256 revision of the joined catalog.
- Exact project binding, caller/request identity, timeout, and JSON argument forwarding.

## [0.1.0] - 2026-08-20

### Added

- Initial Unity CLI Pipeline package.
- Bounded rich-contract discovery through vm_catalog_status, vm_catalog_list, and vm_catalog_get.
- Explicit vm_editor_state command with separate play, pause, transition, compilation, and asset-update facts.
- Immutable command contracts containing input/output schema, stable domain errors, side effects, preconditions, completion evidence, and transaction metadata.
