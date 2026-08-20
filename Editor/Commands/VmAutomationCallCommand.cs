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
        public const string Description =
            "Execute one exact VM automation or project-tool contract through the shared owner. " +
            "Call reload-resumable submission contracts attached so their durable job token is " +
            "published before a domain reload, then poll it with vm_job_status. Use the outer " +
            "Unity CLI --detach flow only for long non-durable calls.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            Description,
            new[] { "automation/execute" },
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "command", VmJsonSchema.String("Exact vm_auto_ or vm_pt_ identifier, or exact automation route.") },
                    { "arguments_json", VmJsonSchema.String("One JSON object containing owner arguments.", "{}") },
                    { "expected_project_path", VmJsonSchema.String("Absolute project root required by mutating owner contracts.") },
                    { "request_id", VmJsonSchema.String("Optional idempotent request identifier.") },
                    { "agent_id", VmJsonSchema.String("Optional caller identity for action and job ownership.") },
                    { "timeout_seconds", VmJsonSchema.Integer(
                        "Inner automation deferred-call wait timeout. This does not extend the outer Unity CLI request timeout. Durable submission contracts must remain attached until they return their inner job token; use unity command --detach only for long non-durable calls.",
                        120, 1, 3600) }
                },
                new[] { "command" }),
            CreateOutputSchema(),
            new[]
            {
                "invalid_arguments_json",
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
            "poll it with vm_job_status until terminal.",
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
            [CliArg("command", "Exact automation command name or route.", Required = true)]
            string command,
            [CliArg("arguments_json", "Owner arguments as one JSON object.")]
            string argumentsJson = "{}",
            [CliArg("expected_project_path", "Absolute project root for mutating commands.")]
            string expectedProjectPath = null,
            [CliArg("request_id", "Optional idempotent request identifier.")]
            string requestId = null,
            [CliArg("agent_id", "Optional caller identity.")]
            string agentId = null,
            [CliArg("timeout_seconds",
                "Inner deferred-call timeout from 1 through 3600 seconds. Keep durable submissions attached until they return a job token; use outer --detach only for long non-durable calls.")]
            int timeoutSeconds = 120)
        {
            if (!VmCliJsonArguments.TryParseObject(
                    argumentsJson,
                    out Dictionary<string, object> arguments,
                    out string parseError))
            {
                return Failure(command, requestId, "invalid_arguments_json", parseError);
            }

            if (!TryApplyExpectedProjectPath(
                    arguments,
                    expectedProjectPath,
                    out string bindingError))
            {
                return Failure(command, requestId, "argument_conflict", bindingError);
            }

            try
            {
                return await VmAutomationExecutor.ExecuteAsync(
                    command,
                    arguments,
                    requestId,
                    agentId,
                    timeoutSeconds);
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

        private static bool TryApplyExpectedProjectPath(
            IDictionary<string, object> arguments,
            string expectedProjectPath,
            out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(expectedProjectPath))
                return true;

            if (arguments.TryGetValue("expectedProjectPath", out object existing) &&
                existing != null &&
                !string.Equals(
                    existing.ToString(),
                    expectedProjectPath,
                    StringComparison.Ordinal))
            {
                errorMessage =
                    "expected_project_path conflicts with arguments_json.expectedProjectPath.";
                return false;
            }

            arguments["expectedProjectPath"] = expectedProjectPath;
            return true;
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
