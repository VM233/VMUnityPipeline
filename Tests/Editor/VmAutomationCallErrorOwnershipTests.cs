using System.Threading.Tasks;
using NUnit.Framework;
using VMUnityAutomation.Editor;
using VMUnityPipeline.Editor.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmAutomationCallErrorOwnershipTests
    {
        [Test]
        public async Task InvalidJsonUsesTheSharedTypedFailureProduct()
        {
            var result = (VmAutomationCallResult)await VmAutomationCallCommand.Execute(
                "vm_auto_asset_list", argumentsJson: "{invalid}", requestId: "invalid-json-owner");
            Assert.That(result.Ok, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("invalid_arguments_json"));
            Assert.That(result.RequestId, Is.EqualTo("invalid-json-owner"));
            Assert.That(result.CatalogRevision, Is.EqualTo(VmAutomationCatalog.CatalogRevision));
            Assert.That(result.Polling, Is.Null);
        }

        [Test]
        public async Task ConflictingSourcesRetainTheirSpecificDomainError()
        {
            var result = (VmAutomationCallResult)await VmAutomationCallCommand.Execute(
                "vm_auto_asset_list", argumentsJson: "{}", argumentsFile: "unused.json");
            Assert.That(result.Ok, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("argument_conflict"));
            Assert.That(result.Error.Retryable, Is.False);
        }

        [Test]
        public async Task UnknownCommandRetainsTheDispatchOwnersFailure()
        {
            var result = (VmAutomationCallResult)await VmAutomationCallCommand.Execute(
                "__unknown_cli_error_owner");
            Assert.That(result.Ok, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("command_not_found"));
            Assert.That(result.CatalogRevision, Is.EqualTo(VmAutomationCatalog.CatalogRevision));
        }
    }
}
