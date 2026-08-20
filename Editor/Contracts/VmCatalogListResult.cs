using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VMUnityPipeline.Editor.Contracts
{
    internal sealed class VmCatalogListResult : VmCommandResult
    {
        [JsonProperty("total")]
        public int Total { get; }

        [JsonProperty("offset")]
        public int Offset { get; }

        [JsonProperty("limit")]
        public int Limit { get; }

        [JsonProperty("commands")]
        public IReadOnlyList<VmCommandSummary> Commands { get; }

        private VmCatalogListResult(
            bool ok,
            int total,
            int offset,
            int limit,
            IReadOnlyList<VmCommandSummary> commands,
            string errorCode = null,
            string errorMessage = null)
            : base(ok, errorCode, errorMessage)
        {
            Total = total;
            Offset = offset;
            Limit = limit;
            Commands = commands;
        }

        public static VmCatalogListResult Success(
            int total,
            int offset,
            int limit,
            List<VmCommandSummary> commands)
        {
            return new VmCatalogListResult(
                true,
                total,
                offset,
                limit,
                commands.AsReadOnly());
        }

        public static VmCatalogListResult Failure(
            string errorCode,
            string errorMessage,
            int offset,
            int limit)
        {
            return new VmCatalogListResult(
                false,
                0,
                offset,
                limit,
                Array.Empty<VmCommandSummary>(),
                errorCode,
                errorMessage);
        }
    }
}
