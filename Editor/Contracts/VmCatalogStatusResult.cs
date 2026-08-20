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

        private VmCatalogStatusResult(
            bool ok,
            int commandCount,
            string catalogRevision,
            string errorCode = null,
            string errorMessage = null)
            : base(ok, errorCode, errorMessage)
        {
            ContractVersion = VmUnityPipelineInfo.ContractVersion;
            CatalogRevision = catalogRevision;
            PackageId = VmUnityPipelineInfo.PackageId;
            PackageVersion = VmUnityPipelineInfo.PackageVersion;
            CommandCount = commandCount;
        }

        public static VmCatalogStatusResult Success(int commandCount)
        {
            return new VmCatalogStatusResult(
                true,
                commandCount,
                VmCommandContractCatalog.CatalogRevision);
        }

        public static VmCatalogStatusResult Failure(string errorMessage)
        {
            return new VmCatalogStatusResult(
                false,
                0,
                null,
                "catalog_initialization_failed",
                errorMessage);
        }
    }
}
