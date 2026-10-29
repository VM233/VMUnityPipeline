using Unity.Pipeline.Commands;
using UnityEngine.Scripting;
using VMUnityAutomation.Runtime;

namespace VMUnityPipeline.Runtime
{
    [Preserve]
    public static class VmRuntimeFrameCaptureCommands
    {
        [Preserve]
        [CliCommand(
#if UNITY_6000_0_OR_NEWER
            "global",
#endif
            "vm_runtime_frame_capture_start",
            "Admit one process-lifetime contiguous frame capture. Creates a temporary sampling owner; changes no game/settings state. Retain capture/lifetime IDs, poll get, cancel on early exit. The same UUID/input observes its original admission. A completed capture proves only its reported native interval, not GPU timing or gameplay coverage.",
            MainThreadRequired = true,
            Tags = new[] { "runtime/vm/observability" })]
        public static VmFrameCaptureResult Start(
            [CliArg("capture_id", "Fresh UUID in D format; retained as the capture identity.", Required = true)]
            string captureID,
            [CliArg("expected_build_guid", "Exact Application.buildGUID from this runtime's status; may be empty only in Editor Play Mode.", Required = true)]
            string expectedBuildGUID,
            [CliArg("frame_intervals", "Contiguous native frame intervals from1 through120000. This does not set frame rate.")]
            int frameIntervals = 3600) =>
            VmFrameCaptureService.Start(captureID, frameIntervals, expectedBuildGUID);

        [Preserve]
        [CliCommand(
#if UNITY_6000_0_OR_NEWER
            "global",
#endif
            "vm_runtime_frame_capture_get",
            "Read the originally admitted frame capture without advancing it. Progress is constant-sized; terminal statistics are immutable. Explicit histogram output has100001 bins. Failed/interrupted/canceled prefixes do not establish complete capture. Returns a stable owner error for missing or mismatched identity.",
            MainThreadRequired = true,
            Tags = new[] { "runtime/vm/observability" })]
        public static VmFrameCaptureResult Get(
            [CliArg("capture_id", "Exact capture UUID from the original start receipt.", Required = true)]
            string captureID,
            [CliArg("lifetime_id", "Exact service lifetime UUID from the original start receipt.", Required = true)]
            string lifetimeID,
            [CliArg("include_histogram", "Include the read-only terminal histogram; progress never includes it.")]
            bool includeHistogram = false) =>
            VmFrameCaptureService.Get(captureID, lifetimeID, includeHistogram);

        [Preserve]
        [CliCommand(
#if UNITY_6000_0_OR_NEWER
            "global",
#endif
            "vm_runtime_frame_capture_cancel",
            "Cancel the original capture, retain its immutable observed prefix and disable sampling. Repeated cancel of a terminal capture leaves its report unchanged. Mutates only the capture owner, without changing game/settings state.",
            MainThreadRequired = true,
            Tags = new[] { "runtime/vm/observability" })]
        public static VmFrameCaptureResult Cancel(
            [CliArg("capture_id", "Exact capture UUID from the original start receipt.", Required = true)]
            string captureID,
            [CliArg("lifetime_id", "Exact service lifetime UUID from the original start receipt.", Required = true)]
            string lifetimeID) =>
            VmFrameCaptureService.Cancel(captureID, lifetimeID);
    }
}
