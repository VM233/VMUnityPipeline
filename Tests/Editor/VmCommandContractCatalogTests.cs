using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Pipeline.Commands;
using VMUnityPipeline.Editor.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmCommandContractCatalogTests
    {
        [Test]
        public void PipelineRegistrations_CoverEveryPackageContract()
        {
            Assert.That(typeof(CliCommandAttribute).Assembly.GetName().Name,
                Is.EqualTo("Unity.Pipeline.Attributes"));

            var registrations = typeof(VmCatalogGetCommand).Assembly.GetTypes()
                .SelectMany(type => type.GetMethods(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => System.Attribute.IsDefined(method, typeof(CliCommandAttribute)))
                .ToArray();
            var contractCount = VmCommandContractCatalog.Contracts.Count(
                contract => contract.Package == VmUnityPipelineInfo.PackageId);

            Assert.That(registrations, Has.Length.EqualTo(contractCount));
            Assert.That(registrations.Select(method => method.DeclaringType), Is.Unique);
        }

        [Test]
        public void CatalogStatus_ReportsOwnerCountsAndInvalidProjectTools()
        {
            var projectTools = new[]
            {
                new Dictionary<string, object> { { "toolName", "valid/tool" } },
                new Dictionary<string, object>
                {
                    { "toolName", "invalid/tool" },
                    { "package", "project:test" },
                    { "validationError", "The registered output schema is invalid." }
                }
            };
            var result = VmCatalogStatusCommand.CreateResult(
                VmCommandContractCatalog.Contracts, projectTools);

            Assert.That(result.Ok, Is.True);
            Assert.That(result.OwnerCounts.Values.Sum(), Is.EqualTo(result.CommandCount));
            Assert.That(result.OwnerCounts[VmUnityPipelineInfo.PackageId], Is.EqualTo(7));
            Assert.That(result.InvalidProjectTools, Has.Count.EqualTo(1));
            Assert.That(result.InvalidProjectTools[0].ToolName, Is.EqualTo("invalid/tool"));
            Assert.That(result.InvalidProjectTools[0].Package, Is.EqualTo("project:test"));
            Assert.That(result.InvalidProjectTools[0].ValidationError,
                Is.EqualTo("The registered output schema is invalid."));
            var schema = (VmJsonSchema)VmCatalogStatusCommand.Contract.OutputSchema;
            Assert.That(schema.Required, Does.Contain("ownerCounts"));
            Assert.That(schema.Required, Does.Contain("invalidProjectTools"));
        }

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
                Assert.That(contract.Invocation, Is.Not.Null, contract.Name);
                Assert.That(contract.Invocation.Command, Is.Not.Empty, contract.Name);
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

        [Test]
        public void JobStatus_DeclaresWorkspaceAdmissionAcknowledgement()
        {
            VmCommandContract contract = VmJobStatusCommand.Contract;

            Assert.That(contract.SideEffects,
                Does.Contain("acknowledgesDurableWorkspaceJob"));
            Assert.That(contract.Description,
                Does.Contain("first authorized read"));
            Assert.That(contract.Completion,
                Does.Contain("execution acknowledgement"));
        }

        [TestCase("com.example.project-tools", "com.example.project-tools")]
        [TestCase(null, "com.vm233.unity-automation")]
        public void AutomationAdapter_PreservesDeclaredOwnerPackage(
            string declaredPackage, string expectedPackage)
        {
            var tool = new Dictionary<string, object>();
            if (declaredPackage != null)
                tool["package"] = declaredPackage;

            Assert.That(
                VmAutomationContractAdapter.ResolvePackage(tool),
                Is.EqualTo(expectedPackage));
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
