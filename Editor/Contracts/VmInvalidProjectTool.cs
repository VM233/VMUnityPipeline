using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmInvalidProjectTool
    {
        [JsonProperty("toolName")]
        public string ToolName { get; }

        [JsonProperty("package")]
        public string Package { get; }

        [JsonProperty("validationError")]
        public string ValidationError { get; }

        public VmInvalidProjectTool(string toolName, string package, string validationError)
        {
            ToolName = toolName;
            Package = package;
            ValidationError = validationError;
        }
    }
}
