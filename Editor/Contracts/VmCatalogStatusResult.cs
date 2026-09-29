using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmCatalogStatusResult : VmCommandResult
    {
        [JsonProperty("contractVersion")]
        public int ContractVersion { get; }

        [JsonProperty("catalogRevision")]
        public string CatalogRevision { get; }

        [JsonProperty("packageId")]
        public string PackageId { get; }

        [JsonProperty("packageVersion")]
        public string PackageVersion { get; }

        [JsonProperty("commandCount")]
        public int CommandCount { get; }

        [JsonProperty("ownerCounts")]
        public IReadOnlyDictionary<string, int> OwnerCounts { get; }

        [JsonProperty("invalidProjectTools")]
        public IReadOnlyList<VmInvalidProjectTool> InvalidProjectTools { get; }

        private VmCatalogStatusResult(
            bool ok,
            int commandCount,
            string catalogRevision,
            IDictionary<string, int> ownerCounts,
            IList<VmInvalidProjectTool> invalidProjectTools,
            string errorCode = null,
            string errorMessage = null)
            : base(ok, errorCode, errorMessage)
        {
            ContractVersion = VmUnityPipelineInfo.ContractVersion;
            CatalogRevision = catalogRevision;
            PackageId = VmUnityPipelineInfo.PackageId;
            PackageVersion = VmUnityPipelineInfo.PackageVersion;
            CommandCount = commandCount;
            OwnerCounts = new ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>(ownerCounts));
            InvalidProjectTools = new List<VmInvalidProjectTool>(invalidProjectTools).AsReadOnly();
        }

        public static VmCatalogStatusResult Success(
            int commandCount,
            IDictionary<string, int> ownerCounts,
            IList<VmInvalidProjectTool> invalidProjectTools)
        {
            return new VmCatalogStatusResult(
                true,
                commandCount,
                VmCommandContractCatalog.CatalogRevision,
                ownerCounts,
                invalidProjectTools);
        }

        public static VmCatalogStatusResult Failure(string errorMessage)
        {
            return new VmCatalogStatusResult(
                false,
                0,
                null,
                new Dictionary<string, int>(),
                new List<VmInvalidProjectTool>(),
                "catalog_initialization_failed",
                errorMessage);
        }
    }
}
