using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.Pipeline.Commands;
using VMUnityPipeline.Editor.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmCommandInvocationTests
    {
        [TestCase(typeof(VmCatalogStatusCommand))]
        [TestCase(typeof(VmCatalogListCommand))]
        [TestCase(typeof(VmCatalogGetCommand))]
        [TestCase(typeof(VmEditorStateCommand))]
        [TestCase(typeof(VmAutomationCallCommand))]
        [TestCase(typeof(VmJobStatusCommand))]
        [TestCase(typeof(VmRemoveMissingScriptsCommand))]
        public void NativeContractsTargetTheirActualRegisteredEntry(Type owner)
        {
            var contract = (VmCommandContract)owner.GetField("Contract").GetValue(null);
            var registration = owner.GetMethod("Execute").GetCustomAttribute<CliCommandAttribute>();
            Assert.That(contract.Invocation.Command, Is.EqualTo(registration.Name));
            Assert.That(contract.Invocation.Arguments, Is.Empty);
            Assert.That(contract.Invocation.ArgumentsJsonParameter, Is.Null);
            var json = JObject.Parse(JsonConvert.SerializeObject(contract.Invocation));
            Assert.That(json["argumentsJsonParameter"], Is.Null);
        }

        [TestCase("vm_auto_prefab_asset_get_properties", "com.vm233.unity-automation")]
        [TestCase("vm_pt_fixture_inspect", "project:fixture")]
        [TestCase("fixture_without_prefix", "com.vm233.unity-pipeline")]
        public void AdaptedContractsTargetTheFacadeRegardlessOfIdentifierOrPackage(
            string name, string package)
        {
            var contract = VmAutomationContractAdapter.CreateContract(Tool(name, package));
            Assert.That(contract.Invocation.Command, Is.EqualTo(VmAutomationCallCommand.CommandName));
            Assert.That(contract.Invocation.Arguments, Has.Count.EqualTo(1));
            Assert.That(contract.Invocation.Arguments[VmAutomationCallCommand.TargetCommandParameter],
                Is.EqualTo(name));
            Assert.That(contract.Invocation.ArgumentsJsonParameter,
                Is.EqualTo(VmAutomationCallCommand.ArgumentsJsonParameter));
            Assert.That(contract.Package, Is.EqualTo(package));
        }

        [Test]
        public void SummaryAndFullContractShareOneImmutableMapping()
        {
            var source = Tool("vm_auto_prefab_asset_get_properties", "com.vm233.unity-automation");
            var contract = VmAutomationContractAdapter.CreateContract(source);
            var summary = new VmCommandSummary(contract);
            source["toolName"] = "changed-after-publication";
            Assert.That(summary.Invocation, Is.SameAs(contract.Invocation));
            Assert.That(summary.Invocation.Arguments[VmAutomationCallCommand.TargetCommandParameter],
                Is.EqualTo("vm_auto_prefab_asset_get_properties"));
            Assert.Throws<NotSupportedException>(() =>
                ((IDictionary<string, string>)summary.Invocation.Arguments)
                [VmAutomationCallCommand.TargetCommandParameter] = "caller-change");
            var full = JObject.Parse(JsonConvert.SerializeObject(contract));
            var compact = JObject.Parse(JsonConvert.SerializeObject(summary));
            Assert.That(JToken.DeepEquals(full["invocation"], compact["invocation"]), Is.True);
        }

        [Test]
        public void InvocationParametersBelongToTheRegisteredFacadeAndItsSchema()
        {
            var parameters = typeof(VmAutomationCallCommand).GetMethod("Execute").GetParameters();
            var target = parameters[0].GetCustomAttribute<CliArgAttribute>();
            var json = parameters[1].GetCustomAttribute<CliArgAttribute>();
            var native = (VmJsonSchema)VmAutomationCallCommand.Contract.InputSchema;
            var invocation = VmCommandInvocation.Automation("fixture-owner");
            Assert.That(invocation.Arguments.ContainsKey(target.Name), Is.True);
            Assert.That(invocation.ArgumentsJsonParameter, Is.EqualTo(json.Name));
            Assert.That(native.Required, Does.Contain(target.Name));
            Assert.That(native.Properties.ContainsKey(json.Name), Is.True);
        }

        [Test]
        public void BothDiscoverySchemasRequireAClosedInvocationProduct()
        {
            var exact = (VmJsonSchema)VmCatalogGetCommand.Contract.OutputSchema;
            var contract = exact.Definitions["commandContract"];
            var page = (VmJsonSchema)VmCatalogListCommand.Contract.OutputSchema;
            var summary = page.Properties["commands"].Items;
            Assert.That(contract.Required, Does.Contain("invocation"));
            Assert.That(summary.Required, Does.Contain("invocation"));
            Assert.That(contract.Properties["invocation"].AdditionalProperties, Is.EqualTo(false));
            Assert.That(summary.Properties["invocation"].Properties["arguments"].AdditionalProperties,
                Is.EqualTo(false));
            Assert.That(contract.Properties["invocation"].Required,
                Is.EqualTo(new[] { "command", "arguments" }));
            Assert.That(contract.Properties["invocation"].Properties["argumentsJsonParameter"].EnumValues,
                Is.EqualTo(new[] { VmAutomationCallCommand.ArgumentsJsonParameter }));
        }

        private static Dictionary<string, object> Tool(string name, string package) => new()
        {
            { "toolName", name },
            { "package", package },
            { "description", "Invocation contract fixture." },
            { "inputSchema", new Dictionary<string, object>() },
            { "outputSchema", new Dictionary<string, object>() }
        };
    }
}
