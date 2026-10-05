using System.Collections.Generic;
using Unity.Pipeline.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmCatalogGetCommand
    {
        public const string CommandName = "vm_catalog_get";
        public const string Description =
            "Return one exact VM contract with its native CLI invocation mapping. Automation and project-tool identifiers execute through vm_automation_call.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            VmCommandInvocation.Direct(CommandName),
            Description,
            new[] { "observability/catalog" },
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "name", VmJsonSchema.String("Exact catalog identifier. Use the returned invocation.command to execute it.") }
                },
                new[] { "name" }),
            VmCommandContractSchema.CreateCatalogGetOutputSchema(),
            new[] { "command_not_found" },
            new[] { "read" },
            new[] { "pipeline_connected" },
            "Returns exactly one immutable contract or command_not_found.");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = true,
            Tags = new[] { "observability/catalog" })]
        public static VmCatalogGetResult Execute(
            [CliArg("name", "Exact catalog identifier. Its invocation.command identifies the native execution entry.", Required = true)] string commandName)
        {
            return VmCommandContractCatalog.TryGet(commandName, out var contract)
                ? VmCatalogGetResult.Success(contract)
                : VmCatalogGetResult.NotFound(commandName);
        }
    }
}
