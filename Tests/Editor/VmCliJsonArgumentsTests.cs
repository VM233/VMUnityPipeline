using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmCliJsonArgumentsTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void LargeFileRetainsExactTextAndNumericIdentity(bool bom)
        {
            FileFixture((root, file) =>
            {
                string payload = new string('x', 120000);
                string json = "{\"payload\":\"" + payload + "\",\"value\":57.21696670694649,\"identity\":9223372036854775807}";
                File.WriteAllText(file, json, new UTF8Encoding(bom));
                Assert.That(VmCliJsonArguments.TryReadSource(null, file, root, out string source,
                    out string code, out string error), Is.True, error);
                Assert.That(source, Is.EqualTo(json));
                Assert.That(VmCliJsonArguments.TryParseObject(source, out var values, out error), Is.True, error);
                Assert.That(values["payload"], Is.EqualTo(payload));
                Assert.That(values["identity"], Is.EqualTo(long.MaxValue));
                Assert.That(BitConverter.DoubleToInt64Bits((double)values["value"]), Is.EqualTo(0x404c9bc590a753f7L));
            });
        }

        [Test]
        public void FileSourceRejectsAmbiguousExternalMissingAndInvalidInputs()
        {
            FileFixture((root, file) =>
            {
                File.WriteAllText(file, "{}");
                Assert.That(VmCliJsonArguments.TryReadSource("{}", file, root, out _, out string code, out _), Is.False);
                Assert.That(code, Is.EqualTo("argument_conflict"));
                foreach (string path in new[] { "relative.json", Path.Combine(Path.GetDirectoryName(root), "outside.json"), Path.Combine(root, "missing.json") })
                {
                    Assert.That(VmCliJsonArguments.TryReadSource(null, path, root, out _, out code, out _), Is.False);
                    Assert.That(code, Is.EqualTo("invalid_arguments_file"));
                }
                File.WriteAllBytes(file, new byte[] { 0xff, 0xfe, 0x41 });
                Assert.That(VmCliJsonArguments.TryReadSource(null, file, root, out _, out code, out _), Is.False);
                Assert.That(code, Is.EqualTo("invalid_arguments_file"));
                File.WriteAllBytes(file, new byte[4 * 1024 * 1024 + 1]);
                Assert.That(VmCliJsonArguments.TryReadSource(null, file, root, out _, out code, out _), Is.False);
                Assert.That(code, Is.EqualTo("input_validation_limit"));
            });
        }

        private static void FileFixture(Action<string, string> test)
        {
            string root = Path.Combine(Path.GetTempPath(), "VMUnityPipeline-arguments-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string file = Path.Combine(root, "Arguments.json");
            try { test(root, file); }
            finally { File.Delete(file); Directory.Delete(root); }
        }

        [TestCase("2026-09-08T20:05:52.800Z")]
        [TestCase("2026-09-09T04:05:52.800+08:00")]
        [TestCase("2026-09-09")]
        public void JsonStringsRetainTheirExactTextAtEveryDepth(string value)
        {
            string json = "{\"since\":\"" + value +
                "\",\"nested\":{\"value\":\"" + value +
                "\"},\"list\":[\"" + value + "\"]}";

            Assert.That(VmCliJsonArguments.TryParseObject(json,
                out Dictionary<string, object> arguments, out string error), Is.True, error);
            Assert.That(arguments["since"], Is.TypeOf<string>().And.EqualTo(value));
            Assert.That(((Dictionary<string, object>)arguments["nested"])["value"],
                Is.TypeOf<string>().And.EqualTo(value));
            Assert.That(((List<object>)arguments["list"])[0],
                Is.TypeOf<string>().And.EqualTo(value));
        }

        [TestCase("{\"since\":1,\"since\":2}")]
        [TestCase("{\"nested\":{\"key\":1,\"key\":2}}")]
        [TestCase("{} {}")]
        [TestCase("{} trailing")]
        [TestCase("[]")]
        [TestCase("null")]
        public void InvalidObjectDocumentsRemainRejected(string json)
        {
            Assert.That(VmCliJsonArguments.TryParseObject(json,
                out Dictionary<string, object> arguments, out string error), Is.False);
            Assert.That(arguments, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void JsonPrimitiveTypesRemainUnchanged()
        {
            Assert.That(VmCliJsonArguments.TryParseObject(
                "{\"integer\":42,\"fraction\":1.25,\"boolean\":true,\"empty\":null}",
                out Dictionary<string, object> arguments, out string error), Is.True, error);
            Assert.That(arguments["integer"], Is.TypeOf<long>().And.EqualTo(42L));
            Assert.That(arguments["fraction"], Is.TypeOf<double>().And.EqualTo(1.25d));
            Assert.That(arguments["boolean"], Is.True);
            Assert.That(arguments["empty"], Is.Null);
        }

        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("\r")]
        public void NumericLexemesRetainTheirExactBitsAcrossNestedSourceLines(string newline)
        {
            string json = "{" + newline + "\"pivot\":57.21696670694649," + newline +
                "\"nested\":{\"small\":5e-324,\"values\":[1,57.21696670694649,-1.0000000000000002]}," +
                newline + "\"text\":\"57.21696670694649\"}";
            Assert.That(VmCliJsonArguments.TryParseObject(json, out Dictionary<string, object> arguments,
                out string error), Is.True, error);
            Assert.That(BitConverter.DoubleToInt64Bits((double)arguments["pivot"]), Is.EqualTo(0x404c9bc590a753f7L));
            var nested = (Dictionary<string, object>)arguments["nested"];
            Assert.That(BitConverter.DoubleToInt64Bits((double)nested["small"]), Is.EqualTo(1L));
            var values = (List<object>)nested["values"];
            Assert.That(values[0], Is.TypeOf<long>().And.EqualTo(1L));
            Assert.That(BitConverter.DoubleToInt64Bits((double)values[1]), Is.EqualTo(0x404c9bc590a753f7L));
            Assert.That(BitConverter.DoubleToInt64Bits((double)values[2]),
                Is.EqualTo(unchecked((long)0xbff0000000000001UL)));
            Assert.That(arguments["text"], Is.TypeOf<string>().And.EqualTo("57.21696670694649"));
        }

        [TestCase("{\"value\":NaN}")]
        [TestCase("{\"value\":Infinity}")]
        [TestCase("{\"value\":1e309}")]
        [TestCase("{\"value\":010}")]
        public void NonFiniteNumericArgumentsRemainRejected(string json)
        {
            Assert.That(VmCliJsonArguments.TryParseObject(json, out Dictionary<string, object> arguments,
                out string error), Is.False);
            Assert.That(arguments, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }
    }
}
