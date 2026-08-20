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

        public VmCatalogStatusResult(int commandCount)
            : base(true)
        {
            ContractVersion = VmUnityPipelineInfo.ContractVersion;
            CatalogRevision = VmUnityPipelineInfo.CatalogRevision;
            PackageId = VmUnityPipelineInfo.PackageId;
            PackageVersion = VmUnityPipelineInfo.PackageVersion;
            CommandCount = commandCount;
        }
    }
}
