using System;
using System.Collections.Generic;
using Unity.Pipeline.Commands;
using VMUnityAutomation.Editor;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmCatalogStatusCommand
    {
        public const string CommandName = "vm_catalog_status";
        public const string Description =
            "Return the VM Pipeline catalog identity, command counts by owner package, and invalid project-tool registrations.";

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            Description,
            new[] { "observability/catalog" },
            VmJsonSchema.Object(new Dictionary<string, VmJsonSchema>()),
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "ok", VmJsonSchema.Boolean("Whether the domain operation succeeded.") },
                    { "contractVersion", VmJsonSchema.Integer("Rich command contract format version.") },
                    { "catalogRevision", VmJsonSchema.String("Revision shared by all contracts in this Domain.") },
                    { "packageId", VmJsonSchema.String("UPM package identifier.") },
                    { "packageVersion", VmJsonSchema.String("UPM package version.") },
                    { "commandCount", VmJsonSchema.Integer("Number of VM command contracts.") },
                    { "ownerCounts", VmJsonSchema.Object(
                        new Dictionary<string, VmJsonSchema>(),
                        description: "Contract counts keyed by their authoritative package owner.",
                        additionalProperties: VmJsonSchema.Integer("Number of contracts owned by this package.")) },
                    { "invalidProjectTools", VmJsonSchema.Array(
                        VmJsonSchema.Object(
                            new Dictionary<string, VmJsonSchema>
                            {
                                { "toolName", VmJsonSchema.String("Invalid project-tool registration name.") },
                                { "package", VmJsonSchema.String("Authoritative owning package.") },
                                { "validationError", VmJsonSchema.String("Registration error reported by Automation.") }
                            },
                            new[] { "toolName", "package", "validationError" }),
                        "Invalid registrations reported by the Automation project-tool owner, including tools excluded from the valid catalog.") }
                },
                new[]
                {
                    "ok",
                    "contractVersion",
                    "catalogRevision",
                    "packageId",
                    "packageVersion",
                    "commandCount",
                    "ownerCounts",
                    "invalidProjectTools"
                }),
            new[] { "catalog_initialization_failed" },
            new[] { "read" },
            new[] { "pipeline_connected" },
            "Returns the immutable catalog identity for the current Domain.");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = true,
            Tags = new[] { "observability/catalog" })]
        public static VmCatalogStatusResult Execute()
        {
            try
            {
                return CreateResult(
                    VmCommandContractCatalog.Contracts,
                    VmProjectToolRegistry.GetToolSummaries(validOnly: false));
            }
            catch (Exception exception)
            {
                return VmCatalogStatusResult.Failure(
                    exception.GetBaseException().Message);
            }
        }

        internal static VmCatalogStatusResult CreateResult(
            IReadOnlyList<VmCommandContract> contracts,
            IEnumerable<Dictionary<string, object>> projectTools)
        {
            var ownerCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (VmCommandContract contract in contracts)
            {
                ownerCounts.TryGetValue(contract.Package, out int count);
                ownerCounts[contract.Package] = count + 1;
            }

            var invalidProjectTools = new List<VmInvalidProjectTool>();
            foreach (Dictionary<string, object> tool in projectTools)
            {
                if (tool.TryGetValue("validationError", out object error))
                {
                    invalidProjectTools.Add(new VmInvalidProjectTool(
                        (string)tool["toolName"], (string)tool["package"], (string)error));
                }
            }

            return VmCatalogStatusResult.Success(contracts.Count, ownerCounts, invalidProjectTools);
        }
    }
}
