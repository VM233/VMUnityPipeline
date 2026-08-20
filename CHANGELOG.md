# Changelog

All notable changes to this package are documented here.

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
