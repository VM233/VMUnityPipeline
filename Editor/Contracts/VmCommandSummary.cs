using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmCommandSummary
    {
        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("package")]
        public string Package { get; }

        [JsonProperty("tags")]
        public IReadOnlyList<string> Tags { get; }

        [JsonProperty("sideEffects")]
        public IReadOnlyList<string> SideEffects { get; }

        public VmCommandSummary(VmCommandContract contract)
        {
            Name = contract.Name;
            Description = contract.Description;
            Package = contract.Package;
            Tags = contract.Tags;
            SideEffects = contract.SideEffects;
        }
    }
}
