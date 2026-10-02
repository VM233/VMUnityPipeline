using System.Collections.Generic;
using Newtonsoft.Json;
using VMUnityAutomation.Editor;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmAutomationCallResult
    {
        private readonly VmAutomationInvocationResult owner;

        [JsonProperty("ok")]
        public bool Ok => owner.Ok;

        [JsonProperty("command")]
        public string Command => owner.Command;

        [JsonProperty("route")]
        public string Route => owner.Route;

        [JsonProperty("requestId")]
        public string RequestId => owner.RequestId;

        [JsonProperty("status")]
        public string Status => owner.Status;

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public object Result => owner.Result;

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public VmAutomationError Error => owner.Error;

        [JsonProperty("warnings")]
        public IReadOnlyList<object> Warnings => owner.Warnings;

        [JsonProperty("executionTimeMs")]
        public long ExecutionTimeMs => owner.ExecutionTimeMs;

        [JsonProperty("catalogRevision")]
        public string CatalogRevision => owner.CatalogRevision;

        [JsonProperty("polling", NullValueHandling = NullValueHandling.Ignore)]
        public VmJobPollingInstructions Polling { get; }

        internal VmAutomationCallResult(VmAutomationInvocationResult owner, string agentId = null)
        {
            this.owner = owner;
            Polling = VmJobPollingInstructions.Create(owner.Ok, owner.Result, agentId);
        }
    }
}
