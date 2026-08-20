using System.Linq;
using NUnit.Framework;
using VMUnityPipeline.Editor.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmCommandContractCatalogTests
    {
        [Test]
        public void Contracts_AreUniqueSortedAndComplete()
        {
            var contracts = VmCommandContractCatalog.Contracts;
            var names = contracts.Select(contract => contract.Name).ToArray();

            Assert.That(names, Is.Ordered.Using<string>(System.StringComparer.Ordinal));
            Assert.That(names, Is.Unique);
            Assert.That(names, Has.All.StartsWith("vm_"));

            foreach (var contract in contracts)
            {
                Assert.That(contract.Description, Is.Not.Empty, contract.Name);
                Assert.That(contract.Package, Is.Not.Empty, contract.Name);
                Assert.That(contract.Tags, Is.Not.Empty, contract.Name);
                Assert.That(contract.InputSchema, Is.Not.Null, contract.Name);
                Assert.That(contract.OutputSchema, Is.Not.Null, contract.Name);
                Assert.That(contract.SideEffects, Is.Not.Empty, contract.Name);
                Assert.That(contract.Preconditions, Is.Not.Empty, contract.Name);
                Assert.That(contract.Completion, Is.Not.Empty, contract.Name);
            }
        }

        [Test]
        public void CatalogGet_UnknownName_ReturnsStableDomainError()
        {
            var result = VmCatalogGetCommand.Execute("missing_vm_command");

            Assert.That(result.Ok, Is.False);
            Assert.That(result.Found, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("command_not_found"));
            Assert.That(result.Contract, Is.Null);
        }

        [Test]
        public void CatalogList_IsBoundedAndSupportsTagSubtrees()
        {
            var result = VmCatalogListCommand.Execute(
                package: VmUnityPipelineInfo.PackageId,
                tag: "observability",
                offset: 1,
                limit: 1);

            Assert.That(result.Ok, Is.True);
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Commands, Has.Count.EqualTo(1));
            Assert.That(result.Commands[0].Name, Is.EqualTo(VmCatalogListCommand.CommandName));
        }

        [Test]
        public void Contracts_ExposeOneFacadeInsteadOfRegisteringEveryAutomationRoute()
        {
            Assert.That(
                VmCommandContractCatalog.Contracts.Count(contract =>
                    contract.Package == VmUnityPipelineInfo.PackageId &&
                    contract.Name == VmAutomationCallCommand.CommandName),
                Is.EqualTo(1));
            Assert.That(
                VmCommandContractCatalog.Contracts.Count(contract =>
                    contract.Package == "com.vm233.unity-automation"),
                Is.GreaterThan(300));
        }

        [TestCase(0)]
        [TestCase(51)]
        public void CatalogList_InvalidLimit_ReturnsStableDomainError(int limit)
        {
            var result = VmCatalogListCommand.Execute(limit: limit);

            Assert.That(result.Ok, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("invalid_limit"));
            Assert.That(result.Commands, Is.Empty);
        }
    }
}
