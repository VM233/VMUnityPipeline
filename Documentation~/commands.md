# Commands

## vm_catalog_status

Returns the package version, contract version, catalog revision, number of VM
extension commands, and `ownerCounts` keyed by their authoritative package owners.
The `invalidProjectTools` list preserves Automation's registration errors for
project tools excluded from the valid catalog. Require an empty list before
accepting registrations. This command runs on Unity's main thread.

## vm_catalog_list

Returns compact command summaries. Optional query, package, tag, and side-effect filters
combine with AND. Results are ordinally sorted by command name before filtering. Offset
must be non-negative; limit must be between 1 and 50.

Every summary includes the same immutable `invocation` mapping as its full contract.

## vm_catalog_get

Returns the complete rich contract for one exact catalog identifier. An unknown name
returns `ok=false` with `errorCode=command_not_found`.

`name` identifies the contract. `invocation.command` identifies its registered native
Unity CLI execution entry. Copy `invocation.arguments` as fixed native parameters. When
`invocation.argumentsJsonParameter` is present, serialize the owner arguments described
by `inputSchema` into one JSON object and supply that string in the named native parameter.
Otherwise supply those arguments as direct native command parameters.

For example, discovery of `vm_auto_prefab_asset_get_properties` returns:

```json
{"command":"vm_automation_call","arguments":{"command":"vm_auto_prefab_asset_get_properties"},"argumentsJsonParameter":"arguments_json"}
```

Native commands such as `vm_editor_state` publish their own registered name, empty fixed
arguments and no JSON parameter. Automation and project tools publish the facade mapping
from their registration origin, without inferring routing from prefixes or package names.
Keep the absolute `unity command --project-path` binding for every invocation. For facade
mutations, also supply the caller's project root as `expected_project_path` and observe
the selected owner contract's preconditions and confirmation requirements. Discovery does
not execute the owner or prove that its current prerequisites are satisfied. Durable
submissions still publish the separate `polling` instruction for their original job.

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

## vm_remove_missing_scripts

Removes every missing MonoBehaviour slot from one loaded-scene GameObject addressed by
the official Pipeline `ObjectRef` contract. The operation requires stable Edit Mode,
registers one Unity Undo snapshot, and marks the owning scene dirty. It never saves the
scene implicitly; call `save_scene` after checking `removedCount` and `sceneDirty`.

An already-clean GameObject succeeds with `removedCount=0`, so the command is idempotent.
Domain failures return `ok=false` with one of:

- `edit_mode_required`
- `editor_not_stable`
- `target_not_found`
- `target_not_game_object`
- `loaded_scene_target_required`

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

Automation owns comparison of `expected_project_path` with an optional JSON
`expectedProjectPath`. Equivalent normalized absolute paths are accepted and
fingerprinted consistently. Different roots return `argument_conflict`; a
consistent binding to another checkout returns `project_mismatch`. Relative paths
return `invalid_project_path`. All fail before owner side effects.

JSON strings retain their exact text, including timestamp precision and timezone offsets.
The facade does not infer dates or coerce string values before contract binding.

If the selected identifier names a project tool that was discovered but has an
invalid or duplicate registration, the command returns `invalid_project_tool`
or `duplicate_project_tool` with the exact registration source and validation
error instead of `command_not_found`.

`timeout_seconds` is the inner wait bound used by the automation facade. It cannot extend
the official CLI request timeout around this main-thread command. Keep reload-resumable
submission contracts attached until they return their own `jobId` and `jobAccessToken`,
then invoke the response's `polling.command` with `polling.arguments`, keeping the same
absolute project binding. The closed instruction selects `vm_job_status` and maps the
original job identity, optional capability and caller into its CLI parameters. Its first
authorized read releases the admission-queued workspace job; continue reading that same
job until terminal. Immediate owner products and domain failures omit `polling`.
The owner's `pollRoute` remains transport-neutral; it is not the CLI polling command.
An outer detached job does not survive a domain reload. Use
`unity command --detach` plus `unity job wait` only for genuinely long, non-durable
main-thread calls.

Package mutations require stable Edit Mode. Durable update/resolve jobs wait with the
`edit-mode-required` blocked reason until Play Mode exits; package add/remove calls fail
with the typed `edit_mode_required` error before starting Package Manager work.

# Catalog revision adoption

The facade reads Automation's current catalog revision before publishing or
looking up contracts. [Catalog publication](catalog-publication.md) describes
ownership, cost bounds and readiness-change verification.
