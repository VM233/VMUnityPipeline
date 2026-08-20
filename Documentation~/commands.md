# Commands

## vm_catalog_status

Returns the package version, contract version, catalog revision, and number of VM extension commands in the current Domain.

## vm_catalog_list

Returns compact command summaries. Optional query, package, and tag filters combine with AND. Results are ordinally sorted by command name before filtering. Offset must be non-negative; limit must be between 1 and 50.

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
