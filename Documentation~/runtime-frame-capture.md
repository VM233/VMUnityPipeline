# Runtime frame capture

The runtime-only `vm_runtime_frame_capture_start`,
`vm_runtime_frame_capture_get` and `vm_runtime_frame_capture_cancel` commands
adapt the transport-neutral VM Unity Automation Runtime capture owner. They
use the existing official Unity runtime Pipeline manager and CLI instance
binding; no new server or transport is created. They are absent from the VM
Editor catalog. Discover the exact parameters on the selected live runtime.

Start requires a fresh capture UUID, the exact build GUID from official runtime
status and1–120,000 frame intervals. Save the original receipt before polling:
every get/cancel must carry its capture and lifetime UUIDs. An identical start
observes the same admission; another active capture is rejected. After a
terminal result, a fresh UUID replaces that one current result. There is no
history cache. A process exit/reload ends that lifetime; never dispatch a
replacement merely because a result is uncertain or the old process is gone.

The owner establishes one LateUpdate baseline, then records consecutive native
Unity frame numbers and monotonic Stopwatch intervals. Missing, duplicated or
out-of-order frames, a nonincreasing clock, changed time scale or changed screen
dimensions end the capture as failed with its observed prefix. Cancel disables
the component and retains a canceled prefix; disable, destroy and application
exit interrupt it. No game object, frame rate, time scale, resolution or
persistent setting outside this temporary capture owner is modified.

Progress contains bounded scalars and no histogram. Terminal statistics reduce
once and remain immutable. `include_histogram=true` returns100,001 read-only
integer bins,0.1 ms through10,000 ms; larger intervals remain in a separate
overflow count. P50/P90/P99 are intervals, not exact raw quantiles. Threshold
decisions use the upper bound; overflow has no finite histogram upper bound.
Zero-sample statistics are null. `Ok` is request/domain status; only a terminal
`Progress.Status=completed` with the full requested count is complete evidence.
Cancellation can return `Ok=true` while remaining explicitly incomplete.

Stable admission errors are owned by Automation: requires-play, invalid-identity,
invalid-budget, build-mismatch, identity-conflict, active, not-found and
identity-mismatch, all prefixed `frame_capture_` and using underscores. Native
observation failures are `frame_capture_context_changed`,
`frame_capture_frame_discontinuity` and `frame_capture_clock_discontinuity`.
Lifecycle interruption/cancellation codes retain the named cause. Unexpected
Unity/execution faults remain official Pipeline failures, not successful data.

This is wall time between Unity frame boundaries, including waits and stalls;
it is neither isolated CPU work nor GPU timing. Acceptance also needs the
official Player process/build identity, actual hardware/resolution, battle and
wave context, and complete runtime Console review. Editor timing is not Player
acceptance. Retain the exact start/end frame bounds and original evidence UUIDs.

## Builds and compatibility

The Runtime facade references only the native Pipeline attribute assembly and
Automation Runtime owner. Its assembly constraints mirror official Pipeline's
runtime build gates; it creates no bootstrap in builds without runtime Pipeline.
Unity2022.3 uses the declared Pipeline0.8-compatible attribute signature;
Unity6 uses the explicit global-group signature. The existing integration guide
continues to own selection of the reviewed2022 compatibility dependency.

The small Runtime facade and Automation Runtime DTO/owner assemblies are
preserved by the package's link.xml so runtime method discovery and reflection
serialization survive managed stripping. They use no Editor references. Player
tests must use the normal declared Release build configuration, independently
of any observation-only build define, and confirm command registration through
the official CLI.

The Automation documentation owns the measurement contract, cost ledger,
quantile definition, component lifecycle and focused tests. This facade only
adapts typed input/output; it contains no second sampler or histogram.
