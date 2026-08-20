using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Contracts
{
    internal static class VmCommandContractCatalog
    {
        private static readonly IReadOnlyList<VmCommandContract> s_Contracts;
        private static readonly IReadOnlyDictionary<string, VmCommandContract> s_ContractsByName;

        static VmCommandContractCatalog()
        {
            var contracts = new[]
            {
                VmCatalogGetCommand.Contract,
                VmCatalogListCommand.Contract,
                VmCatalogStatusCommand.Contract,
                VmEditorStateCommand.Contract
            };

            var contractsByName = new Dictionary<string, VmCommandContract>(
                contracts.Length,
                StringComparer.Ordinal);

            foreach (var contract in contracts)
            {
                contractsByName.Add(contract.Name, contract);
            }

            s_Contracts = Array.AsReadOnly(contracts);
            s_ContractsByName = new ReadOnlyDictionary<string, VmCommandContract>(contractsByName);
        }

        public static IReadOnlyList<VmCommandContract> Contracts => s_Contracts;

        public static bool TryGet(string commandName, out VmCommandContract contract)
        {
            return s_ContractsByName.TryGetValue(commandName, out contract);
        }
    }
}
