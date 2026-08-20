using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmCommandContract
    {
        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("package")]
        public string Package { get; }

        [JsonProperty("tags")]
        public IReadOnlyList<string> Tags { get; }

        [JsonProperty("inputSchema")]
        public object InputSchema { get; }

        [JsonProperty("outputSchema")]
        public object OutputSchema { get; }

        [JsonProperty("errorCodes")]
        public IReadOnlyList<string> ErrorCodes { get; }

        [JsonProperty("sideEffects")]
        public IReadOnlyList<string> SideEffects { get; }

        [JsonProperty("preconditions")]
        public IReadOnlyList<string> Preconditions { get; }

        [JsonProperty("completion")]
        public string Completion { get; }

        [JsonProperty("transactionScope")]
        public string TransactionScope { get; }

        [JsonProperty("transactionAtomicity")]
        public string TransactionAtomicity { get; }

        [JsonProperty("transactionIsolation")]
        public string TransactionIsolation { get; }

        [JsonProperty("transactionDurability")]
        public string TransactionDurability { get; }

        [JsonProperty("transactionRollbackKind")]
        public string TransactionRollbackKind { get; }

        public VmCommandContract(
            string name,
            string description,
            string[] tags,
            object inputSchema,
            object outputSchema,
            string[] errorCodes,
            string[] sideEffects,
            string[] preconditions,
            string completion,
            string transactionScope = "none",
            string transactionAtomicity = "none",
            string transactionIsolation = "none",
            string transactionDurability = "editor-domain",
            string transactionRollbackKind = "none",
            string package = null)
        {
            Name = name;
            Description = description;
            Package = package ?? VmUnityPipelineInfo.PackageId;
            Tags = Array.AsReadOnly(tags);
            InputSchema = inputSchema;
            OutputSchema = outputSchema;
            ErrorCodes = Array.AsReadOnly(errorCodes);
            SideEffects = Array.AsReadOnly(sideEffects);
            Preconditions = Array.AsReadOnly(preconditions);
            Completion = completion;
            TransactionScope = transactionScope;
            TransactionAtomicity = transactionAtomicity;
            TransactionIsolation = transactionIsolation;
            TransactionDurability = transactionDurability;
            TransactionRollbackKind = transactionRollbackKind;
        }
    }
}
