using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using VMUnityAutomation.Editor;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Contracts
{
    internal static class VmCommandContractCatalog
    {
        private static readonly object s_Sync = new object();
        private static IReadOnlyList<VmCommandContract> s_Contracts;
        private static IReadOnlyDictionary<string, VmCommandContract> s_ContractsByName;
        private static string s_CatalogRevision;
        private static string s_AutomationCatalogRevision;

        public static IReadOnlyList<VmCommandContract> Contracts
        {
            get
            {
                EnsureCurrent();
                return s_Contracts;
            }
        }

        public static string CatalogRevision
        {
            get
            {
                EnsureCurrent();
                return s_CatalogRevision;
            }
        }

        public static bool TryGet(string commandName, out VmCommandContract contract)
        {
            EnsureCurrent();
            return s_ContractsByName.TryGetValue(commandName, out contract);
        }

        internal static void Invalidate()
        {
            lock (s_Sync)
            {
                s_Contracts = null;
                s_ContractsByName = null;
                s_CatalogRevision = null;
                s_AutomationCatalogRevision = null;
            }
        }

        private static void EnsureCurrent()
        {
            lock (s_Sync)
            {
                string automationCatalogRevision =
                    VmAutomationCatalog.CatalogRevision;
                if (s_Contracts != null &&
                    string.Equals(s_AutomationCatalogRevision,
                        automationCatalogRevision,
                        StringComparison.Ordinal))
                {
                    return;
                }

                Rebuild(automationCatalogRevision);
            }
        }

        private static void Rebuild(string automationCatalogRevision)
        {
            var contracts = new List<VmCommandContract>
            {
                VmCatalogGetCommand.Contract,
                VmCatalogListCommand.Contract,
                VmCatalogStatusCommand.Contract,
                VmEditorStateCommand.Contract,
                VmJobStatusCommand.Contract,
                VmAutomationCallCommand.Contract
            };
            contracts.AddRange(VmAutomationContractAdapter.LoadContracts());
            contracts.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));

            var contractsByName = new Dictionary<string, VmCommandContract>(
                contracts.Count,
                StringComparer.Ordinal);

            foreach (var contract in contracts)
            {
                contractsByName.Add(contract.Name, contract);
            }

            s_Contracts = contracts.AsReadOnly();
            s_ContractsByName = new ReadOnlyDictionary<string, VmCommandContract>(contractsByName);
            s_CatalogRevision = ComputeCatalogRevision(contracts);
            s_AutomationCatalogRevision = automationCatalogRevision;
        }

        private static string ComputeCatalogRevision(IEnumerable<VmCommandContract> contracts)
        {
            string json = JsonConvert.SerializeObject(contracts, Formatting.None);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using (SHA256 sha256 = SHA256.Create())
            {
                return string.Concat(
                    sha256.ComputeHash(bytes).Select(value => value.ToString("x2")));
            }
        }
    }
}
