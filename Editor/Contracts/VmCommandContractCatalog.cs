using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using VMUnityPipeline.Editor.Commands;
using VMUnityAutomation.Editor;

namespace VMUnityPipeline.Editor.Contracts
{
    internal static class VmCommandContractCatalog
    {
        private static IReadOnlyList<VmCommandContract> s_Contracts;
        private static IReadOnlyDictionary<string, VmCommandContract> s_ContractsByName;
        private static string s_CatalogRevision;
        private static string s_AutomationRevision;

        private static void EnsureCurrent()
        {
            AdoptRevision(VmAutomationCatalog.CatalogRevision, VmAutomationContractAdapter.LoadContracts);
        }

        internal static IReadOnlyDictionary<string, VmCommandContract> AdoptRevision(string revision, Func<IReadOnlyList<VmCommandContract>> loadContracts)
        {
            if (s_Contracts != null && s_AutomationRevision == revision) return s_ContractsByName;
            var contracts = new List<VmCommandContract>
            {
                VmCatalogGetCommand.Contract,
                VmCatalogListCommand.Contract,
                VmCatalogStatusCommand.Contract,
                VmEditorStateCommand.Contract,
                VmRemoveMissingScriptsCommand.Contract,
                VmJobStatusCommand.Contract,
                VmAutomationCallCommand.Contract
            };
            contracts.AddRange(loadContracts());
            contracts.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));

            var contractsByName = new Dictionary<string, VmCommandContract>(
                contracts.Count,
                StringComparer.Ordinal);

            foreach (var contract in contracts)
            {
                contractsByName.Add(contract.Name, contract);
            }

            string catalogRevision = ComputeCatalogRevision(contracts);
            s_Contracts = contracts.AsReadOnly();
            s_ContractsByName = new ReadOnlyDictionary<string, VmCommandContract>(contractsByName);
            s_CatalogRevision = catalogRevision;
            s_AutomationRevision = revision;
            return s_ContractsByName;
        }

        public static IReadOnlyList<VmCommandContract> Contracts
        {
            get { EnsureCurrent(); return s_Contracts; }
        }

        public static string CatalogRevision
        {
            get { EnsureCurrent(); return s_CatalogRevision; }
        }

        public static bool TryGet(string commandName, out VmCommandContract contract)
        {
            EnsureCurrent();
            return s_ContractsByName.TryGetValue(commandName, out contract);
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
