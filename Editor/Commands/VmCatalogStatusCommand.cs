using System;
using System.Collections.Generic;
using Unity.Pipeline.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmCatalogStatusCommand
    {
        public const string CommandName = "vm_catalog_status";
        public const string Description =
            "Return the VM Pipeline contract version, catalog revision, package version, and command count.";

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
                    { "commandCount", VmJsonSchema.Integer("Number of VM command contracts.") }
                },
                new[]
                {
                    "ok",
                    "contractVersion",
                    "catalogRevision",
                    "packageId",
                    "packageVersion",
                    "commandCount"
                }),
            new[] { "catalog_initialization_failed" },
            new[] { "read" },
            new[] { "pipeline_connected" },
            "Returns the immutable catalog identity for the current Domain.");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = false,
            Tags = new[] { "observability/catalog" })]
        public static VmCatalogStatusResult Execute()
        {
            try
            {
                return VmCatalogStatusResult.Success(
                    VmCommandContractCatalog.Contracts.Count);
            }
            catch (Exception exception)
            {
                return VmCatalogStatusResult.Failure(
                    exception.GetBaseException().Message);
            }
        }
    }
}
