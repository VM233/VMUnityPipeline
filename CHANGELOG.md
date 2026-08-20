# Changelog

All notable changes to this package are documented here.

## [Unreleased]

### Added

- Reproducible 406-route VMUnityMCP migration ledger with a separate reviewed-decision source.
- Bounded snapshot of the 142 Unity-owned Pipeline 0.5.0-exp.1 commands.
- Compact ledger generator that rejects source drift and never prints the full ledger by default.
- Documented the verified warm NDJSON Agent session and consent-neutral startup.

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
