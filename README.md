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

    "com.vm233.unity-pipeline": "https://github.com/VM233/VMUnityPipeline.git#ec3eb011934701b64aeab397bf998ea8549575ea"
    "com.vm233.unity-automation": "https://github.com/VM233/VMUnityAutomation.git#4e3732577d8b9889031241554a9116483c1262bd"

Both entries are direct project dependencies. Pipeline declares the compatible Automation
SemVer, while the project owns the immutable Git selection. Local file dependencies and
embedded package overrides are not supported.

## Discovery flow

Do not run an unbounded full command listing during normal Agent work.

1. Run a compact official query with a small limit.
2. Call vm_catalog_list only when searching VM extension contracts; keep its page small.
3. Call vm_catalog_get for one exact command.
4. Execute direct Pipeline commands normally, or pass one discovered `vm_auto_` / `vm_pt_`
   contract to `vm_automation_call`.

## Warm agent session

Use one `unity shell --protocol ndjson` process for repeated Agent calls. Set
`UNITY_PROJECT_PATH` to the exact absolute checkout before starting the process;
do not select an Editor by project name. A caller writes one JSON request per
line and reads exactly one correlated response per line.

```powershell
$env:UNITY_PROJECT_PATH = 'D:\UnityProjects\YourProject'
$env:UNITY_NO_CONSENT_PROMPT = '1'
unity --non-interactive --no-banner shell --protocol ndjson
```

```json
{"id":"1","argv":["command","vm_catalog_list","--query","editor","--limit","2","--format","json"]}
```

`UNITY_NO_CONSENT_PROMPT` suppresses the first-run analytics question without
recording either an opt-in or opt-out. The Agent must not choose that preference
for the user. Do not replace this shell with `unity mcp`.

The official CLI surface contains only five commands:

- vm_catalog_status
- vm_catalog_list
- vm_catalog_get
- vm_editor_state
- vm_automation_call

The automation catalog can contain hundreds of contracts without registering hundreds of
official CLI commands. `vm_catalog_list` remains capped at 50 and defaults to 10;
`vm_catalog_get` retrieves one full schema. `vm_automation_call` accepts the selected
identifier plus one JSON object and delegates to the transport-neutral automation owner.

The first three commands read immutable managed contract data on a background thread.
`vm_editor_state` and `vm_automation_call` execute on the Unity main thread. Mutating
automation contracts require an exact absolute `expected_project_path`; dangerous
contracts additionally require `confirm=true` inside `arguments_json`.

## Output contract

Domain failures are returned inside the command result with `ok=false` and a stable error
code. Transport, CLI binding, readiness, parameter binding, and unexpected Pipeline
execution failures remain owned by the official Pipeline envelope.

Catalog list output is always sorted, paginated, and capped at 50 entries. The default page size is 10.

## Development

The catalog joins five explicit Pipeline contracts with the bounded, deterministic
`VMUnityAutomation` catalog. Its revision is a SHA-256 digest of the full sorted contract
set. Automation routes are not expanded into `[CliCommand]` registrations.

Package code is compiled through a supported consuming project after publishing an immutable Git revision. Do not add a local UPM dependency for development.

The offline `Migration~` directory tracks the MCP-to-CLI route inventory and
reviewed cutover decisions. Its generator returns only compact counts by
default; full catalogs remain file artifacts rather than normal Agent output.
