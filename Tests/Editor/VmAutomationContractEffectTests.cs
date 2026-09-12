using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmAutomationContractEffectTests
    {
        [Test]
        public void MissingEffectsCannotBecomeAReadOnlyClaim()
        {
            var contract = VmAutomationContractAdapter.CreateContract(Input());
            Assert.That(contract.SideEffects, Is.Empty);
        }

        [Test]
        public void DeclaredEffectsSurviveAdaptationWithoutAdditionalClaims()
        {
            var input = Input();
            input["sideEffects"] = new[] { "writesBuildOutput", "startsProcesses" };
            var contract = VmAutomationContractAdapter.CreateContract(input);
            Assert.That(contract.SideEffects, Is.EqualTo(new[] { "writesBuildOutput", "startsProcesses" }));
        }

        [Test]
        public void PublishedBuildContractUsesTheAutomationEffectOwner()
        {
            var contract = VmCommandContractCatalog.Contracts.Single(value => value.Name == "vm_auto_build_start");
            Assert.That(contract.SideEffects, Does.Contain("writesBuildOutput"));
            Assert.That(contract.SideEffects, Does.Contain("startsProcesses"));
            Assert.That(contract.SideEffects, Does.Not.Contain("read"));
            Assert.That(contract.Preconditions, Does.Contain("stableEditMode"));
        }

        private static Dictionary<string, object> Input() => new()
        {
            { "toolName", "synthetic_effect_contract" },
            { "description", "Synthetic contract fixture, not a runtime build." },
            { "inputSchema", new Dictionary<string, object>() },
            { "outputSchema", new Dictionary<string, object>() }
        };
    }
}
