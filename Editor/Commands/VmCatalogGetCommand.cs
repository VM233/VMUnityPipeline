using System.Collections.Generic;
using Unity.Pipeline.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmCatalogGetCommand
    {
        public const string CommandName = "vm_catalog_get";
        public const string Description = "Return one exact VM Pipeline rich command contract.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            Description,
            new[] { "observability/catalog" },
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "name", VmJsonSchema.String("Exact VM Pipeline command name.") }
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
            MainThreadRequired = false,
            Tags = new[] { "observability/catalog" })]
        public static VmCatalogGetResult Execute(
            [CliArg("name", "Exact VM Pipeline command name.", Required = true)] string commandName)
        {
            return VmCommandContractCatalog.TryGet(commandName, out var contract)
                ? VmCatalogGetResult.Success(contract)
                : VmCatalogGetResult.NotFound(commandName);
        }
    }
}
