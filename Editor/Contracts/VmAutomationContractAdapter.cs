using System;
using System.Collections;
using System.Collections.Generic;
using VMUnityAutomation.Editor;

namespace VMUnityPipeline.Editor.Contracts
{
    internal static class VmAutomationContractAdapter
    {
        private const int PageSize = 50;
        private const string AutomationPackageId = "com.vm233.unity-automation";

        public static IReadOnlyList<VmCommandContract> LoadContracts()
        {
            var contracts = new List<VmCommandContract>(VmAutomationCatalog.Count);
            var offset = 0;

            while (true)
            {
                object rawPage = VmAutomationCatalog.GetRegisteredTools(
                    compact: false,
                    includeSchema: true,
                    offset: offset,
                    limit: PageSize);
                if (!(rawPage is IDictionary<string, object> page))
                    throw new InvalidOperationException("Automation catalog returned an invalid page.");
                if (!page.TryGetValue("tools", out object rawTools) ||
                    !(rawTools is IEnumerable tools))
                {
                    throw new InvalidOperationException(
                        "Automation catalog page did not contain a tools collection.");
                }

                var pageCount = 0;
                foreach (object rawTool in tools)
                {
                    if (!(rawTool is IDictionary<string, object> tool))
                    {
                        throw new InvalidOperationException(
                            "Automation catalog contained an invalid tool contract.");
                    }

                    contracts.Add(CreateContract(tool));
                    pageCount++;
                }

                if (!page.TryGetValue("nextOffset", out object nextOffset))
                    break;
                offset = Convert.ToInt32(nextOffset);
                if (pageCount == 0)
                    throw new InvalidOperationException("Automation catalog pagination did not advance.");
            }

            if (contracts.Count != VmAutomationCatalog.Count)
            {
                throw new InvalidOperationException(
                    $"Automation catalog count changed while loading: expected " +
                    $"{VmAutomationCatalog.Count}, received {contracts.Count}.");
            }

            return contracts.AsReadOnly();
        }

        private static VmCommandContract CreateContract(IDictionary<string, object> tool)
        {
            IDictionary<string, object> transaction = ReadDictionary(tool, "transaction");
            IReadOnlyList<string> tags = ReadStrings(tool, "tags", "automation");
            IReadOnlyList<string> sideEffects = ReadStrings(tool, "sideEffects", "read");
            IReadOnlyList<string> preconditions = ReadStrings(
                tool,
                "preconditions",
                "editor_connected");

            return new VmCommandContract(
                ReadRequiredString(tool, "toolName"),
                ReadRequiredString(tool, "description"),
                ToArray(tags),
                ReadRequiredValue(tool, "inputSchema"),
                ReadRequiredValue(tool, "outputSchema"),
                ToArray(ReadStrings(tool, "errorCodes")),
                ToArray(sideEffects),
                ToArray(preconditions),
                ReadString(tool, "completionEvidence") ??
                "Returns the owner result after the automation route reports completion.",
                ReadString(transaction, "scope") ?? "none",
                ReadString(transaction, "atomicity") ?? "none",
                ReadString(transaction, "isolation") ?? "none",
                ReadString(transaction, "durability") ?? "editor-domain",
                ReadString(transaction, "rollbackKind") ?? "none",
                AutomationPackageId);
        }

        private static object ReadRequiredValue(
            IDictionary<string, object> values,
            string key)
        {
            if (!values.TryGetValue(key, out object value) || value == null)
                throw new InvalidOperationException($"Automation contract is missing '{key}'.");
            return value;
        }

        private static string ReadRequiredString(
            IDictionary<string, object> values,
            string key)
        {
            string value = ReadString(values, key);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Automation contract is missing '{key}'.");
            return value;
        }

        private static string ReadString(IDictionary<string, object> values, string key)
        {
            return values != null &&
                   values.TryGetValue(key, out object value) &&
                   value != null &&
                   !string.IsNullOrWhiteSpace(value.ToString())
                ? value.ToString()
                : null;
        }

        private static IDictionary<string, object> ReadDictionary(
            IDictionary<string, object> values,
            string key)
        {
            return values.TryGetValue(key, out object value)
                ? value as IDictionary<string, object>
                : null;
        }

        private static IReadOnlyList<string> ReadStrings(
            IDictionary<string, object> values,
            string key,
            string fallback = null)
        {
            var result = new List<string>();
            if (values != null &&
                values.TryGetValue(key, out object rawValues) &&
                rawValues is IEnumerable enumerable &&
                !(rawValues is string))
            {
                foreach (object rawValue in enumerable)
                {
                    string value = rawValue?.ToString();
                    if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value))
                        result.Add(value);
                }
            }

            if (result.Count == 0 && fallback != null)
                result.Add(fallback);
            return result.AsReadOnly();
        }

        private static string[] ToArray(IReadOnlyList<string> values)
        {
            var result = new string[values.Count];
            for (var index = 0; index < values.Count; index++)
                result[index] = values[index];
            return result;
        }
    }
}
