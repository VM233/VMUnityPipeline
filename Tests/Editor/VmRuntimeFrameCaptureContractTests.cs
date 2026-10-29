using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Pipeline.Commands;
using VMUnityPipeline.Runtime;

namespace VMUnityPipeline.Editor.Tests
{
    public sealed class VmRuntimeFrameCaptureContractTests
    {
        [TestCase("Start", "captureID")]
        [TestCase("Start", "expectedBuildGUID")]
        [TestCase("Get", "captureID")]
        [TestCase("Get", "lifetimeID")]
        [TestCase("Cancel", "captureID")]
        [TestCase("Cancel", "lifetimeID")]
        public void CaptureIdentityArgumentsAreRequiredByTheNativeSchema(string method, string parameter)
        {
            var argument = typeof(VmRuntimeFrameCaptureCommands).GetMethod(method).GetParameters()
                .Single(value => value.Name == parameter).GetCustomAttribute<CliArgAttribute>();
            Assert.That(argument.Required, Is.True);
        }

        [TestCase("Start", "frameIntervals")]
        [TestCase("Get", "includeHistogram")]
        public void ObservationOptionsRetainTheirNativeOptionalDefaults(string method, string parameter)
        {
            var argument = typeof(VmRuntimeFrameCaptureCommands).GetMethod(method).GetParameters()
                .Single(value => value.Name == parameter);
            Assert.That(argument.GetCustomAttribute<CliArgAttribute>().Required, Is.False);
            Assert.That(argument.HasDefaultValue, Is.True);
        }

        [Test]
        public void RegistrationsAreRuntimeOnlyAndExecuteOnTheMainThread()
        {
            foreach (string method in new[] { "Start", "Get", "Cancel" })
            {
                var registration = typeof(VmRuntimeFrameCaptureCommands).GetMethod(method)
                    .GetCustomAttribute<CliCommandAttribute>();
                Assert.That(registration.MainThreadRequired, Is.True);
                Assert.That(registration.Tags, Is.EqualTo(new[] { "runtime/vm/observability" }));
            }
        }
    }
}
