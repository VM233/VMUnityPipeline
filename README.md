# VM Unity Pipeline

VM Unity Pipeline extends Unity Technologies' official Unity CLI and Pipeline
with bounded command discovery, typed automation contracts and durable job
observation. It uses the official Pipeline server as its only Editor transport.

Install `com.vm233.unity-pipeline` and `com.vm233.unity-automation` from immutable
remote Git revisions. The package manifest declares supported Unity and upstream
package requirements; each consuming project owns its exact revision selection.

- [Integration and installation](Documentation~/integration.md)
- [Command contracts](Documentation~/commands.md)
- [Release history](CHANGELOG.md)

Package code is compiled and tested in supported consuming Unity projects after
publishing an immutable revision. Local UPM dependencies and embedded overrides
are not supported.
