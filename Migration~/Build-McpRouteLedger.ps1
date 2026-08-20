[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$McpRepository,

    [string]$OfficialSnapshot =
        (Join-Path $PSScriptRoot 'official-pipeline-0.5.0-exp.1-command-snapshot.json'),

    [string]$DecisionFile =
        (Join-Path $PSScriptRoot 'mcp-route-decisions.tsv'),

    [string]$OutputPath,

    [string]$CheckPath,

    [string]$ExpectedMcpRevision = '3441d9e63486d51e3bdccf872cc1c5bcdd1ac23c'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedMcpRepository = (Resolve-Path -LiteralPath $McpRepository).Path
$resolvedOfficialSnapshot = (Resolve-Path -LiteralPath $OfficialSnapshot).Path
$actualMcpRevision = (& git -C $resolvedMcpRepository rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Could not read the VMUnityMCP revision at '$resolvedMcpRepository'."
}
if ($actualMcpRevision -ne $ExpectedMcpRevision) {
    throw "VMUnityMCP revision drifted. Expected '$ExpectedMcpRevision', actual '$actualMcpRevision'."
}

$snapshot = Get-Content -Raw -LiteralPath $resolvedOfficialSnapshot | ConvertFrom-Json
if ($snapshot.pipelinePackageVersion -ne '0.5.0-exp.1') {
    throw "Unexpected Pipeline snapshot version '$($snapshot.pipelinePackageVersion)'."
}

$officialNames = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)
foreach ($command in $snapshot.commands) {
    if (-not $officialNames.Add([string]$command.name)) {
        throw "Duplicate official command '$($command.name)' in the snapshot."
    }
}
if ($officialNames.Count -ne [int]$snapshot.officialCommandCount) {
    throw 'The official command snapshot count does not match its command array.'
}

$officialCandidates = @{
    'animation/add-layer' = 'add_animator_layer'
    'animation/add-parameter' = 'add_animator_parameter'
    'animation/add-state' = 'add_animator_state'
    'animation/add-transition' = 'add_animator_transition'
    'animation/clip-info' = 'get_animation_clip'
    'animation/controller-info' = 'get_animator_controller'
    'animation/create-clip' = 'create_animation_clip'
    'animation/create-controller' = 'create_animator_controller'
    'animation/remove-curve' = 'remove_animation_curve'
    'animation/set-clip-curve' = 'set_animation_curve'
    'asset/copy' = 'copy_asset'
    'asset/create-folder' = 'create_folder'
    'asset/create-material' = 'create_asset'
    'asset/create-prefab' = 'create_prefab'
    'asset/delete' = 'delete_asset'
    'asset/import' = 'import_asset'
    'asset/import-settings/get' = 'get_import_settings'
    'asset/import-settings/set' = 'set_import_settings'
    'asset/list' = 'find_assets'
    'asset/move' = 'move_asset'
    'asset/rename' = 'rename_asset'
    'build/get-job' = 'build_status'
    'build/start' = 'build'
    'compilation/errors' = 'get_console_logs'
    'component/add' = 'add_component'
    'component/get-properties' = 'get_component_properties'
    'component/remove' = 'remove_component'
    'component/set-property' = 'set_component_properties'
    'component/set-reference' = 'set_component_properties'
    'console/clear' = 'clear_console'
    'console/query' = 'console'
    'editor/execute-code' = 'eval'
    'editor/execute-menu-item' = 'menu'
    'gameobject/create' = 'create_gameobject'
    'gameobject/delete' = 'delete_gameobject'
    'gameobject/info' = 'find_gameobjects'
    'gameobject/reparent' = 'set_parent'
    'gameobject/set-active' = 'set_active'
    'gameobject/set-transform' = 'set_transform'
    'material/properties/get' = 'get_material_properties'
    'material/properties/set' = 'set_material_properties'
    'navigation/bake' = 'bake_navmesh'
    'navigation/clear' = 'clear_navmesh'
    'navigation/info' = 'get_navmesh_settings'
    'packages/add' = 'package_add'
    'packages/list' = 'package_list'
    'packages/remove' = 'package_remove'
    'packages/resolve' = 'package_resolve'
    'packages/search' = 'package_search'
    'packages/status' = 'package_status'
    'prefab/apply-overrides' = 'apply_prefab_overrides'
    'prefab/create-variant' = 'create_prefab_variant'
    'prefab/revert-overrides' = 'revert_prefab_overrides'
    'prefab/unpack' = 'unpack_prefab'
    'scene/hierarchy' = 'get_scene_hierarchy'
    'scene/instantiate-prefab' = 'instantiate_prefab'
    'scene/new' = 'create_scene'
    'scene/open' = 'open_scene'
    'scene/save' = 'save_scene'
    'screenshot/game' = 'capture_game_view'
    'screenshot/scene' = 'capture_scene_view'
    'script/create' = 'create_script'
    'script/read' = 'read_text_file'
    'script/update' = 'write_text_file'
    'selection/get' = 'get_selection'
    'selection/set' = 'set_selection'
    'settings/physics' = 'get_physics_settings'
    'settings/player' = 'get_player_settings'
    'settings/quality' = 'get_quality_settings'
    'settings/set-physics' = 'set_physics_settings'
    'settings/set-player' = 'set_player_settings'
    'settings/set-time' = 'set_time_settings'
    'settings/time' = 'get_time_settings'
    'testing/get-job' = 'test_status'
    'testing/list-tests' = 'list_tests'
    'testing/run-tests' = 'run_tests'
}

$mergeCandidates = @{
    '_meta/capabilities' = 'vm_catalog_status'
    '_meta/tools' = 'unity command discovery|vm_catalog_list|vm_catalog_get'
    'build/profile' = 'get_build_settings|list_build_profiles|set_build_settings'
    'editor/play-mode' = 'editor_play|editor_pause|editor_stop'
    'editor/state' = 'editor_status|vm_editor_state'
    'instance/assert-project' = 'unity command --project-path|vm_editor_state'
    'instance/current' = 'unity status|vm_editor_state'
    'instance/list' = 'unity status'
    'instance/resolve' = 'unity command --project-path'
    'packages/info' = 'package_list|package_status'
    'screenshot/editor-window' = 'screenshot|capture_scene_view|capture_game_view'
    'taglayer/add-tag' = 'get_tags_layers|set_tags_layers'
    'taglayer/info' = 'get_tags_layers'
    'taglayer/set-layer' = 'set_layer|set_tags_layers'
    'taglayer/set-static' = 'set_component_properties'
    'taglayer/set-tag' = 'set_tag|set_tags_layers'
    'testing/get-package-job' = 'test_status'
    'testing/run-package-tests' = 'run_tests'
    'timeline/info' = 'get_timeline'
    'timeline/transaction' = 'create_timeline|add_timeline_track|add_timeline_clip'
}

$retireCandidates = @{
    'agents/list' = 'Unity CLI client owns process/session inventory'
    'agents/log' = 'Unity CLI stderr and artifact logs replace MCP agent logs'
    'mcp/health' = 'unity status and Pipeline readiness replace MCP health'
    'mcp/set-autostart' = 'MCP server lifecycle is removed at cutover'
    'ping' = 'unity status supplies transport liveness'
}

foreach ($entry in $officialCandidates.GetEnumerator()) {
    if (-not $officialNames.Contains([string]$entry.Value)) {
        throw "Official candidate '$($entry.Value)' for '$($entry.Key)' is absent from the snapshot."
    }
}

$allowedFinalDispositions = [System.Collections.Generic.HashSet[string]]::new(
    [string[]]@(
        'official_exact',
        'custom_cli',
        'merge_into',
        'delete_redundant',
        'blocked'),
    [System.StringComparer]::Ordinal)
$decisionsByRoute = @{}
if (Test-Path -LiteralPath $DecisionFile) {
    foreach ($decision in @(Import-Csv -Delimiter "`t" -LiteralPath $DecisionFile)) {
        $decisionRoute = [string]$decision.sourceRoute
        $finalDisposition = [string]$decision.finalDisposition
        $finalTarget = [string]$decision.finalTarget
        $evidence = [string]$decision.evidence
        if ([string]::IsNullOrWhiteSpace($decisionRoute)) {
            throw 'A route decision has no sourceRoute.'
        }
        if (-not $allowedFinalDispositions.Contains($finalDisposition)) {
            throw "Route '$decisionRoute' has invalid finalDisposition '$finalDisposition'."
        }
        if ($finalDisposition -in @('official_exact', 'custom_cli', 'merge_into') -and
            [string]::IsNullOrWhiteSpace($finalTarget)) {
            throw "Route '$decisionRoute' requires a finalTarget for '$finalDisposition'."
        }
        if ($finalDisposition -eq 'official_exact' -and
            -not $officialNames.Contains($finalTarget)) {
            throw "Route '$decisionRoute' targets unknown official command '$finalTarget'."
        }
        if ($finalDisposition -eq 'delete_redundant' -and
            -not [string]::IsNullOrWhiteSpace($finalTarget)) {
            throw "Deleted route '$decisionRoute' must not declare a finalTarget."
        }
        if ([string]::IsNullOrWhiteSpace($evidence)) {
            throw "Route '$decisionRoute' requires review evidence."
        }
        if ($decisionsByRoute.ContainsKey($decisionRoute)) {
            throw "Duplicate reviewed decision for '$decisionRoute'."
        }
        $decisionsByRoute.Add($decisionRoute, $decision)
    }
}

$routePattern = [regex]'Create(?<kind>Immediate|Deferred)\("(?<route>[^"]+)"'
$routeRows = [System.Collections.Generic.List[object]]::new()
$editorRoot = Join-Path $resolvedMcpRepository 'Editor'
foreach ($file in Get-ChildItem -Recurse -File -LiteralPath $editorRoot -Filter '*.cs') {
    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $file.FullName) {
        $lineNumber++
        $match = $routePattern.Match($line)
        if (-not $match.Success) {
            continue
        }

        $route = $match.Groups['route'].Value
        $executionKind = $match.Groups['kind'].Value.ToLowerInvariant()
        $candidateKind = 'custom_cli_review'
        $candidateTarget = 'vm_' + (($route -replace '[/\-]', '_') -replace '^_+', '')
        $candidateOwner = 'com.vm233.unity-pipeline'
        $reviewState = 'pending_contract_migration'
        $reviewNote = 'Create a typed Pipeline command that calls the production owner directly; do not call the MCP dispatcher.'

        if ($retireCandidates.ContainsKey($route)) {
            $candidateKind = 'retire_candidate'
            $candidateTarget = ''
            $candidateOwner = 'none_after_cutover'
            $reviewState = 'pending_consumer_zero_proof'
            $reviewNote = [string]$retireCandidates[$route]
        }
        elseif ($mergeCandidates.ContainsKey($route)) {
            $candidateKind = 'merge_candidate'
            $candidateTarget = [string]$mergeCandidates[$route]
            $candidateOwner = 'official_and_or_vm_pipeline'
            $reviewState = 'pending_semantic_review'
            $reviewNote = 'Prove combined input, output, error, effect, lifecycle, and consumer equivalence before retiring the route.'
        }
        elseif ($officialCandidates.ContainsKey($route)) {
            $candidateKind = 'official_candidate'
            $candidateTarget = [string]$officialCandidates[$route]
            $candidateOwner = 'com.unity.pipeline'
            $reviewState = 'pending_semantic_review'
            $reviewNote = 'Name/action match only; prove input, output, error, side-effect, transaction, lifecycle, and consumer parity.'
        }

        $finalDisposition = 'pending'
        $finalTarget = ''
        $evidence = ''
        if ($decisionsByRoute.ContainsKey($route)) {
            $decision = $decisionsByRoute[$route]
            $finalDisposition = [string]$decision.finalDisposition
            $finalTarget = [string]$decision.finalTarget
            $evidence = [string]$decision.evidence
            $reviewState = if ($finalDisposition -eq 'blocked') { 'blocked' } else { 'reviewed' }
        }

        $relativeSource = [System.IO.Path]::GetRelativePath(
            $resolvedMcpRepository,
            $file.FullName) -replace '\\', '/'
        $routeRows.Add([pscustomobject][ordered]@{
            sourceRoute = $route
            executionKind = $executionKind
            sourceOwner = 'com.vm233.unity-mcp'
            sourceRevision = $actualMcpRevision
            sourceFile = $relativeSource
            sourceLine = $lineNumber
            candidateKind = $candidateKind
            candidateTarget = $candidateTarget
            candidateOwner = $candidateOwner
            reviewState = $reviewState
            finalDisposition = $finalDisposition
            finalTarget = $finalTarget
            evidence = $evidence
            reviewNote = $reviewNote
        })
    }
}

$orderedRows = @($routeRows | Sort-Object sourceRoute)
if ($orderedRows.Count -ne 406) {
    throw "Expected 406 VMUnityMCP routes, found $($orderedRows.Count)."
}
$duplicates = @($orderedRows | Group-Object sourceRoute | Where-Object Count -gt 1)
if ($duplicates.Count -ne 0) {
    throw "Duplicate source route '$($duplicates[0].Name)'."
}
if (@($orderedRows | Where-Object { [string]::IsNullOrWhiteSpace($_.candidateKind) }).Count -ne 0) {
    throw 'At least one route has no migration candidate.'
}
$unknownDecision = @($decisionsByRoute.Keys | Where-Object { $_ -notin $orderedRows.sourceRoute })
if ($unknownDecision.Count -ne 0) {
    throw "Decision references unknown source route '$($unknownDecision[0])'."
}

$serializedLedger =
    (($orderedRows | ConvertTo-Csv -Delimiter "`t" -NoTypeInformation) -join "`n") + "`n"
if (-not [string]::IsNullOrWhiteSpace($OutputPath) -and
    -not [string]::IsNullOrWhiteSpace($CheckPath)) {
    throw 'Specify OutputPath or CheckPath, not both.'
}

$operation = 'summary'
$artifactPath = ''
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $artifactPath = [System.IO.Path]::GetFullPath($OutputPath)
    $parentDirectory = [System.IO.Path]::GetDirectoryName($artifactPath)
    if (-not [System.IO.Directory]::Exists($parentDirectory)) {
        throw "Output directory '$parentDirectory' does not exist."
    }
    [System.IO.File]::WriteAllText(
        $artifactPath,
        $serializedLedger,
        [System.Text.UTF8Encoding]::new($false))
    $operation = 'written'
}
elseif (-not [string]::IsNullOrWhiteSpace($CheckPath)) {
    $artifactPath = (Resolve-Path -LiteralPath $CheckPath).Path
    $checkedInLedger =
        [System.IO.File]::ReadAllText($artifactPath).Replace("`r`n", "`n")
    if ($checkedInLedger -ne $serializedLedger) {
        throw "Generated ledger differs from '$artifactPath'."
    }
    $operation = 'verified'
}

[pscustomobject][ordered]@{
    operation = $operation
    routeCount = $orderedRows.Count
    immediateCount = @($orderedRows | Where-Object executionKind -eq 'immediate').Count
    deferredCount = @($orderedRows | Where-Object executionKind -eq 'deferred').Count
    officialCandidateCount = @($orderedRows | Where-Object candidateKind -eq 'official_candidate').Count
    mergeCandidateCount = @($orderedRows | Where-Object candidateKind -eq 'merge_candidate').Count
    retireCandidateCount = @($orderedRows | Where-Object candidateKind -eq 'retire_candidate').Count
    customCliReviewCount = @($orderedRows | Where-Object candidateKind -eq 'custom_cli_review').Count
    reviewedCount = @($orderedRows | Where-Object reviewState -eq 'reviewed').Count
    blockedCount = @($orderedRows | Where-Object reviewState -eq 'blocked').Count
    pendingCount = @($orderedRows | Where-Object finalDisposition -eq 'pending').Count
    artifactPath = $artifactPath
}
