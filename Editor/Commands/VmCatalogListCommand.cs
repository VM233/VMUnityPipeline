using System;
using System.Collections.Generic;
using Unity.Pipeline.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Commands
{
    internal static class VmCatalogListCommand
    {
        public const string CommandName = "vm_catalog_list";
        public const string Description =
            "Return a bounded, filtered page of compact VM Pipeline command contracts.";
        private const int DefaultLimit = 10;
        private const int MaximumLimit = 50;

        public static readonly VmCommandContract Contract = new VmCommandContract(
            CommandName,
            Description,
            new[] { "observability/catalog" },
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "query", VmJsonSchema.String("Case-insensitive substring matched against name, description, package, and tags.") },
                    { "package", VmJsonSchema.String("Exact package identifier filter.") },
                    { "tag", VmJsonSchema.String("Exact tag or tag subtree filter.") },
                    { "side_effect", VmJsonSchema.String("Exact declared side-effect filter.") },
                    { "offset", VmJsonSchema.Integer("Number of matching commands to skip.", 0, 0) },
                    { "limit", VmJsonSchema.Integer("Maximum summaries to return.", DefaultLimit, 1, MaximumLimit) }
                }),
            VmJsonSchema.Object(
                new Dictionary<string, VmJsonSchema>
                {
                    { "ok", VmJsonSchema.Boolean("Whether the domain operation succeeded.") },
                    { "errorCode", VmJsonSchema.String("Stable domain error code.") },
                    { "errorMessage", VmJsonSchema.String("Developer-facing domain error message.") },
                    { "total", VmJsonSchema.Integer("Total contracts matching all filters.") },
                    { "offset", VmJsonSchema.Integer("Applied offset.") },
                    { "limit", VmJsonSchema.Integer("Applied page limit.") },
                    {
                        "commands",
                        VmJsonSchema.Array(
                            VmJsonSchema.Object(
                                new Dictionary<string, VmJsonSchema>
                                {
                                    { "name", VmJsonSchema.String("Stable command name.") },
                                    { "description", VmJsonSchema.String("Compact command description.") },
                                    { "package", VmJsonSchema.String("Owning UPM package.") },
                                    { "tags", VmJsonSchema.Array(VmJsonSchema.String("Hierarchical discovery tag.")) },
                                    { "sideEffects", VmJsonSchema.Array(VmJsonSchema.String("Declared side effect.")) }
                                },
                                new[] { "name", "description", "package", "tags", "sideEffects" }),
                            "Current result page.")
                    }
                },
                new[] { "ok", "total", "offset", "limit", "commands" }),
            new[] { "invalid_filter", "invalid_offset", "invalid_limit" },
            new[] { "read" },
            new[] { "pipeline_connected" },
            "Returns a deterministic page and the total number of matches.");

        [CliCommand(
            CommandName,
            Description,
            MainThreadRequired = false,
            Tags = new[] { "observability/catalog" })]
        public static VmCatalogListResult Execute(
            [CliArg("query", "Optional case-insensitive text query.")] string query = null,
            [CliArg("package", "Optional exact package identifier.")] string package = null,
            [CliArg("tag", "Optional exact tag or tag subtree.")] string tag = null,
            [CliArg("side_effect", "Optional exact declared side effect.")] string sideEffect = null,
            [CliArg("offset", "Number of matching contracts to skip.")] int offset = 0,
            [CliArg("limit", "Maximum summaries to return, from 1 through 50.")] int limit = DefaultLimit)
        {
            if ((query != null && query.Length == 0) ||
                (package != null && package.Length == 0) ||
                (tag != null && tag.Length == 0) ||
                (sideEffect != null && sideEffect.Length == 0))
            {
                return VmCatalogListResult.Failure(
                    "invalid_filter",
                    "Provided filters must contain at least one character.",
                    offset,
                    limit);
            }

            if (offset < 0)
            {
                return VmCatalogListResult.Failure(
                    "invalid_offset",
                    "Offset must be zero or greater.",
                    offset,
                    limit);
            }

            if (limit < 1 || limit > MaximumLimit)
            {
                return VmCatalogListResult.Failure(
                    "invalid_limit",
                    $"Limit must be between 1 and {MaximumLimit}.",
                    offset,
                    limit);
            }

            var tagPrefix = tag == null ? null : tag + "/";
            var summaries = new List<VmCommandSummary>(
                Math.Min(limit, VmCommandContractCatalog.Contracts.Count));
            var total = 0;

            foreach (var contract in VmCommandContractCatalog.Contracts)
            {
                if (!Matches(contract, query, package, tag, tagPrefix, sideEffect))
                {
                    continue;
                }

                if (total >= offset && summaries.Count < limit)
                {
                    summaries.Add(new VmCommandSummary(contract));
                }

                total++;
            }

            return VmCatalogListResult.Success(total, offset, limit, summaries);
        }

        private static bool Matches(
            VmCommandContract contract,
            string query,
            string package,
            string tag,
            string tagPrefix,
            string sideEffect)
        {
            if (package != null &&
                !string.Equals(contract.Package, package, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (tag != null && !MatchesTag(contract, tag, tagPrefix))
            {
                return false;
            }

            if (sideEffect != null && !MatchesSideEffect(contract, sideEffect))
                return false;

            return query == null || MatchesQuery(contract, query);
        }

        private static bool MatchesQuery(VmCommandContract contract, string query)
        {
            if (contract.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                contract.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                contract.Package.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            foreach (var contractTag in contract.Tags)
            {
                if (contractTag.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesTag(
            VmCommandContract contract,
            string tag,
            string tagPrefix)
        {
            foreach (var contractTag in contract.Tags)
            {
                if (string.Equals(contractTag, tag, StringComparison.OrdinalIgnoreCase) ||
                    contractTag.StartsWith(tagPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesSideEffect(
            VmCommandContract contract,
            string sideEffect)
        {
            foreach (string declaredSideEffect in contract.SideEffects)
            {
                if (string.Equals(
                        declaredSideEffect,
                        sideEffect,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
