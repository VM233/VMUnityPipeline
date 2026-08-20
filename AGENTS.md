# VMUnityPipeline Agent Rules

This repository is the authoritative source for the user-maintained com.vm233.unity-pipeline Unity package.

- Work only on the checked-out main branch unless the user explicitly requests otherwise.
- Use Unity Technologies' com.unity.pipeline as the only Editor command transport. Do not add a second HTTP/stdio bridge or a fallback to a retired repository.
- Package dependencies must be published registry versions or immutable remote Git revisions. Local file dependencies, embedded overrides, symlinks, and junctions are forbidden.
- Public commands use the vm_ prefix, compact default output, stable domain error codes, and explicit side-effect and lifecycle contracts.
- A command has one production owner. Command wrappers adapt typed input and return evidence; they do not copy Unity or project business logic.
- Keep one handwritten top-level type per C# file. Do not add handwritten partial types.
- Update README, Documentation~, CHANGELOG, package version, tests, and consumer pins in the same published change when their contracts are affected.
- Compile package code in a supported consuming Unity project after publishing an immutable revision. Do not validate through a local UPM dependency.
