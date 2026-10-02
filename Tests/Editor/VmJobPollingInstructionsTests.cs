using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.Pipeline.Commands;
using VMUnityAutomation.Editor;
using VMUnityPipeline.Editor.Commands;
using VMUnityPipeline.Editor.Contracts;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmJobPollingInstructionsTests
    {
        [Test]
        public void DurableProductPublishesExactBackgroundCommandAndCapability()
        {
            var snapshot = Job();
            snapshot["jobAccessToken"] = "fixture-capability";
            var polling = VmJobPollingInstructions.Create(true, snapshot, " fixture-agent ");

            Assert.That(polling.Command, Is.EqualTo(VmJobStatusCommand.CommandName));
            Assert.That(polling.Arguments["job_id"], Is.EqualTo("fixture-job"));
            Assert.That(polling.Arguments["job_type"], Is.EqualTo("asset-refresh"));
            Assert.That(polling.Arguments["job_access_token"], Is.EqualTo("fixture-capability"));
            Assert.That(polling.Arguments["agent_id"], Is.EqualTo("fixture-agent"));
            Assert.That(polling.Arguments, Has.Count.EqualTo(4));
            var registration = typeof(VmJobStatusCommand).GetMethod("Execute")
                .GetCustomAttribute<CliCommandAttribute>();
            Assert.That(registration.MainThreadRequired, Is.False);
        }

        [Test]
        public void ImmediateAndFailedProductsDoNotAdvertisePolling()
        {
            Assert.That(VmJobPollingInstructions.Create(false, Job()), Is.Null);
            Assert.That(VmJobPollingInstructions.Create(true, null), Is.Null);
            Assert.That(VmJobPollingInstructions.Create(true, "completed"), Is.Null);
            Assert.That(VmJobPollingInstructions.Create(true,
                new Dictionary<string, object> { { "jobId", "not-a-durable-product" } }), Is.Null);
            Assert.That(VmJobPollingInstructions.Create(true,
                new Dictionary<string, object> { { "jobType", "asset-refresh" } }), Is.Null);
        }

        [Test]
        public void MissingOptionalFieldsAreOmitted()
        {
            var polling = VmJobPollingInstructions.Create(true, Job());
            JObject serialized = JObject.Parse(JsonConvert.SerializeObject(polling));
            Assert.That(polling.Arguments, Has.Count.EqualTo(2));
            Assert.That(serialized["arguments"]["job_access_token"], Is.Null);
            Assert.That(serialized["arguments"]["agent_id"], Is.Null);
        }

        [Test]
        public void PublishedArgumentsAreImmutableAndIndependentOfTheOwnerDictionary()
        {
            var snapshot = Job();
            var polling = VmJobPollingInstructions.Create(true, snapshot);
            snapshot["jobId"] = "changed-after-publication";
            Assert.That(polling.Arguments["job_id"], Is.EqualTo("fixture-job"));
            Assert.Throws<NotSupportedException>(() =>
                ((IDictionary<string, string>)polling.Arguments)["job_id"] = "caller-change");
        }

        [Test]
        public void FacadeReferencesTheOwnerPayloadOnceAndPreservesItsEnvelope()
        {
            var snapshot = Job();
            var owner = (VmAutomationInvocationResult)typeof(VmAutomationInvocationResult)
                .GetMethod("Success", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { "fixture-command", "fixture/route", "fixture-request", snapshot, 7L });
            var facade = new VmAutomationCallResult(owner);
            Assert.That(facade.Result, Is.SameAs(snapshot));
            Assert.That(facade.Warnings, Is.SameAs(owner.Warnings));
            var serialized = JObject.Parse(JsonConvert.SerializeObject(facade));
            var expected = JObject.Parse(JsonConvert.SerializeObject(owner));
            Assert.That(serialized.Remove("polling"), Is.True);
            Assert.That(JToken.DeepEquals(serialized, expected), Is.True);
        }

        [Test]
        public async Task FacadePreservesARealDomainFailureWithoutPolling()
        {
            object result = await VmAutomationCallCommand.Execute("fixture-unregistered-polling-command");
            var serialized = JObject.Parse(JsonConvert.SerializeObject(result));
            Assert.That(serialized["ok"].Value<bool>(), Is.False);
            Assert.That(serialized["error"]["code"].Value<string>(), Is.EqualTo("command_not_found"));
            Assert.That(serialized["polling"], Is.Null);
        }

        [Test]
        public void FacadeSchemaClosesPollingArgumentsAndReusesTheirCommandOwner()
        {
            var facade = (VmJsonSchema)VmAutomationCallCommand.Contract.OutputSchema;
            var polling = facade.Properties["polling"];
            var arguments = polling.Properties["arguments"];
            var nativeInput = (VmJsonSchema)VmJobStatusCommand.Contract.InputSchema;
            Assert.That(polling.AdditionalProperties, Is.EqualTo(false));
            Assert.That(arguments.AdditionalProperties, Is.EqualTo(false));
            Assert.That(arguments.Required, Is.EqualTo(new[] { "job_id", "job_type" }));
            Assert.That(arguments.Properties["job_id"], Is.SameAs(nativeInput.Properties["job_id"]));
            Assert.That(polling.Properties["command"].EnumValues,
                Is.EqualTo(new[] { VmJobStatusCommand.CommandName }));
        }

        private static Dictionary<string, object> Job() => new()
        {
            { "jobId", "fixture-job" },
            { "jobType", "asset-refresh" },
            { "pollRoute", "jobs/get" }
        };
    }
}
