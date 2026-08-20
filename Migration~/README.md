# MCP to Unity CLI migration ledger

This directory owns the reproducible migration inventory for the old
`com.vm233.unity-mcp` built-in routes. It is not runtime package content; Unity
ignores the `Migration~` directory.

## Frozen inputs

- VMUnityMCP revision: `3441d9e63486d51e3bdccf872cc1c5bcdd1ac23c`.
- VMUnityAutomation revision: `bfc612c350fbc83b37fd33b324670bd7dec7f447`
  (package 0.2.4).
- Unity CLI: `1.0.0-beta.5`.
- `com.unity.pipeline`: `0.5.0-exp.1`.
- Live official command snapshot: 142 Unity-owned commands. The live catalog
  contained 146 entries after the four VM foundation commands were installed;
  those four are deliberately excluded from the official snapshot.
- Source inventory: 406 executable built-in route descriptors, including the
  two internal `_meta` routes; 387 are immediate and 19 are deferred.

The official snapshot was fetched in three bounded pages of at most 50 entries.
Normal discovery must continue to use a small `query` plus `limit`; this file is
an offline migration artifact, not a reason to print the full catalog into an
Agent conversation.

## Reviewed cutover

The historical candidate classification remains in the generated ledger for
auditability. Every route now also has a reviewed final decision:

| Final disposition | Count | Cutover owner |
| --- | ---: | --- |
| `custom_cli` | 395 | The transport-neutral VMUnityAutomation route, discovered progressively and invoked through `vm_automation_call`. |
| `merge_into` | 6 | Official Unity CLI binding/status plus the five-command VM Pipeline facade. |
| `delete_redundant` | 5 | Retired transport-only agent, health, autostart, and ping routes with no post-cutover consumer. |

The earlier candidate split was:

| Candidate kind | Count | Meaning |
| --- | ---: | --- |
| `official_candidate` | 76 | A Unity command has a plausible action match; full semantic parity is not proven. |
| `merge_candidate` | 20 | One old route likely maps to multiple official/VM commands or a new combined workflow. |
| `retire_candidate` | 5 | The route appears specific to the MCP transport; zero-consumer proof is still required. |
| `custom_cli_review` | 305 | The capability needs a typed VM Pipeline command unless later review finds a better owner. |

Candidate classification did not choose the final owner. The reviewed decision
file and Automation contract validation do.

## Files and ownership

- `official-pipeline-0.5.0-exp.1-command-snapshot.json` is the bounded live
  Unity-owned command snapshot used to validate official targets.
- `Build-McpRouteLedger.ps1` is the sole mechanical inventory owner. It rejects
  source revision drift, missing official targets, duplicate routes, unknown
  decisions, Automation contract drift, any route count other than 406, and
  any final split other than 395 custom, 6 merged, and 5 deleted routes.
- `mcp-route-decisions.tsv` is the human-reviewed decision source. Do not edit
  generated final fields directly in the ledger.
- `mcp-route-ledger.tsv` is generated and must exactly match the script output.

Regenerate from the pinned source checkout:

```powershell
./Migration~/Build-McpRouteLedger.ps1 `
    -McpRepository D:\UnityProjects\VMUnityMCP `
    -AutomationRepository D:\UnityProjects\VMUnityAutomation `
    -OutputPath Migration~/mcp-route-ledger.tsv
```

Verify without rewriting or printing the ledger:

```powershell
./Migration~/Build-McpRouteLedger.ps1 `
    -McpRepository D:\UnityProjects\VMUnityMCP `
    -AutomationRepository D:\UnityProjects\VMUnityAutomation `
    -CheckPath Migration~/mcp-route-ledger.tsv
```

Without either path argument the generator returns the same compact count
summary and never writes or prints the 406-row ledger.

## Review rule

Each reviewed route must choose exactly one final disposition:

- `official_exact`
- `custom_cli`
- `merge_into`
- `delete_redundant`
- `blocked`

For `official_exact`, compare the exact old input/output schemas, stable errors,
side effects, transaction guarantees, main-thread and reload behavior, handler
owner, and every consumer against one exact official command. Name similarity is
insufficient.

For `custom_cli`, the new command calls the production owner directly. Calling
`MCPBuiltInRouteDispatcher`, an MCP command class, or the MCP HTTP bridge is not
a migration.

For `merge_into` and `delete_redundant`, decision evidence must identify the
consumer rewrite or zero-consumer proof. The generator rejects an evidence-free
decision.
