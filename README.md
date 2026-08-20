# VM Unity Pipeline

VM Unity Pipeline extends Unity Technologies' official Unity CLI and com.unity.pipeline package with contracts needed by token-efficient, project-safe automation.

The package does not run an MCP server and does not add a second Editor transport. Commands are discovered and executed by the official Pipeline server.

## Requirements

- Unity 6000.0 or newer.
- Unity CLI 1.0.0-beta.5.
- com.unity.pipeline 0.5.0-exp.1.

Both upstream components are prerelease software. This package pins the Pipeline dependency and reviews Unity CLI release notes before changing the supported CLI version.

## Installation

Install from an immutable Git revision:

    "com.vm233.unity-pipeline": "https://github.com/VM233/VMUnityPipeline.git#<full-commit-sha>"

Local file dependencies and embedded package overrides are not supported.

## Discovery flow

Do not run an unbounded full command listing during normal Agent work.

1. Run a compact official query with a small limit.
2. Call vm_catalog_list only when searching VM extension contracts.
3. Call vm_catalog_get for one exact command.
4. Execute that command.

The initial 0.1.0 surface contains:

- vm_catalog_status
- vm_catalog_list
- vm_catalog_get
- vm_editor_state

The first three commands read immutable managed contract data on a background thread. vm_editor_state executes on the Unity main thread and returns explicit play, pause, transition, compilation, asset-update, scene, platform, Unity-version, and absolute-project-path facts.

## Output contract

Domain failures are returned inside the command result with ok=false and a stable errorCode. Transport, binding, readiness, parameter-binding, and unexpected execution failures remain owned by the official Pipeline envelope.

Catalog list output is always sorted, paginated, and capped at 50 entries. The default page size is 10.

## Development

The bootstrap catalog is an explicit immutable list of four commands. Before bulk route migration, a generator must become the owner of the catalog list and contract revision so hundreds of command registrations cannot drift.

Package code is compiled through a supported consuming project after publishing an immutable Git revision. Do not add a local UPM dependency for development.
