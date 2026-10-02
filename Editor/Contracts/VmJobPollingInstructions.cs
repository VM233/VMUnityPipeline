using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmJobPollingInstructions
    {
        [JsonProperty("command")]
        public string Command => VmJobStatusCommand.CommandName;

        [JsonProperty("arguments")]
        public IReadOnlyDictionary<string, string> Arguments { get; }

        private VmJobPollingInstructions(Dictionary<string, string> arguments)
        {
            Arguments = new ReadOnlyDictionary<string, string>(arguments);
        }

        internal static VmJobPollingInstructions Create(
            bool succeeded, object ownerResult, string agentId = null)
        {
            if (!succeeded || !(ownerResult is IDictionary<string, object> snapshot) ||
                !snapshot.TryGetValue("jobId", out object jobId) ||
                !snapshot.TryGetValue("jobType", out object jobType))
                return null;

            var arguments = new Dictionary<string, string>
            {
                { "job_id", (string)jobId },
                { "job_type", (string)jobType }
            };
            if (snapshot.TryGetValue("jobAccessToken", out object accessToken))
                arguments.Add("job_access_token", (string)accessToken);
            if (!string.IsNullOrWhiteSpace(agentId))
                arguments.Add("agent_id", agentId.Trim());
            return new VmJobPollingInstructions(arguments);
        }

        internal static VmJsonSchema CreateSchema()
        {
            var input = (VmJsonSchema)VmJobStatusCommand.Contract.InputSchema;
            return VmJsonSchema.Object(new Dictionary<string, VmJsonSchema>
            {
                { "command", VmJsonSchema.String("Official background CLI polling command.",
                    enumValues: new[] { VmJobStatusCommand.CommandName }) },
                { "arguments", VmJsonSchema.Object(new Dictionary<string, VmJsonSchema>
                    {
                        { "job_id", input.Properties["job_id"] },
                        { "job_type", input.Properties["job_type"] },
                        { "job_access_token", input.Properties["job_access_token"] },
                        { "agent_id", input.Properties["agent_id"] }
                    }, new[] { "job_id", "job_type" }) }
            }, new[] { "command", "arguments" },
                "CLI instruction for the same admitted job. Preserve the absolute project binding.");
        }
    }
}
