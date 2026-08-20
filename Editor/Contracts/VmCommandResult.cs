using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal abstract class VmCommandResult
    {
        [JsonProperty("ok")]
        public bool Ok { get; }

        [JsonProperty("errorCode", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorCode { get; }

        [JsonProperty("errorMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; }

        protected VmCommandResult(bool ok, string errorCode = null, string errorMessage = null)
        {
            Ok = ok;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}
