using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmCatalogGetResult : VmCommandResult
    {
        [JsonProperty("found")]
        public bool Found { get; }

        [JsonProperty("contract", NullValueHandling = NullValueHandling.Ignore)]
        public VmCommandContract Contract { get; }

        private VmCatalogGetResult(
            bool ok,
            bool found,
            VmCommandContract contract,
            string errorCode = null,
            string errorMessage = null)
            : base(ok, errorCode, errorMessage)
        {
            Found = found;
            Contract = contract;
        }

        public static VmCatalogGetResult Success(VmCommandContract contract)
        {
            return new VmCatalogGetResult(true, true, contract);
        }

        public static VmCatalogGetResult NotFound(string commandName)
        {
            return new VmCatalogGetResult(
                false,
                false,
                null,
                "command_not_found",
                $"No VM Pipeline command contract is registered for '{commandName}'.");
        }
    }
}
