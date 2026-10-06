using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Pipeline.Commands;
using VMUnityAutomation.Editor;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmAutomationCallCommand
    {
        public const string CommandName = "vm_automation_call";
        public const string TargetCommandParameter = "command";
        public const string ArgumentsJsonParameter = "arguments_json";
        public const string Description =
            "Execute one exact VM automation or project-tool contract through the shared owner. " +
            "Call reload-resumable submission contracts attached so their durable job token reaches " +
            "the client, then invoke the returned polling.command with polling.arguments; " +
            "it selects the background vm_job_status boundary. The first authorized poll releases the " +
            "inner workspace job for execution. Use the outer " +
            "Unity CLI --detach flow only for long non-durable calls.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            VmCommandInvocation.Direct(CommandName),
            Description,
            new[] { "automation/execute" },
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { TargetCommandParameter, VmJsonSchema.String("Exact vm_auto_ or vm_pt_ identifier, or exact automation route.") },
                    { ArgumentsJsonParameter, VmJsonSchema.String("One JSON object containing owner arguments.", "{}") },
                    { "expected_project_path", VmJsonSchema.String("Absolute project root required by mutating owner contracts. Automation accepts equivalent normalized paths in arguments_json.expectedProjectPath and rejects different project roots.") },
                    { "request_id", VmJsonSchema.String("Optional idempotent request identifier.") },
                    { "agent_id", VmJsonSchema.String("Optional caller identity for action and job ownership.") },
                    { "timeout_seconds", VmJsonSchema.Integer(
                        "Inner automation deferred-call wait timeout. This does not extend the outer Unity CLI request timeout. Durable submission contracts must remain attached until they return their inner job token, then the first vm_job_status poll acknowledges delivery and releases execution; use unity command --detach only for long non-durable calls.",
                        120, 1, 3600) }
                },
                new[] { TargetCommandParameter }),
            CreateOutputSchema(),
            new[]
            {
                "invalid_arguments_json",
                "invalid_arguments",
                "input_validation_limit",
                "argument_conflict",
                "command_not_found",
                "request_id_conflict",
                "invalid_timeout",
                "project_binding_required",
                "invalid_project_path",
                "project_mismatch",
                "confirmation_required",
                "play_mode_required",
                "workspace_job_active",
                "automation_timeout",
                "command_exception"
            },
            new[] { "depends_on_selected_command" },
            new[] { "pipeline_connected", "editor_connected" },
            "Returns the selected owner response or one stable domain error. " +
            "A response containing an inner jobId is durable admission evidence; " +
            "the returned polling object specifies its exact background CLI command and arguments; " +
            "the first authorized poll releases a queued workspace job, " +
            "and subsequent polls observe it until terminal.",
            transactionScope: "delegated",
            transactionAtomicity: "declared_by_selected_command",
            transactionIsolation: "request_and_owner",
            transactionDurability: "declared_by_selected_command",
            transactionRollbackKind: "declared_by_selected_command");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = true,
            Tags = new[] { "automation/execute" })]
        public static async Task<object> Execute(
            [CliArg(TargetCommandParameter, "Exact automation command name or route.", Required = true)]
            string command,
            [CliArg(ArgumentsJsonParameter, "Owner arguments as one JSON object.")]
            string argumentsJson = "{}",
            [CliArg("expected_project_path", "Absolute project root for mutating commands. Equivalent normalized arguments_json.expectedProjectPath is accepted.")]
            string expectedProjectPath = null,
            [CliArg("request_id", "Optional idempotent request identifier.")]
            string requestId = null,
            [CliArg("agent_id", "Optional caller identity.")]
            string agentId = null,
            [CliArg("timeout_seconds",
                "Inner deferred-call timeout from 1 through 3600 seconds. Keep durable submissions attached until they return a job token, then poll once to release execution; use outer --detach only for long non-durable calls.")]
            int timeoutSeconds = 120)
        {
            if (!VmCliJsonArguments.TryParseObject(
                    argumentsJson,
                    out Dictionary<string, object> arguments,
                    out string parseError))
            {
                return Failure(command, requestId, "invalid_arguments_json", parseError);
            }

            try
            {
                var ownerResult = await VmAutomationExecutor.ExecuteAsync(
                    command,
                    arguments,
                    requestId,
                    agentId,
                    timeoutSeconds,
                    expectedProjectPath);
                return new VmAutomationCallResult(ownerResult, agentId);
            }
            catch (Exception exception)
            {
                Exception rootCause = exception.GetBaseException();
                return Failure(
                    command,
                    requestId,
                    "command_exception",
                    $"{rootCause.GetType().Name}: {rootCause.Message}");
            }
        }

        private static Dictionary<string, object> Failure(
            string command,
            string requestId,
            string code,
            string message)
        {
            return new Dictionary<string, object>
            {
                { "ok", false },
                { "command", command ?? "" },
                { "route", "" },
                { "requestId", requestId ?? "" },
                { "status", "failed" },
                {
                    "error",
                    new Dictionary<string, object>
                    {
                        { "code", code },
                        { "message", message ?? "Automation call was rejected." },
                        { "retryable", false }
                    }
                },
                { "warnings", Array.Empty<object>() },
                { "executionTimeMs", 0L },
                { "catalogRevision", GetCatalogRevisionOrEmpty() }
            };
        }

        private static string GetCatalogRevisionOrEmpty()
        {
            try
            {
                return VmAutomationCatalog.CatalogRevision;
            }
            catch
            {
                return "";
            }
        }

        private static VmJsonSchema CreateOutputSchema()
        {
            return VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "ok", VmJsonSchema.Boolean("Whether the selected command succeeded.") },
                    { "command", VmJsonSchema.String("Resolved automation command name.") },
                    { "route", VmJsonSchema.String("Resolved automation route.") },
                    { "requestId", VmJsonSchema.String("Idempotent request identifier.") },
                    { "status", VmJsonSchema.String("completed or failed.") },
                    { "result", VmJsonSchema.Any("Owner-defined result on success.") },
                    { "polling", VmJobPollingInstructions.CreateSchema() },
                    {
                        "error",
                        VmJsonSchema.Object(
                            new Dictionary<string, VmJsonSchema>
                            {
                                { "code", VmJsonSchema.String("Stable domain error code.") },
                                { "message", VmJsonSchema.String("Developer-facing error message.") },
                                { "retryable", VmJsonSchema.Boolean("Whether a state-aware retry can be considered.") },
                                { "details", VmJsonSchema.Any("Owner-defined structured error details.") }
                            },
                            new[] { "code", "message", "retryable" })
                    },
                    { "warnings", VmJsonSchema.Array(VmJsonSchema.Any("Structured warning.")) },
                    { "executionTimeMs", VmJsonSchema.Integer("Owner execution time in milliseconds.", minimum: 0) },
                    { "catalogRevision", VmJsonSchema.String("Automation catalog revision used for dispatch.") }
                },
                new[]
                {
                    "ok",
                    "command",
                    "route",
                    "requestId",
                    "status",
                    "warnings",
                    "executionTimeMs",
                    "catalogRevision"
                });
        }
    }
}
