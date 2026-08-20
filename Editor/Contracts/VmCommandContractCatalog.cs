using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Contracts
{
    internal static class VmCommandContractCatalog
    {
        private static readonly IReadOnlyList<VmCommandContract> s_Contracts;
        private static readonly IReadOnlyDictionary<string, VmCommandContract> s_ContractsByName;
        private static readonly string s_CatalogRevision;

        static VmCommandContractCatalog()
        {
            var contracts = new List<VmCommandContract>
            {
                VmCatalogGetCommand.Contract,
                VmCatalogListCommand.Contract,
                VmCatalogStatusCommand.Contract,
                VmEditorStateCommand.Contract,
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
        }

        public static IReadOnlyList<VmCommandContract> Contracts => s_Contracts;

        public static string CatalogRevision => s_CatalogRevision;

        public static bool TryGet(string commandName, out VmCommandContract contract)
        {
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
