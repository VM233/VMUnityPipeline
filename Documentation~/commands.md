# Commands

## vm_catalog_status

Returns the package version, contract version, catalog revision, and number of VM extension commands in the current Domain.

## vm_catalog_list

Returns compact command summaries. Optional query, package, tag, and side-effect filters
combine with AND. Results are ordinally sorted by command name before filtering. Offset
must be non-negative; limit must be between 1 and 50.

## vm_catalog_get

Returns the complete rich contract for one exact command name. An unknown name returns ok=false with errorCode command_not_found.

## vm_editor_state

Returns:

- isIdle
- isPlaying
- isPaused
- isCompiling
- isUpdating
- isChangingPlayMode
- isPlayingOrWillChangePlaymode
- activeScene
- activeScenePath
- sceneDirty
- unityVersion
- platform
- projectPath

isChangingPlayMode is derived from Unity's two authoritative EditorApplication facts:

    isPlayingOrWillChangePlaymode != isPlaying

The command is main-thread-only. It does not enter or exit Play Mode and does not modify project state.

## vm_job_status

Reads the latest immutable published snapshot for one durable VM automation job by
`job_id` or `request_id`. Supply the returned `job_access_token` when the original caller
identity is unavailable. This command runs off the Unity main thread, so it remains usable
while package import, compilation, build, or another long Editor operation is blocking the
main-thread automation facade. For a newly admitted workspace job, the first authorized
read durably acknowledges that its token reached the client and releases it for main-thread
execution; subsequent reads are observational.

## vm_automation_call

Executes one exact `vm_auto_` or `vm_pt_` contract, or one exact automation route, through
the transport-neutral owner. `arguments_json` must be one JSON object. Mutations require
the connected checkout's exact absolute path via `expected_project_path`; dangerous
contracts require `confirm=true` in the JSON object. Request IDs are idempotent inside the
current Editor domain, while reload-resumable owners publish durable job state.

If the selected identifier names a project tool that was discovered but has an
invalid or duplicate registration, the command returns `invalid_project_tool`
or `duplicate_project_tool` with the exact registration source and validation
error instead of `command_not_found`.

`timeout_seconds` is the inner wait bound used by the automation facade. It cannot extend
the official CLI request timeout around this main-thread command. Keep reload-resumable
submission contracts attached until they return their own `jobId` and `jobAccessToken`,
then call `vm_job_status` once to release the admission-queued workspace job and continue
polling it until terminal. An outer detached job does not survive a domain reload. Use
`unity command --detach` plus `unity job wait` only for genuinely long, non-durable
main-thread calls.

Package mutations require stable Edit Mode. Durable update/resolve jobs wait with the
`edit-mode-required` blocked reason until Play Mode exits; package add/remove calls fail
with the typed `edit_mode_required` error before starting Package Manager work.
