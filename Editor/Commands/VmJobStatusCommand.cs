using System.Collections.Generic;
using Unity.Pipeline.Commands;
using VMUnityAutomation.Editor;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmJobStatusCommand
    {
        public const string CommandName = "vm_job_status";
        public const string Description =
            "Read one durable VM automation job snapshot without entering Unity's main-thread queue.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            Description,
            new[] { "jobs/status" },
            VmJsonSchema.Object(new Dictionary<string, VmJsonSchema>
            {
                { "job_id", VmJsonSchema.String("Durable job ID. Supply job_id or request_id.") },
                { "request_id", VmJsonSchema.String("Durable request ID. Supply request_id or job_id.") },
                { "job_type", VmJsonSchema.String("Optional exact job type discriminator.") },
                { "job_access_token", VmJsonSchema.String("Access token returned when the job was created.") },
                { "agent_id", VmJsonSchema.String("Optional original caller identity. Defaults to cli.") },
            }),
            VmJsonSchema.Any("Latest immutable published job snapshot or one stable domain error."),
            new[]
            {
                "invalid_arguments",
                "job_not_found",
                "job_type_mismatch",
                "job_owner_mismatch",
            },
            new[] { "read" },
            new[] { "pipeline_connected" },
            "Returns the latest already-published snapshot without waiting for the Unity main thread.",
            transactionDurability: "published-job-snapshot");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = false,
            Tags = new[] { "jobs/status" })]
        public static object Execute(
            [CliArg("job_id", "Durable job ID. Supply job_id or request_id.")]
            string jobId = null,
            [CliArg("request_id", "Durable request ID. Supply request_id or job_id.")]
            string requestId = null,
            [CliArg("job_type", "Optional exact job type discriminator.")]
            string jobType = null,
            [CliArg("job_access_token", "Access token returned when the job was created.")]
            string jobAccessToken = null,
            [CliArg("agent_id", "Optional original caller identity. Defaults to cli.")]
            string agentId = null)
        {
            var arguments = new Dictionary<string, object>();
            AddIfPresent(arguments, "jobId", jobId);
            AddIfPresent(arguments, "requestId", requestId);
            AddIfPresent(arguments, "jobType", jobType);
            AddIfPresent(arguments, "jobAccessToken", jobAccessToken);
            return VmAutomationPublishedJobReader.Get(arguments, agentId);
        }

        private static void AddIfPresent(
            IDictionary<string, object> arguments,
            string key,
            string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                arguments[key] = value.Trim();
        }
    }
}
