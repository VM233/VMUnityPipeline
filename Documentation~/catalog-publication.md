# Automation catalog adoption

The facade adopts Automation's revision before reading contracts. A changed
revision replaces the complete sorted contract list, name lookup and facade
revision together. Domain initialization does not freeze optional-package
availability. All top-level commands and the official transport remain unchanged.

## Static Cost Ledger (before implementation)

The consuming witness has fewer than 900 Automation contracts, seven facade
contracts and pages of 50. Each access checks one Automation revision. When that
revision changes, at most 18 pages, 907 dictionary insertions and a single revision
serialization build one new snapshot on the main thread. Unchanged revisions
reuse the complete snapshot. At most the old and new snapshots coexist during
publication; no Asset scan or gameplay-frame work is added. Budget: one adoption
per changed revision and linear construction plus the existing sort. PASS.

Regression checks replace an optional contract on revision change and ensure the
old name is removed. Integration uses Addressables list/get/invocation after an
Editor domain reload in the bound consuming project.
