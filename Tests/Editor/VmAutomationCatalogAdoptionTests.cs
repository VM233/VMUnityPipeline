using System;
using NUnit.Framework;
using VMUnityPipeline.Editor.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmAutomationCatalogAdoptionTests
    {
        [Test]
        public void ChangedRevisionReplacesNamesAndUnchangedRevisionDoesNotReload()
        {
            try
            {
                var absent = VmCommandContractCatalog.AdoptRevision("absent", () => Array.Empty<VmCommandContract>());
                Assert.That(absent.ContainsKey(VmCatalogGetCommand.CommandName), Is.True);
                const string name = "vm_optional_fixture";
                var optional = new VmCommandContract(name, "Optional capability fixture.", new[] { "readOnly" },
                    VmCatalogGetCommand.Contract.InputSchema, VmCatalogGetCommand.Contract.OutputSchema,
                    Array.Empty<string>(), new[] { "readsProjectState" }, new[] { "editor_connected" }, "Returns fixture evidence.");
                var present = VmCommandContractCatalog.AdoptRevision("present", () => new[] { optional });
                Assert.That(present[name], Is.SameAs(optional));
                var replaced = VmCommandContractCatalog.AdoptRevision("removed", () => Array.Empty<VmCommandContract>());
                Assert.That(replaced.ContainsKey(name), Is.False);
                var retained = VmCommandContractCatalog.AdoptRevision("removed", () => throw new InvalidOperationException("Unexpected reload"));
                Assert.That(retained, Is.SameAs(replaced));
                Assert.That(present, Is.Not.SameAs(replaced));
            }
            finally
            {
                _ = VmCommandContractCatalog.Contracts;
            }
        }
    }
}
