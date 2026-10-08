# Numeric CLI arguments

The JSON reader remains the owner of document syntax, duplicate-property rejection,
source token coordinates and nesting. `VmCliJsonNumberSource` extracts numeric
lexemes from those coordinates with one forward traversal of source lines and one
backward traversal of each numeric token. `VmJsonNumber` in VMUnityAutomation is
the only decimal-to-binary64 converter, shared with durable job persistence.
Platform reader numeric values are not adopted. Strings, booleans, null, arrays,
objects and Int64 integer tokens keep their existing contracts.

Static Cost Ledger before executable writes: the frozen request is below 16 KiB,
with 20 scientific values and at most 64 nested levels. Source line traversal and
numeric extraction together visit at most twice the input characters, with no
per-token document scan. Each lexeme is bounded by the original document; conversion
keeps at most 769 digits and below 16 KiB live arithmetic storage. There is no cache,
second transport, Unity API, I/O or new delayed lifecycle. Conversion work is
additive in the existing document tokens, not tokens times document length. Tests
cover the original literal and nested LF/CRLF source locations. PASS.

See the Automation package's JSON-number documentation for the rounding proof and
independent bit-level oracle. Invocation failures continue to use
`invalid_arguments_json`; no request, source fingerprint or report is rewritten.
