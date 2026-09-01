# VM Unity Pipeline

VM Unity Pipeline extends Unity Technologies' official Unity CLI and com.unity.pipeline package with contracts needed by token-efficient, project-safe automation.

The package adds no second Editor transport. Commands are discovered and executed by the official Pipeline server.

## Requirements

- Unity 6000.0 or newer.
- Unity CLI 1.0.0-beta.5.
- com.unity.pipeline 0.5.0-exp.1.

Both upstream components are prerelease software. This package pins the Pipeline dependency and reviews Unity CLI release notes before changing the supported CLI version.

## Installation

Install from an immutable Git revision:

    "com.vm233.unity-pipeline": "https://github.com/VM233/VMUnityPipeline.git#<full-commit-sha>"
    "com.vm233.unity-automation": "https://github.com/VM233/VMUnityAutomation.git#<full-commit-sha>"

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
for the user. Do not replace this shell with an alternate transport.

The official CLI surface contains only seven commands:

- vm_catalog_status
- vm_catalog_list
- vm_catalog_get
- vm_editor_state
- vm_remove_missing_scripts
- vm_job_status
- vm_automation_call

The automation catalog can contain hundreds of contracts without registering hundreds of
official CLI commands. `vm_catalog_list` remains capped at 50 and defaults to 10;
`vm_catalog_get` retrieves one full schema. `vm_automation_call` accepts the selected
identifier plus one JSON object and delegates to the transport-neutral automation owner.
If that identifier names a discovered but invalid or duplicate project tool, the facade
returns `invalid_project_tool` or `duplicate_project_tool` together with the exact
registration source and validation error.

The first three commands read immutable managed contract data on a background thread.
`vm_editor_state`, `vm_remove_missing_scripts`, and `vm_automation_call` execute on the
Unity main thread. `vm_remove_missing_scripts` owns one Undo-backed loaded-scene cleanup,
marks that scene dirty, and deliberately leaves persistence to a later `save_scene` call.
Mutating automation contracts require an exact absolute `expected_project_path`;
dangerous contracts additionally require `confirm=true` inside `arguments_json`.

`vm_job_status` is the intentionally separate background-safe polling boundary for durable
automation jobs. It reads the latest immutable published snapshot, so package imports,
compilation, and builds remain observable even while Unity's main thread cannot service
`vm_automation_call`. For a newly admitted workspace job, the first authorized status
read also publishes a durable client-adoption acknowledgement. The main-thread runner
must adopt that acknowledgement before it can mutate state or trigger Domain Reload.

Choose the wait boundary from the selected owner contract:

- A reload-resumable submission such as `asset/refresh`, `packages/update-git`,
  or the `play`/`stop` actions of `editor/play-mode` returns
  its own `jobId` and `jobAccessToken` immediately. Keep this short outer call attached so
  the durable token reaches the client, then poll the inner job with `vm_job_status`.
  That first authorized poll releases the admission-queued workspace job; continue polling
  until terminal. An outer detached job is intentionally in-memory and can be lost at
  domain reload.
- Package mutations require stable Edit Mode. Durable update/resolve jobs report
  `edit-mode-required` and resume after Play Mode exits; add/remove calls return a typed
  `edit_mode_required` error instead of starting a package request Unity cannot adopt.
- A genuinely long, non-durable main-thread call such as a VFX Graph transaction uses the
  official CLI's outer `--detach` flow and is collected with `unity job wait`.

The facade's `timeout_seconds` controls only an inner deferred automation owner; it does
not extend the CLI request timeout. For a long non-durable call:

```powershell
$submission = unity --json --no-banner --non-interactive command --detach `
  --project-path 'D:\UnityProjects\YourProject' vm_automation_call -- `
  --command vm_auto_vfxgraph_transaction --arguments_json $argumentsJson `
  --expected_project_path 'D:\UnityProjects\YourProject' | ConvertFrom-Json
$jobId = $submission.data.jobId
unity --json --no-banner --non-interactive job wait `
  --project-path 'D:\UnityProjects\YourProject' $jobId
```

For a durable submission, omit outer `--detach`, retain the returned inner token, and use
the background-safe poller. Its first authorized call acknowledges token delivery and
releases workspace execution:

```powershell
$submission = unity --json --no-banner --non-interactive command `
  --project-path 'D:\UnityProjects\YourProject' vm_automation_call -- `
  --command vm_auto_asset_refresh --arguments_json '{}' `
  --expected_project_path 'D:\UnityProjects\YourProject' | ConvertFrom-Json
$inner = $submission.data.result.result
unity --json --no-banner --non-interactive command `
  --project-path 'D:\UnityProjects\YourProject' vm_job_status -- `
  --job_id $inner.jobId --job_access_token $inner.jobAccessToken
```

## Output contract

Domain failures are returned inside the command result with `ok=false` and a stable error
code. Transport, CLI binding, readiness, parameter binding, and unexpected Pipeline
execution failures remain owned by the official Pipeline envelope.

Catalog list output is always sorted, paginated, and capped at 50 entries. The default page size is 10.

## Development

The catalog joins seven explicit Pipeline contracts with the bounded, deterministic
`VMUnityAutomation` catalog. Its revision is a SHA-256 digest of the full sorted contract
set. Automation routes are not expanded into `[CliCommand]` registrations.

Package code is compiled through a supported consuming project after publishing an immutable Git revision. Do not add a local UPM dependency for development.
